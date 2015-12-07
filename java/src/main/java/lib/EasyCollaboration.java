package lib;

import java.util.Collections;
import java.util.LinkedList;
import java.util.List;
import java.util.logging.Logger;

import org.omg.CORBA.Any;
import org.omg.CORBA.ORB;
import org.omg.CORBA.OctetSeqHelper;
import org.omg.CORBA.ORBPackage.InvalidName;
import org.omg.PortableServer.POA;
import org.omg.PortableServer.POAHelper;

import scs.core.IComponent;
import tecgraf.openbus.OpenBusContext;
import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.core.v2_0.services.offer_registry.ServiceOfferDesc;
import tecgraf.openbus.core.v2_0.services.offer_registry.ServiceProperty;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationObserver;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationObserverHelper;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationObserverPOA;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationRegistry;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationRegistryHelper;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationSession;
import tecgraf.openbus.services.collaboration.v1_0.EventConsumer;
import tecgraf.openbus.services.collaboration.v1_0.EventConsumerHelper;
import tecgraf.openbus.services.collaboration.v1_0.EventConsumerPOA;
import tecgraf.openbus.services.collaboration.v1_0.SessionDoesNotExist;
import tecgraf.openbus.services.collaboration.v1_0.SessionRegistry;
import tecgraf.openbus.services.collaboration.v1_0.SessionRegistryHelper;

public class EasyCollaboration implements IEasyCollaboration {

  private OpenBusContext context;
  private SessionRegistry sessions;
  private CollaborationRegistry collabs;
  private CollaborationSession theSession;
  private Consumer servant;
  private SessionObserver observer;
  private EventConsumer consumer;
  private int subsId;
  private int obsId;
  
  /**
   * O <i>logger</i>.
   */
  private static Logger logger = Logger
    .getLogger(EasyCollaboration.class.getName());

  public EasyCollaboration(OpenBusContext context) {
    this.context = context;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public CollaborationSession startCollaboration() throws ServiceFailure {
    logger.info("Starting collaboration");
    SessionRegistry sreg = getSessions();
    try {
      theSession = sreg.getSession();
      logger.info("Session retrieved: " + theSession);
    }
    catch (SessionDoesNotExist e) {
      logger.warning("Session not found for entity " + e.entity);
    }
    catch (tecgraf.openbus.core.v2_0.services.ServiceFailure e) {
      throw new ServiceFailure(e.getMessage());
    }
    catch (Throwable t) {
      logger.severe("Unknown error: " + t.getMessage());
      t.printStackTrace();
    }
    try {
      if (theSession == null) {
        CollaborationRegistry collab = getCollabs();
        theSession = collab.createCollaborationSession();
        sreg.registerSession(theSession);
      }

      obsId = theSession.subscribeObserver(buildObserver());
      logger.info("Observer subscribed");

      consumer = buildConsumer();
      subsId = theSession.channel().subscribe(consumer);
      logger.info("Consumer registered");
    }
    catch (tecgraf.openbus.core.v2_0.services.ServiceFailure ex) {
      throw new ServiceFailure(ex.getMessage());
    } 
    return theSession;

  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void exitCollaboration() throws ServiceFailure {
    try {
      theSession.channel().unsubscribe(subsId);
      theSession.unsubscribeObserver(obsId);
      logger.info("Collaboration finished");
    }
    catch (tecgraf.openbus.core.v2_0.services.ServiceFailure e) {
      throw new ServiceFailure(e.getMessage());
    }
    finally {
      
      subsId = 0;
      obsId = 0;
      theSession = null;
      servant = null;
      consumer = null;
      observer = null;
      collabs = null;
      sessions = null;
    }
  }

  private CollaborationObserver buildObserver() throws ServiceFailure {
    try {
      ORB orb = context.orb();
      observer = new SessionObserver(context.getCurrentConnection().login().entity);
      POA poa = POAHelper.narrow(orb.resolve_initial_references("RootPOA"));
      poa.the_POAManager().activate();
      byte[] id = poa.activate_object(observer);
      CollaborationObserver ref = CollaborationObserverHelper.narrow(poa.id_to_reference(id));
      return ref;      
    }
    catch (Exception e) {
      // should never happen
      throw new ServiceFailure(e.getLocalizedMessage(),
          "Error while session observer activation");
    }
  }
  
  private EventConsumer buildConsumer() throws ServiceFailure {
    try {
      ORB orb = context.orb();
      servant = new Consumer();
      POA poa = POAHelper.narrow(orb.resolve_initial_references("RootPOA"));
      poa.the_POAManager().activate();
      byte[] id = poa.activate_object(servant);
      EventConsumer ref = EventConsumerHelper.narrow(poa.id_to_reference(id));
      return ref;
    }
    catch (Exception e) {
      // should never happen
      throw new ServiceFailure(e.getLocalizedMessage(),
        "Error while event consumer activation");
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void shareDataKey(byte[] key) throws ServiceFailure {
    try {
      Any any = context.orb().create_any();
      OctetSeqHelper.insert(any, key);
      theSession.channel().push(any);
    }
    catch (tecgraf.openbus.core.v2_0.services.ServiceFailure e) {
      throw new ServiceFailure(e.getMessage());
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void shareDataKeys(List<byte[]> keys) throws ServiceFailure {
    try {
      for(byte[] key: keys) {
        Any any = context.orb().create_any();
        OctetSeqHelper.insert(any, key);
        theSession.channel().push(any);
      }
    }
    catch (tecgraf.openbus.core.v2_0.services.ServiceFailure e) {
      throw new ServiceFailure(e.getMessage());
    }
  }
  
  /**
   * {@inheritDoc}
   */
  @Override
  public void shareAny(Any any) throws ServiceFailure {
    try {
      theSession.channel().push(any);
    }
    catch (tecgraf.openbus.core.v2_0.services.ServiceFailure e) {
      throw new ServiceFailure(e.getMessage());
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public List<byte[]> consumeDataKeys() {
    synchronized (servant.keys) {
      LinkedList<byte[]> list = new LinkedList<byte[]>(servant.keys);
      servant.keys.clear();
      return list;
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public List<Any> consumeAnys() {
    synchronized (servant.anys) {
      LinkedList<Any> list = new LinkedList<Any>(servant.anys);
      servant.anys.clear();
      return list;
    }
  }

  private SessionRegistry getSessions() throws ServiceFailure {
    if (sessions == null || sessions._non_existent()) {
      ServiceProperty[] serviceProperties = new ServiceProperty[1];
      serviceProperties[0] =
        new ServiceProperty("openbus.component.interface",
          SessionRegistryHelper.id());
      List<ServiceOfferDesc> services =
        LibUtils.findOffer(context.getOfferRegistry(), serviceProperties, 1,
          10, 1);

      for (ServiceOfferDesc offerDesc : services) {
        org.omg.CORBA.Object obj =
          offerDesc.service_ref.getFacet(SessionRegistryHelper.id());
        if (obj == null) {
          continue;
        }
        sessions = SessionRegistryHelper.narrow(obj);
        if (sessions != null) {
          break; // found one
        }
      }
    }
    return sessions;
  }

  private CollaborationRegistry getCollabs() throws ServiceFailure {
    if (collabs == null || collabs._non_existent()) {
      ServiceProperty[] serviceProperties = new ServiceProperty[1];
      serviceProperties[0] =
        new ServiceProperty("openbus.component.interface",
          CollaborationRegistryHelper.id());
      List<ServiceOfferDesc> services =
        LibUtils.findOffer(context.getOfferRegistry(), serviceProperties, 1,
          10, 1);

      for (ServiceOfferDesc offerDesc : services) {
        org.omg.CORBA.Object obj =
          offerDesc.service_ref.getFacet(CollaborationRegistryHelper.id());
        if (obj == null) {
          continue;
        }
        collabs = CollaborationRegistryHelper.narrow(obj);
        if (collabs != null) {
          break; // found one
        }
      }
    }
    return collabs;
  }

  /**
   * Consumidor simplificado para o canais de eventos de sessões
   * 
   * @author Tecgraf/PUC-Rio
   *
   */
  class Consumer extends EventConsumerPOA {

    private List<byte[]> keys;
    private List<Any> anys;

    public Consumer() {
      this.keys = Collections.synchronizedList(new LinkedList<byte[]>());
      this.anys = Collections.synchronizedList(new LinkedList<Any>());
    }

    @Override
    public void push(Any event)
      throws tecgraf.openbus.core.v2_0.services.ServiceFailure {
      logger.info("Received event");
      if (event.type().equal(OctetSeqHelper.type())) {
        keys.add(OctetSeqHelper.extract(event));
      }
      else {
        anys.add(event);
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
  class SessionObserver extends CollaborationObserverPOA {

    private String entity;

    public SessionObserver(String entity) {
      this.entity = entity;
    }

    @Override
    public void memberAdded(String name, IComponent member) throws tecgraf.openbus.core.v2_0.services.ServiceFailure{
      logger.info("Member added: " + name);
    }

    @Override
    public void memberRemoved(String name) throws tecgraf.openbus.core.v2_0.services.ServiceFailure {
      logger.info("Member removed: " + name);
    }

    @Override
    public void destroyed() throws tecgraf.openbus.core.v2_0.services.ServiceFailure {
      logger.info("Session destroyed");
    }
  }

}
