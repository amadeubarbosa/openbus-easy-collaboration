﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using tecgraf.openbus.easycollab;
using tecgraf.openbus;
using Sender.Properties;
using System.Configuration;

namespace Sender
{
  public partial class Sender : Form
  {
    private IEasyCollaboration easy;

    public Sender()
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
    private void Send_click(object sender, EventArgs e)
    {
      easy.ShareDataKeys(new List<byte[]>() { ASCIIEncoding.UTF8.GetBytes(this.input.Text) });
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