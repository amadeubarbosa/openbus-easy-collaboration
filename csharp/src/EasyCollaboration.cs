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
using System.Threading;

namespace tecgraf.openbus.services.collaboration.easy
{
  public class EasyCollaboration : IEasyCollaboration
  {
    #region Fields

    private static readonly ILog Logger =
      LogManager.GetLogger(typeof(EasyCollaboration));

    private OpenBusContext _context;
    private SessionRegistry _sessions;
    private CollaborationRegistry _collabs;
    private CollaborationSession _theSession;

    private CollaborationObserver _observer;
    private EventConsumer _consumer;
    private int _subsId;
    private int _obsId;

    private readonly object _locker = new object();

    #endregion

    /// <summary>
    /// Construtor padrão, nesse caso a biblioteca vai usar implementações 
    /// básicas de <see cref="EventConsumer"/> e <see cref="CollaborationObserver"/>.
    /// <seealso cref="EasyCollaboration.Consumer"/>
    /// </summary>
    /// <param name="context">contexto do OpenBus, objeto responsável pelo gerenciamento de conexões</param>
    public EasyCollaboration(OpenBusContext context)
    {
      _context = context;
      _observer = new EasyCollaboration.Observer();
      _consumer = new EasyCollaboration.Consumer();
    }

    /// <summary>
    /// Construtor opcional para aplicação fornecer um objeto para observação da sessão.
    /// <para>Esse construtor é opcional porque a aplicação sempre pode adicionar outros consumidores ou outros observadores no <see cref="CollaborationSession"/> quando inicia a colaboração através do <see cref="EasyCollaboration.StartCollaboration()"/>.</para>
    /// </summary>
    /// <param name="context">contexto do OpenBus, objeto responsável pelo gerenciamento de conexões</param>
    /// <param name="observer">observador da sessão de colaboração</param>
    /// <exception cref="ArgumentException">caso o <c>observer</c> não implemente <see cref="MarshalByRefObject"/> além da interface <see cref="CollaborationObserver"/></exception>
    public EasyCollaboration(OpenBusContext context, CollaborationObserver observer)
      : this(context)
    {
      if ((observer != null) && ! (observer is MarshalByRefObject))
        throw new ArgumentException("CollaborationObserver object must implement MarshalByRefObject also", "observer");
      _observer = observer;
    }

    /// <summary>
    /// Construtor opcional para aplicação fornecer tanto o consumidor quanto o observador da sessão.
    /// <para>Esse construtor é opcional porque a aplicação sempre pode adicionar outros consumidores ou outros observadores no <see cref="CollaborationSession"/> quando inicia a colaboração através do <see cref="EasyCollaboration.StartCollaboration()"/>.</para>
    /// </summary>
    /// <param name="context">contexto do OpenBus, objeto responsável pelo gerenciamento de conexões</param>
    /// <param name="consumer">consumidor do canal de eventos</param>
    /// <param name="observer">observador da sessão de colaboração</param>
    /// <exception cref="ArgumentException">caso o <c>consumer</c> não implemente <see cref="MarshalByRefObject"/> além da interface <see cref="EventConsumer"/></exception>
    /// <exception cref="ArgumentException">caso o <c>observer</c> não implemente <see cref="MarshalByRefObject"/> além da interface <see cref="CollaborationObserver"/></exception>
    public EasyCollaboration(OpenBusContext context, EventConsumer consumer, CollaborationObserver observer)
      : this(context, observer)
    {
      if ((consumer != null) && ! (consumer is MarshalByRefObject))
        throw new ArgumentException("EventConsumer object must implement MarshalByRefObject also", "consumer");
      _consumer = consumer;
    }

    public CollaborationSession StartCollaboration()
    {
      Logger.Info("Starting collaboration");

      lock (_locker)
      {
        SessionRegistry sreg = GetSessions();
        try
        {
          _theSession = sreg.getSession();
          Logger.Info("Session retrieved: " + OrbServices.GetSingleton().object_to_string(_theSession));
        }
        catch (TargetInvocationException e)
        {
          if (e.InnerException is SessionDoesNotExist)
            Logger.Warn("Session not found for entity " + (e.InnerException as SessionDoesNotExist).entity);
        }
        catch (Exception e)
        {
          Logger.Error("Error trying to obtain session: " + e);
          throw;
        }

        if (_theSession == null)
        {
          try
          {
            CollaborationRegistry collab = GetCollabs();
            _theSession = collab.createCollaborationSession();
            sreg.registerSession(_theSession);
          }
          catch (Exception e)
          {
            Logger.Error("Error creating the session (it will be destroyed): " + e);
            if (_theSession != null)
              _theSession.destroy();
            throw;
          }
        }
        UpdateActivation();
      }
      return _theSession;
    }

    private void UpdateActivation()
    {
      if (_observer != null)
      {
        try
        {
          DeactivateObserver();
        }
        catch (Exception e)
        {
          Logger.Warn("Failed to deactivate previously activated observer: " + e);
        }
        try
        {
          OrbServices.ActivateObject((MarshalByRefObject)_observer);
          _obsId = _theSession.subscribeObserver(_observer);
          Logger.Info("Observer subscribed");
        }
        catch (Exception e)
        {
          if (_obsId != 0)
            _theSession.unsubscribeObserver(_obsId);
          Logger.Error("Error subscribing to the session: " + e);
          throw;
        }
      }
      if (_consumer != null)
      {
        try
        {
          DeactivateConsumer();
        }
        catch (Exception e)
        {
          Logger.Warn("Failed to deactivate previously activated consumer: " + e);
        }
        try
        {
          OrbServices.ActivateObject((MarshalByRefObject)_consumer);
          _subsId = _theSession.channel.subscribe(_consumer);
          Logger.Info("Consumer registered");
        }
        catch (Exception e)
        {
          if (_subsId != 0)
            _theSession.channel.unsubscribe(_subsId);
          Logger.Error("Error subscribing to the channel: " + e);
          throw;
        }
      }
    }

    private void DeactivateConsumer()
    {
      if (_subsId != 0)
      {
        _theSession.channel.unsubscribe(_subsId);
        OrbServices.DeactivateObject((MarshalByRefObject)_consumer);
        _subsId = 0;
      }
    }

    private void DeactivateObserver()
    {
      if (_obsId != 0)
      {
        _theSession.unsubscribeObserver(_obsId);
        OrbServices.DeactivateObject((MarshalByRefObject)_observer);
        _obsId = 0;
      }
    }

    public void ExitCollaboration()
    {
      lock (_locker)
      {
        try
        {
          DeactivateConsumer();
          DeactivateObserver();
          Logger.Info("Collaboration finished");
        }
        finally
        {
          _subsId = 0;
          _obsId = 0;
          _theSession = null;
          _consumer = null;
          _observer = null;
          _collabs = null;
          _sessions = null;
        }
      }
    }

    public void ShareDataKeys(List<byte[]> keys)
    {
      omg.org.CORBA.TypeCode byteTC = OrbServices.GetSingleton().create_octet_tc();
      omg.org.CORBA.TypeCode sequenceTC = OrbServices.GetSingleton().create_sequence_tc(0, byteTC);
      omg.org.CORBA.TypeCode arrayTC = OrbServices.GetSingleton().create_array_tc(keys.Count, sequenceTC);
      Share(new Any(keys.ToArray(), arrayTC));
    }

    public void Share(Any any)
    {
      try
      {
        _theSession.channel.push(any);
      }
      catch (Exception e)
      {
        Logger.Error("Error trying to push an object to the channel: " + e);
        throw;
      }
    }

    public List<byte[]> ConsumeDataKeys()
    {
      List<byte[]> list = new List<byte[]>();
      if (_consumer is Consumer)
      {
        var obj = (_consumer as Consumer);
        lock (obj.Keys)
        {
          list.AddRange(obj.Keys);
          obj.Keys.Clear();
        }
      }
      return list;
    }

    public List<object> ConsumeAnys()
    {
      List<object> list = new List<object>();
      if (_consumer is Consumer)
      {
        var obj = (_consumer as Consumer);
        lock (obj.Anys)
        {
          list.AddRange(obj.Anys);
          obj.Anys.Clear();
        }
      }
      return list;
    }

    private SessionRegistry GetSessions()
    {
      bool find = _sessions == null;
      if (!find)
      {
        try
        {
          find = !_context.ORB.non_existent(_sessions);
        }
        catch (Exception)
        {
          find = true;
        }
      }
      if (find)
      {
        ServiceProperty[] serviceProperties = new ServiceProperty[1];
        string sessionRegType = Repository.GetRepositoryID(typeof(SessionRegistry));
        serviceProperties[0] =
          new ServiceProperty("openbus.component.interface", sessionRegType);
        ServiceOfferDesc[] services = _context.OfferRegistry.findServices(serviceProperties);

        foreach (ServiceOfferDesc offerDesc in services)
        {
          try
          {
            MarshalByRefObject obj =
              offerDesc.service_ref.getFacet(sessionRegType);
            if (obj == null)
            {
              continue;
            }
            _sessions = obj as SessionRegistry;
            if (_sessions != null)
            {
              break; // found one
            }
          }
          catch (Exception e)
          {
            NO_PERMISSION npe = null;
            if (e is TargetInvocationException)
            {
              npe = e.InnerException as NO_PERMISSION;
            }
            // caso não seja uma NO_PERMISSION{NoLogin} descarta essa oferta.
            if ((npe == null) && (!(e is NO_PERMISSION)))
            {
              continue;
            }
            npe = npe ?? e as NO_PERMISSION;
            switch (npe.Minor)
            {
              case NoLoginCode.ConstVal:
                throw;
            }
          }
        }
      }
      return _sessions;
    }

    private CollaborationRegistry GetCollabs()
    {
      bool find = _collabs == null;
      if (!find)
      {
        try
        {
          find = !_context.ORB.non_existent(_collabs);
        }
        catch (Exception)
        {
          find = true;
        }
      }
      if (find)
      {
        ServiceProperty[] serviceProperties = new ServiceProperty[1];
        string collabsRegType = Repository.GetRepositoryID(typeof(CollaborationRegistry));
        serviceProperties[0] =
          new ServiceProperty("openbus.component.interface", collabsRegType);
        ServiceOfferDesc[] services = _context.OfferRegistry.findServices(serviceProperties);

        foreach (ServiceOfferDesc offerDesc in services)
        {
          try
          {
            MarshalByRefObject obj =
              offerDesc.service_ref.getFacet(collabsRegType);
            if (obj == null)
            {
              continue;
            }
            _collabs = obj as CollaborationRegistry;
            if (_collabs != null)
            {
              break; // found one
            }
          }
          catch (Exception e)
          {
            NO_PERMISSION npe = null;
            if (e is TargetInvocationException)
            {
              npe = e.InnerException as NO_PERMISSION;
            }
            // caso não seja uma NO_PERMISSION{NoLogin} descarta essa oferta.
            if ((npe == null) && (!(e is NO_PERMISSION)))
            {
              continue;
            }
            npe = npe ?? e as NO_PERMISSION;
            switch (npe.Minor)
            {
              case NoLoginCode.ConstVal:
                throw;
            }
          }
        }
      }
      return _collabs;
    }

    /// <summary>
    /// Consumidor simplificado para o canais de eventos de sessões com suporte a
    /// consumir byte[] e byte[][] como DataKeys.
    /// </summary>
    public class Consumer : MarshalByRefObject, EventConsumer
    {
      public readonly IList<byte[]> Keys;
      public readonly IList<object> Anys;

      public Consumer()
      {
        Keys = new List<byte[]>();
        Anys = new List<object>();
      }
      /// <summary>
      /// Recebe um evento encapsulado em um <c>omg.org.CORBA.Any</c> e acumula os eventos 
      /// para serem consumidos através do <see cref="EasyCollaboration.ConsumeDataKeys()"/> 
      /// ou <see cref="EasyCollaboration.ConsumeAnys()"/>.
      /// <para>Para estar disponível no <see cref="EasyCollaboration.ConsumeDataKeys()"/>,
      /// o evento precisa ser ou uma sequ~encia de bytes (byte[]) ou um array de uma sequência
      /// de bytes (byte[][]). Caso o evento seja de qualquer outro tipo estará disponível
      /// no <see cref="EasyCollaboration.ConsumeAnys()"/>.</para>
      /// </summary>
      /// <param name="e">evento enviado para o <see cref="EventChannel"/> contido na <see cref="CollaborationSession"/></param>
      public void push(object e)
      {
        Logger.Info("Received event type: " + e.GetType().ToString());
        if (e is byte[])
        {
          Keys.Add((byte[])e);
        }
        else if (e is byte[][])
        {
          foreach (byte[] item in (byte[][])e)
          {
            Keys.Add(item);
          }
        }
        else
        {
          Anys.Add(e);
        }
      }
      /// <summary>
      /// Configura a política de ciclo de vida do MarshalByRefObject.
      /// </summary>
      /// <returns>sempre retorna null</returns>
      public override object InitializeLifetimeService()
      {
        // Evita a desativação automática pela política de ciclo de vida do MarshalByRefObject
        return null;
      }
    }

    /// <summary>
    /// Observador simplificado para sessões de colaboração.
    /// </summary>
    public class Observer : MarshalByRefObject, CollaborationObserver
    {

      public void memberAdded(string name, IComponent member)
      {
        Logger.Info("Member added: " + name);
      }

      public void memberRemoved(string name)
      {
        Logger.Info("Member removed: " + name);
      }

      public void destroyed()
      {
        Logger.Info("Session destroyed");
      }

      /// <summary>
      /// Configura a política de ciclo de vida do MarshalByRefObject.
      /// </summary>
      /// <returns>sempre retorna null</returns>
      public override object InitializeLifetimeService()
      {
        // Evita a desativação automática pela política de ciclo de vida do MarshalByRefObject
        return null;
      }
    }
  }
}