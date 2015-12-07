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

namespace tecgraf.openbus.easycollab{
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
        Logger.Info("Session retrieved: " + _theSession);
      }
      catch (SessionDoesNotExist e){
        Logger.Warn("Session not found for entity " + e.entity);
      }
      catch (Exception e){
        Logger.Error("Error trying to obtain session: " + e);
        throw;
      }
      if (_theSession == null) {
        try{
          CollaborationRegistry collab = GetCollabs();
          _theSession = collab.createCollaborationSession();
        }
        catch (Exception e){
          Logger.Error("Error creating session: " + e);
          throw;
        }
        try{
          sreg.registerSession(_theSession);
        }
        catch (Exception e) {
          Logger.Error("Error registering created session (it will be removed): " + e);
          throw;
        }
        finally{
          try{
            _theSession.destroy();
            Logger.Warn("Created session destroyed because it wasn't registered.");
          }
          catch (Exception e){
            Logger.Error("Error destroying created session, contact the administrator to destroy it: " + e);
            throw;
          }
        }
      }
      try{
        _obsId = _theSession.subscribeObserver(BuildObserver());
        Logger.Info("Observer subscribed");
      }
      catch (Exception e) {
        Logger.Error("Error subscribing observer: " + e);
        throw;
      }
      try {
        _consumer = BuildConsumer();
        _subsId = _theSession.channel.subscribe(_consumer);
        Logger.Info("Consumer registered");
      }
      catch (Exception e) {
        Logger.Error("Error subscribing to the channel (the session observer will be removed): " + e);
        throw;
      }
      finally{
        try{
          _theSession.unsubscribeObserver(_obsId);
          Logger.Warn("Observer unsubscribed because it wasn't possible to subscribe to the channel.");
        }
        catch (Exception e){
          Logger.Error("Error unsubscribing observer and the subscription to the channel was not made. You should leave and rejoin this session or create a new one: " + e);
          throw;
        }
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
      foreach (byte[] key in keys){
        Share(key);
      }
    }

    public void Share(object any){
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

    public List<object> ConsumeAny(){
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
      else {
        Anys.Add(e);
      }
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
  }

  }
}