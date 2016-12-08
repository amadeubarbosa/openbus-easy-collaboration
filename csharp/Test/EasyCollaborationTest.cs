using tecgraf.openbus.services.collaboration.easy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using tecgraf.openbus;
using tecgraf.openbus.services.collaboration.v1_0;
using System.Collections.Generic;
using Test.Properties;
using System.Text;
using System.Threading;
using omg.org.CORBA;

namespace Test
{
    /// <summary>
    ///This is a test class for EasyCollaborationTest and is intended
    ///to contain all EasyCollaborationTest Unit Tests
    ///</summary>
  [TestClass()]
  public class EasyCollaborationTest
  {
    private static OpenBusContext context;

    #region Additional test attributes

    /// <summary>
    /// Use ClassInitialize to run code before running the first test in the class
    /// </summary>
    /// <param name="testContext"></param>
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      context = ORBInitializer.Context;
      Connection conn = context.CreateConnection(Settings.Default.host, Settings.Default.port);
      context.SetDefaultConnection(conn);
      conn.LoginByPassword(Settings.Default.username, ASCIIEncoding.Default.GetBytes(Settings.Default.password));
    }

    /// <summary>
    /// Use ClassCleanup to run code after all tests in a class have run
    /// </summary>
    [ClassCleanup()]
    public static void MyClassCleanup()
    {
      context.GetDefaultConnection().Logout();
    }

    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion

    /// <summary>
    ///A test for multiple calls to StartCollaboration method
    ///</summary>
    [TestMethod()]
    public void MultipleStartCalls()
    {
      EasyCollaboration target = new EasyCollaboration(context);
      CollaborationSession session = target.StartCollaboration();
      Assert.IsNotNull(session);
      Assert.AreEqual(session, target.StartCollaboration());
      Assert.AreEqual(session, target.StartCollaboration());
      Assert.AreEqual(session, target.StartCollaboration());
      Assert.AreEqual(session, target.StartCollaboration());
      target.ExitCollaboration();
      target = new EasyCollaboration(context, null, null);
      session = target.StartCollaboration();
      Assert.IsNotNull(session);
      Assert.AreEqual(session, target.StartCollaboration());
      Assert.AreEqual(session, target.StartCollaboration());
      Assert.AreEqual(session, target.StartCollaboration());
      Assert.AreEqual(session, target.StartCollaboration());
      target.ExitCollaboration();
    }

    /// <summary>
    ///A test for multiple calls to ExitCollaboration method
    ///</summary>
    [TestMethod()]
    public void MultipleExitCalls()
    {
      EasyCollaboration target = new EasyCollaboration(context);
      CollaborationSession session = target.StartCollaboration();
      Assert.IsNotNull(session);
      target.ExitCollaboration();
      target.ExitCollaboration();
      target.ExitCollaboration();
      target.ExitCollaboration();
      target = new EasyCollaboration(context, null, null);
      session = target.StartCollaboration();
      Assert.IsNotNull(session);
      target.ExitCollaboration();
      target.ExitCollaboration();
      target.ExitCollaboration();
      target.ExitCollaboration();
    }

    /// <summary>
    ///A test for ConsumeAnys
    ///</summary>
    [TestMethod()]
    public void ConsumeAnysTest()
    {
      omg.org.CORBA.TypeCode long_tc = context.ORB.create_long_tc();

      EasyCollaboration target = new EasyCollaboration(context);
      target.StartCollaboration();

      List<object> actual = null;
      try
      {
        actual = target.ConsumeAnys();
        Assert.AreEqual(0, actual.Count);

        target.Share(new omg.org.CORBA.Any(11, long_tc));
        target.Share(new omg.org.CORBA.Any(12, long_tc));
        target.Share(new omg.org.CORBA.Any(21, long_tc));
        target.Share(new omg.org.CORBA.Any(22, long_tc));
        Thread.Sleep(4000);
        actual = target.ConsumeAnys();
        Assert.AreEqual(4, actual.Count);
        Assert.AreEqual(11, actual[0]);
        Assert.AreEqual(12, actual[1]);
        Assert.AreEqual(21, actual[2]);
        Assert.AreEqual(22, actual[3]);
      }
      finally
      {
        target.ExitCollaboration();
      }

      target = new EasyCollaboration(context, null, null);
      target.StartCollaboration();
      try
      {
        actual = target.ConsumeAnys();
        Assert.AreEqual(0, actual.Count);

        target.Share(new omg.org.CORBA.Any(11, long_tc));
        target.Share(new omg.org.CORBA.Any(12, long_tc));
        target.Share(new omg.org.CORBA.Any(21, long_tc));
        target.Share(new omg.org.CORBA.Any(22, long_tc));
        Thread.Sleep(2000);
        actual = target.ConsumeAnys();
        Assert.AreEqual(0, actual.Count);
      }
      finally
      {
        target.ExitCollaboration();
      }
    }

    /// <summary>
    ///A test for ConsumeDataKeys
    ///</summary>
    [TestMethod()]
    public void ConsumeDataKeysTest()
    {
      EasyCollaboration target = new EasyCollaboration(context);
      target.StartCollaboration();
      List<byte[]> actual = null;
      List<byte[]> datakeys = new List<byte[]>();
      datakeys.Add(ASCIIEncoding.Default.GetBytes("test"));
      datakeys.Add(ASCIIEncoding.Default.GetBytes("test1"));
      datakeys.Add(ASCIIEncoding.Default.GetBytes("test2"));
      datakeys.Add(ASCIIEncoding.Default.GetBytes("test3"));

      omg.org.CORBA.TypeCode byteTC = context.ORB.create_octet_tc();
      omg.org.CORBA.TypeCode sequenceTC = context.ORB.create_sequence_tc(0, byteTC);

      try 
      {
        actual = target.ConsumeDataKeys();
        Assert.AreEqual(0, actual.Count);

        target.ShareDataKeys(datakeys);
        Thread.Sleep(4000);
        actual = target.ConsumeDataKeys();
        Assert.AreEqual(datakeys.Count, actual.Count);
        for (int i = 0; i < datakeys.Count; i++)
        {
          CollectionAssert.AreEqual(datakeys[i], actual[i]);
        }
      }
      finally
      {
        target.ExitCollaboration();
      }

      target = new EasyCollaboration(context, null, null);
      target.StartCollaboration();
      try
      {
        actual = target.ConsumeDataKeys();
        Assert.AreEqual(0, actual.Count);

        target.ShareDataKeys(datakeys);
        Thread.Sleep(2000);
        actual = target.ConsumeDataKeys();
        Assert.AreEqual(0, actual.Count);
      }
      finally
      {
        target.ExitCollaboration();
      }
    }

  }
}
