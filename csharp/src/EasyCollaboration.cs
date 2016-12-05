using System;
using System.Collections.Generic;
using System.Reflection;
using Ch.Elca.Iiop.Idl;
using log4net;
using omg.org.CORBA;
using tecgraf.openbus.core.v2_0.services.access_control;
using tecgraf.openbus.core.v2_0.services.offer_registry;
using tecgraf.openbus.services.collaboration.v1_0;
using IComponent = scs.core.IComponent;

//TODO: Essa classe precisa ser totalmente revista em termos de robustez. As chamadas remotas precisam ser avaliadas sobre suas exceções e tratamentos (ex: se várias chamadas sao feitas, como o usuario sabe ate que ponto foi feito?).
//TODO: pensar em oferecer thread safety (atualmente está parcial...)

namespace tecgraf.openbus.services.collaboration.easy{
  public class EasyCollaboration : IEasyCollaboration{
    #region Fields

    private static readonly ILog Logger =
      LogManager.GetLogger(typeof (EasyCollaboration));

    private readonly OpenBusContext _context;
    private SessionRegistry _sessions;
    private CollaborationRegistry _collabs;
    private CollaborationSession _theSession;
    private Consumer _servant;
    private SessionObserver _observer;
    private EventConsumer _consumer;
    private int _subsId;
    private int _obsId;

    #endregion

    public EasyCollaboration (OpenBusContext context){
      _context = context;
    }

    public CollaborationSession StartCollaboration() {
      Logger.Info("Starting collaboration");
      SessionRegistry sreg = GetSessions();
      try{
        _theSession = sreg.getSession();
        Logger.Info("Session retrieved: " + OrbServices.GetSingleton().object_to_string(_theSession));
      }
      catch (TargetInvocationException e){
        if (e.InnerException is SessionDoesNotExist)
          Logger.Warn("Session not found for entity " + (e.InnerException as SessionDoesNotExist).entity);
      }
      catch (Exception e){
        Logger.Error("Error trying to obtain session: " + e);
        throw;
      }
      if (_theSession == null) {
        try{
          CollaborationRegistry collab = GetCollabs();
          _theSession = collab.createCollaborationSession();
          sreg.registerSession(_theSession);
        }
        catch (Exception e) {
          Logger.Error("Error creating the session (it will be destroyed): " + e);
          if (_theSession != null) 
            _theSession.destroy();
          throw;
        }
      }
      try{
        _obsId = _theSession.subscribeObserver(BuildObserver());
        Logger.Info("Observer subscribed");
        _consumer = BuildConsumer();
        _subsId = _theSession.channel.subscribe(_consumer);
        Logger.Info("Consumer registered");
      }
      catch (Exception e) {
        if (_obsId != 0) 
          _theSession.unsubscribeObserver(_obsId);
        Logger.Error("Error subscribing to the channel (the session observer will be removed): " + e);
        throw;
      }
      return _theSession;
    }

    public void ExitCollaboration(){
      try {
        _theSession.channel.unsubscribe(_subsId);
        _theSession.unsubscribeObserver(_obsId);
        Logger.Info("Collaboration finished");
      }
      catch (Exception e) {
        Logger.Error("Error unsubscribing to the channel or the session: " + e);
      }
      finally {
        //TODO desativar objetos corba
        _subsId = 0;
        _obsId = 0;
        _theSession = null;
        _servant = null;
        _consumer = null;
        _observer = null;
        _collabs = null;
        _sessions = null;
      }
    }

    public void ShareDataKeys(List<byte[]> keys){
      omg.org.CORBA.TypeCode byteTC = OrbServices.GetSingleton().create_octet_tc();
      omg.org.CORBA.TypeCode sequenceTC = OrbServices.GetSingleton().create_sequence_tc(0, byteTC);
      omg.org.CORBA.TypeCode arrayTC = OrbServices.GetSingleton().create_array_tc(keys.Count, sequenceTC);
      Share(new Any(keys.ToArray(), arrayTC));
    }

    public void Share(Any any){
      try {
        _theSession.channel.push(any);
      }
      catch (Exception e) {
        Logger.Error("Error trying to push an object to the channel: " + e);
        throw;
      }
    }

    public List<byte[]> ConsumeDataKeys(){
      lock (_servant.Keys) {
        List<byte[]> list = new List<byte[]>(_servant.Keys);
        _servant.Keys.Clear();
        return list;
      }
    }

    public List<object> ConsumeAnys(){
      lock (_servant.Anys) {
        List<object> list = new List<object>(_servant.Anys);
        _servant.Anys.Clear();
        return list;
      }
    }

  private CollaborationObserver BuildObserver() {
    LoginInfo? info = _context.GetCurrentConnection().Login;
    if (!info.HasValue) {
      throw new NO_PERMISSION(NoLoginCode.ConstVal, CompletionStatus.Completed_No);
    }
    _observer = new SessionObserver(info.Value.entity);
    OrbServices.ActivateObject(_observer);
    return _observer;
  }
  
  private EventConsumer BuildConsumer() {
    _servant = new Consumer();
    OrbServices.ActivateObject(_servant);
    return _servant;
  }

  private SessionRegistry GetSessions(){
    bool find = _sessions == null;
    if (!find) {
      try{
        find = !_context.ORB.non_existent(_sessions);
      }
      catch (Exception){
        find = true;
      }
    }
    if (find) {
      ServiceProperty[] serviceProperties = new ServiceProperty[1];
      string sessionRegType = Repository.GetRepositoryID(typeof(SessionRegistry));
      serviceProperties[0] =
        new ServiceProperty("openbus.component.interface", sessionRegType);
      ServiceOfferDesc[] services = _context.OfferRegistry.findServices(serviceProperties);

      foreach (ServiceOfferDesc offerDesc in services) {
        try {
          MarshalByRefObject obj =
            offerDesc.service_ref.getFacet(sessionRegType);
          if (obj == null) {
            continue;
          }
          _sessions = obj as SessionRegistry;
          if (_sessions != null) {
            break; // found one
          }
        }
        catch (Exception e) {
          NO_PERMISSION npe = null;
          if (e is TargetInvocationException) {
            npe = e.InnerException as NO_PERMISSION;
          }
          // caso não seja uma NO_PERMISSION{NoLogin} descarta essa oferta.
          if ((npe == null) && (!(e is NO_PERMISSION))) {
            continue;
          }
          npe = npe ?? e as NO_PERMISSION;
          switch (npe.Minor) {
            case NoLoginCode.ConstVal:
              throw;
          }
        }
      }
    }
    return _sessions;
  }

    private CollaborationRegistry GetCollabs(){
    bool find = _collabs == null;
    if (!find) {
      try{
        find = !_context.ORB.non_existent(_collabs);
      }
      catch (Exception){
        find = true;
      }
    }
    if (find) {
      ServiceProperty[] serviceProperties = new ServiceProperty[1];
      string collabsRegType = Repository.GetRepositoryID(typeof(CollaborationRegistry));
      serviceProperties[0] =
        new ServiceProperty("openbus.component.interface", collabsRegType);
      ServiceOfferDesc[] services = _context.OfferRegistry.findServices(serviceProperties);

      foreach (ServiceOfferDesc offerDesc in services) {
        try {
          MarshalByRefObject obj =
            offerDesc.service_ref.getFacet(collabsRegType);
          if (obj == null) {
            continue;
          }
          _collabs = obj as CollaborationRegistry;
          if (_collabs != null) {
            break; // found one
          }
        }
        catch (Exception e) {
          NO_PERMISSION npe = null;
          if (e is TargetInvocationException) {
            npe = e.InnerException as NO_PERMISSION;
          }
          // caso não seja uma NO_PERMISSION{NoLogin} descarta essa oferta.
          if ((npe == null) && (!(e is NO_PERMISSION))) {
            continue;
          }
          npe = npe ?? e as NO_PERMISSION;
          switch (npe.Minor) {
            case NoLoginCode.ConstVal:
              throw;
          }
        }
      }
    }
    return _collabs;
  }

  /// <summary>
  /// Consumidor simplificado para o canais de eventos de sessões
  /// </summary>
  private class Consumer : MarshalByRefObject, EventConsumer {
    public readonly IList<byte[]> Keys;
    public readonly IList<object> Anys;

    public Consumer() {
      Keys = new List<byte[]>();
      Anys = new List<object>();
    }

    public void push(object e) {
      Logger.Info("Received event");
      if (e is byte[]) {
        Keys.Add((byte[]) e);
      }
      else if (e is byte[][]) {
        foreach (byte[] item in (byte[][]) e) {
          Keys.Add(item);
        }
      }
      else
      {
        Anys.Add(e);
      }
    }

    public override object InitializeLifetimeService()
    {
      // Evita a desativação automática pela política de 
      // ciclo de vida do MarshalByRefObject
      return null;
    }
  }
  
  /**
   * Observador simplificado para sessões de colaboração.
   * 
   * 
   * @author Tecgraf/PUC-Rio
   *
   */
  class SessionObserver : MarshalByRefObject, CollaborationObserver {

    protected String Entity;

    public SessionObserver(String entity) {
      Entity = entity;
    }

    public void memberAdded(string name, IComponent member) {
      Logger.Info("Member added: " + name);
    }

    public void memberRemoved(string name) {
      Logger.Info("Member removed: " + name);
    }

    public void destroyed() {
      Logger.Info("Session destroyed");
    }

    public override object InitializeLifetimeService()
    {
      // Evita a desativação automática pela política de 
      // ciclo de vida do MarshalByRefObject
      return null;
    }
  }

  }
}