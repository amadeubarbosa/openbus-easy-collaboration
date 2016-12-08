using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Receiver
{
  static class Program
  {
    class MultiFormApplicationContext : ApplicationContext
    {
      private Receiver receiver;
      private OnDemandReceiver ondemand;

      public MultiFormApplicationContext()
      {
        receiver = new Receiver();
        ondemand = new OnDemandReceiver();

        receiver.Closed += new EventHandler(Receiver.Shutdown);
        receiver.Closed += new EventHandler(CheckIfShouldExit);
        ondemand.Closed += new EventHandler(OnDemandReceiver.Shutdown);
        ondemand.Closed += new EventHandler(CheckIfShouldExit);

        receiver.Show();
        ondemand.Show();
      }

      private void CheckIfShouldExit(object sender, EventArgs e)
      {
        if (((sender is Receiver) && ondemand.IsDisposed) ||
            ((sender is OnDemandReceiver) && receiver.IsDisposed))
          ExitThread();
      }
    }
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MultiFormApplicationContext());
    }
  }
}
