package tecgraf.openbus.services.collaboration.easy;

import org.junit.AfterClass;
import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.omg.CORBA.Any;
import org.omg.CORBA.ORB;
import scs.core.IComponent;
import tecgraf.openbus.Connection;
import tecgraf.openbus.OpenBusContext;
import tecgraf.openbus.core.ORBInitializer;
import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationObserverPOA;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationSession;
import tecgraf.openbus.services.collaboration.v1_0.EventConsumerPOA;

import java.util.concurrent.CountDownLatch;

public class EasyCollaborationTest {

  private static TestConfigs configs;
  private static OpenBusContext context;
  private CollaborationSession session;

  @BeforeClass
  public static void setup() {
    try {
      configs = TestConfigs.readConfigsFile();
      ORB orb = ORBInitializer.initORB();
      context = (OpenBusContext) orb.resolve_initial_references("OpenBusContext");
      Connection connection = context.createConnection(configs.bushost, configs.busport);
      context.setDefaultConnection(connection);
      connection.loginByPassword(configs.user, configs.password);
    } catch (Exception e) {
      Assert.fail(e.getMessage());
    }
  }

  @AfterClass
  public static void teardown() {
    try {
      context.getDefaultConnection().logout();
    } catch (Exception e) {
      Assert.fail(e.getMessage());
    }
  }

  @Test
  public void multipleStartCallInSameReference() throws Exception {
    EasyCollaboration easy = new EasyCollaboration(context);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    Assert.assertEquals(session, easy.startCollaboration());
    Assert.assertEquals(session, easy.startCollaboration());
    Assert.assertEquals(session, easy.startCollaboration());
    Assert.assertEquals(session, easy.startCollaboration());
    easy.exitCollaboration();
    easy = new EasyCollaboration(context, null, null);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    Assert.assertEquals(session, easy.startCollaboration());
    Assert.assertEquals(session, easy.startCollaboration());
    Assert.assertEquals(session, easy.startCollaboration());
    Assert.assertEquals(session, easy.startCollaboration());
    easy.exitCollaboration();
  }

  @Test
  public void noCollaborationExitTwice() throws Exception {
    EasyCollaboration easy = new EasyCollaboration(context);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    easy.exitCollaboration();
    try {
      easy.exitCollaboration();
      Assert.fail("Exit collaboration called twice doesn't throw an exception");
    } catch (NullPointerException e) {
      // expected only after first exit call
    }
  }

  @Test
  public void onlyDefaultShouldReceiveEvents() throws Exception {
    EasyCollaboration easy = new EasyCollaboration(context);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    byte[] simulation = {10,20,30,40};
    easy.shareDataKey(simulation);
    Thread.sleep(configs.sleeptime);
    Assert.assertEquals(1, easy.consumeDataKeys().size());
    Any any = context.orb().create_any();
    any.insert_double(10.20);
    easy.shareAny(any);
    Thread.sleep(configs.sleeptime);
    Assert.assertEquals(1, easy.consumeAnys().size());
    easy.exitCollaboration();
    easy = new EasyCollaboration(context, null, null);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    easy.shareDataKey(simulation);
    Thread.sleep(configs.sleeptime);
    Assert.assertEquals(0, easy.consumeDataKeys().size());
    easy.shareAny(any);
    Thread.sleep(configs.sleeptime);
    Assert.assertEquals(0, easy.consumeAnys().size());
  }

  @Test
  public void onlyCustomShouldReceiveEvents() throws Exception {
    final boolean[] consumerOk = new boolean[1];
    consumerOk[0] = false;
    EasyCollaboration easy = new EasyCollaboration(context, new EventConsumerPOA() {
      @Override
      public void push(Any event) throws ServiceFailure {
        consumerOk[0] = true;
      }
    }, new CollaborationObserverPOA() {
      @Override
      public void memberAdded(String name, IComponent member) throws ServiceFailure {

      }

      @Override
      public void memberRemoved(String name) throws ServiceFailure {

      }

      @Override
      public void destroyed() throws ServiceFailure {

      }
    });
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    byte[] simulation = {10,20,30,40};
    easy.shareDataKey(simulation);
    Thread.sleep(configs.sleeptime);
    Assert.assertEquals(0, easy.consumeDataKeys().size());
    Assert.assertTrue(consumerOk[0]);
    consumerOk[0] = false;
    Any any = context.orb().create_any();
    any.insert_double(10.20);
    easy.shareAny(any);
    Thread.sleep(configs.sleeptime);
    Assert.assertEquals(0, easy.consumeAnys().size());
    Assert.assertTrue(consumerOk[0]);
  }

  @Test
  public void consumeDataKeysButNoEvents() throws Exception {
    EasyCollaboration easy = new EasyCollaboration(context);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    Assert.assertEquals(0, easy.consumeDataKeys().size());
    easy.exitCollaboration();
    easy = new EasyCollaboration(context, null, null);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    Assert.assertEquals(0, easy.consumeDataKeys().size());
  }

  @Test
  public void consumeAnysButNoEvents() throws Exception {
    EasyCollaboration easy = new EasyCollaboration(context);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    Assert.assertEquals(0, easy.consumeAnys().size());
    easy.exitCollaboration();
    easy = new EasyCollaboration(context, null, null);
    session = easy.startCollaboration();
    Assert.assertNotNull(session);
    Assert.assertEquals(0, easy.consumeAnys().size());
  }

}