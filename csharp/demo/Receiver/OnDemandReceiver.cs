using System;
using System.Text;
using System.Windows.Forms;
using Receiver.Properties;
using tecgraf.openbus;
using tecgraf.openbus.services.collaboration.easy;
using tecgraf.openbus.services.collaboration.v1_0;

namespace Receiver
{
  public partial class OnDemandReceiver : Form
  {
    private EasyCollaboration easy;
    private readonly String firstStatus = "Waiting to start...";

    public OnDemandReceiver()
    {
      InitializeComponent();
      status.Text = firstStatus;
      Setup();
    }

    private void Setup()
    {
      OpenBusContext context = ORBInitializer.Context;
      if (context.GetDefaultConnection() == null)
      {
        Connection conn = context.CreateConnection(Settings.Default.host, Settings.Default.port);
        context.SetDefaultConnection(conn);
        conn.LoginByPassword(Settings.Default.username, ASCIIEncoding.Default.GetBytes(Settings.Default.password));
      }
      // it will force EasyCollaboration to use only our custom consumer and no observer
      easy = new EasyCollaboration(context, new Consumer(this), null);
    }

    public static void Shutdown(object sender, EventArgs e)
    {
      ORBInitializer.Context.GetDefaultConnection().Logout();
    }

    private void Start_click(object sender, EventArgs e)
    {
      easy.StartCollaboration();
      status.Text = "Waiting for events...";
      start.Enabled = false;
      stop.Enabled = true;
    }
    private void Stop_click(object sender, EventArgs e)
    {
      easy.ExitCollaboration();
      status.Text = firstStatus;
      start.Enabled = true;
      stop.Enabled = false;
    }

    private void OnDemandReceiver_Load(object sender, EventArgs e)
    {
      this.start.Click += new System.EventHandler(this.Start_click);
      this.stop.Click += new System.EventHandler(this.Stop_click);
    }

    class Consumer : MarshalByRefObject, EventConsumer
    {
      private long count = 0;
      private OnDemandReceiver component;

      public Consumer(OnDemandReceiver receiver)
      {
        this.component = receiver;
      }

      public void push(object ev) 
      {
        String n = "th";
        count++;
        switch (count) {
          case 3 : n = "rd"; break;
          case 2 : n = "nd"; break;
          case 1 : n = "st"; break;
        }
        component.Invoke((MethodInvoker) delegate { 
          component.status.Text = "Received "+ count.ToString() + n + " events !";
        });
      }

      public override object InitializeLifetimeService()
      {
        // Evita a desativação automática pela política de ciclo de vida do MarshalByRefObject
        return null;
      }
    }
  }
}
