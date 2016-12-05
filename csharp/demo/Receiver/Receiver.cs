using System;
using System.Text;
using System.Windows.Forms;
using Receiver.Properties;
using tecgraf.openbus;
using tecgraf.openbus.services.collaboration.easy;

namespace Receiver
{
  public partial class Receiver : Form
  {
    private EasyCollaboration easy;

    public Receiver()
    {
      InitializeComponent();
      Setup();
    }

    private void Setup()
    {
      OpenBusContext context = ORBInitializer.Context;

      Connection conn = context.CreateConnection(Settings.Default.host, Settings.Default.port);
      context.SetDefaultConnection(conn);
      conn.LoginByPassword(Settings.Default.username, ASCIIEncoding.Default.GetBytes(Settings.Default.password));
      easy = new EasyCollaboration(ORBInitializer.Context);
    }

    public static void Shutdown(object sender, EventArgs e)
    {
      ORBInitializer.Context.GetDefaultConnection().Logout();
    }

    private void Start_click(object sender, EventArgs e)
    {
      easy.StartCollaboration();
      start.Enabled = false;
      stop.Enabled = true;
    }
    private void Stop_click(object sender, EventArgs e)
    {
      easy.ExitCollaboration();
      start.Enabled = true;
      stop.Enabled = false;
    }
    private void Receive_click(object sender, EventArgs e)
    {
      var datakeys = easy.ConsumeDataKeys();
      foreach (byte[] key in datakeys)
      {
        this.output.Text += ASCIIEncoding.UTF8.GetString(key) + System.Environment.NewLine;
      }
    }

    private void Receiver_Load(object sender, EventArgs e)
    {
      this.start.Click += new System.EventHandler(this.Start_click);
      this.stop.Click += new System.EventHandler(this.Stop_click);
      this.receive.Click += new System.EventHandler(this.Receive_click);
    }
  }
}
