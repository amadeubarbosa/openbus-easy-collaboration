using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Sender.Properties;
using tecgraf.openbus;
using tecgraf.openbus.services.collaboration.easy;

namespace Sender
{
  public partial class Sender : Form
  {
    private IEasyCollaboration easy;

    public Sender()
    {
      InitializeComponent();
      Setup();
      new ToolTip().SetToolTip(this.input, 
        "DICA: Você pode usar ; para separar vários dados"+
        " que serão enviados como um array de strings!!");
    }

    private void Setup()
    {
      OpenBusContext context = ORBInitializer.Context;

      Connection conn = context.CreateConnection(Settings.Default.host, Settings.Default.port);
      context.SetDefaultConnection(conn);
      conn.LoginByPassword(Settings.Default.username, ASCIIEncoding.Default.GetBytes(Settings.Default.password));
      easy = new EasyCollaboration(context, null, null);
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
    private void Send_click(object sender, EventArgs e)
    {
      char[] delimiter = {';'};
      List<byte[]> data = new List<byte[]>();
      foreach (string item in this.input.Text.Split(delimiter))
      {
        data.Add(ASCIIEncoding.UTF8.GetBytes(item));
      }
      easy.ShareDataKeys(data);
      this.input.Text = "";
    }

    private void Sender_Load(object sender, EventArgs e)
    {
      this.start.Click += new System.EventHandler(this.Start_click);
      this.stop.Click += new System.EventHandler(this.Stop_click);
      this.send.Click += new System.EventHandler(this.Send_click);
    }

  }
}
