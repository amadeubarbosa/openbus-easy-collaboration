package demo;

import demo.Utils.ORBRunThread;
import demo.Utils.ShutdownThread;
import org.omg.CORBA.Any;
import org.omg.CORBA.ORB;
import tecgraf.openbus.Connection;
import tecgraf.openbus.OpenBusContext;
import tecgraf.openbus.core.ORBInitializer;
import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.services.collaboration.easy.EasyCollaboration;
import tecgraf.openbus.services.collaboration.easy.IEasyCollaboration;
import tecgraf.openbus.services.collaboration.v1_0.EventConsumerPOA;

import javax.swing.BorderFactory;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.SwingUtilities;
import javax.swing.SwingWorker;
import javax.swing.border.Border;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

public class OnDemandReceiver extends JFrame implements ActionListener {

  private static Connection conn;
  private static OpenBusContext context;
  private static IEasyCollaboration easy;
  private final GridBagConstraints constraints;
  public final Border border = BorderFactory.createLoweredBevelBorder();
  final JButton startButton, stopButton;
  final JLabel receiveStatus;

  private StartTask startTask;
  private StopTask stopTask;

  private JButton makeButton(String caption) {
    JButton b = new JButton(caption);
    b.setActionCommand(caption);
    b.addActionListener(this);
    getContentPane().add(b, constraints);
    return b;
  }

  public OnDemandReceiver() {
    super(OnDemandReceiver.class.getSimpleName());
    setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

    //Make text boxes
    getContentPane().setLayout(new GridBagLayout());
    constraints = new GridBagConstraints();
    constraints.insets = new Insets(3, 10, 3, 10);

    //Make buttons
    startButton = makeButton("Start");
    stopButton = makeButton("Stop");
    stopButton.setEnabled(false);

    //Make online label
    receiveStatus = new JLabel();
    receiveStatus.setText("Waiting to start...        ");
    getContentPane().add(receiveStatus, constraints);

    easy = new EasyCollaboration(context, new OnDemandReceiver.Consumer(), null);

    //Display the window.
    pack();
    setVisible(true);
    setResizable(false);
  }

  private class StartTask extends SwingWorker<Void, Void> {

    @Override
    protected Void doInBackground() throws Exception {
      easy.startCollaboration();
      return null;
    }

    @Override
    protected void done() {
      startButton.setEnabled(false);
      stopButton.setEnabled(true);
      receiveStatus.setText("Waiting for events...    ");
    }

  }

  private class StopTask extends SwingWorker<Void, Void> {

    @Override
    protected Void doInBackground() throws Exception {
      easy.exitCollaboration();
      return null;
    }

    @Override
    protected void done() {
      startButton.setEnabled(true);
      stopButton.setEnabled(false);
      receiveStatus.setText("Waiting to start...      ");
    }

  }

  public void actionPerformed(ActionEvent e) {
    switch (e.getActionCommand()) {
      case "Start":
        (startTask = new StartTask()).execute();
        break;
      case "Stop":
        (stopTask = new StopTask()).execute();
        break;
    }

  }

  class Consumer extends EventConsumerPOA {
    long events = 0;

    @Override
    public void push(Any event) throws ServiceFailure {
      String unit = "th";
      events += 1;
      if (events <= 3) {
        unit = "rd";
      } else if (events == 2) {
        unit = "nd";
      } else if (events == 1) {
        unit = "st";
      }
      receiveStatus.setText("Received "+ events + unit + " event!");
    }
  }


  public static void main(String[] args) throws Exception {
    Configs configs = Configs.readConfigsFile();
    String entity = configs.user;
    byte[] password = configs.password;
    String host = configs.bushost;
    int port = configs.busport;
    //Utils.setLibLogLevel(Level.FINE);

    ORB orb = ORBInitializer.initORB(args);
    new ORBRunThread(orb).start();
    ShutdownThread shutdown = new ShutdownThread(orb);
    Runtime.getRuntime().addShutdownHook(shutdown);

    context = (OpenBusContext) orb.resolve_initial_references("OpenBusContext");
    conn = context.createConnection(host, port);
    context.setDefaultConnection(conn);
    conn.loginByPassword(entity, password);
    shutdown.addConnection(conn);

    SwingUtilities.invokeLater(new Runnable() {
      public void run() {
        new OnDemandReceiver();
      }
    });
  }
}
