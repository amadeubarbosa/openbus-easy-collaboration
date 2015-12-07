package demo;

import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.List;
import java.util.concurrent.ExecutionException;

import javax.swing.BorderFactory;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.SwingUtilities;
import javax.swing.SwingWorker;
import javax.swing.border.Border;

import lib.IEasyCollaboration;
import lib.EasyCollaboration;
import lib.LibUtils.ORBRunThread;
import lib.LibUtils.ShutdownThread;

import org.omg.CORBA.ORB;

import tecgraf.openbus.Connection;
import tecgraf.openbus.OpenBusContext;
import tecgraf.openbus.core.ORBInitializer;

public class Receiver extends JFrame implements ActionListener {

  private static Connection conn;
  private static OpenBusContext context;
  private static IEasyCollaboration easy;
  private final GridBagConstraints constraints;
  public final Border border = BorderFactory.createLoweredBevelBorder();
  protected final JTextArea keyText;
  final JButton startButton, stopButton, receiveButton;

  private StartTask startTask;
  private StopTask stopTask;
  private ReceiveTask receiveTask;

  private JTextArea makeText() {
    JTextArea t = new JTextArea(6, 20);
    t.setEditable(false);
    t.setBorder(border);
    getContentPane().add(new JScrollPane(t), constraints);
    return t;
  }

  private JButton makeButton(String caption) {
    JButton b = new JButton(caption);
    b.setActionCommand(caption);
    b.addActionListener(this);
    getContentPane().add(b, constraints);
    return b;
  }

  public Receiver() {
    super("Receiver");
    setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

    //Make text boxes
    getContentPane().setLayout(new GridBagLayout());
    constraints = new GridBagConstraints();
    constraints.insets = new Insets(3, 10, 3, 10);
    keyText = makeText();

    //Make buttons
    startButton = makeButton("Start");
    stopButton = makeButton("Stop");
    stopButton.setEnabled(false);
    receiveButton = makeButton("Receive");
    receiveButton.setEnabled(false);

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
      receiveButton.setEnabled(true);
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
      receiveButton.setEnabled(false);
    }

  }

  private class ReceiveTask extends SwingWorker<List<byte[]>, Void> {

    @Override
    protected List<byte[]> doInBackground() throws Exception {
      List<byte[]> list = easy.consumeDataKeys();
      return list;
    }

    @Override
    protected void done() {
      List<byte[]> list;
      try {
        list = get();
        StringBuffer buffer = new StringBuffer(keyText.getText());
        for (byte[] key : list) {
          buffer.append(new String(key) + "\n");
        }
        keyText.setText(buffer.toString());
      }
      catch (InterruptedException | ExecutionException e) {
        e.printStackTrace();
      }
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
      case "Receive":
        (receiveTask = new ReceiveTask()).execute();
        break;
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
    shutdown.addConnetion(conn);
    easy = new EasyCollaboration(context);

    SwingUtilities.invokeLater(new Runnable() {
      public void run() {
        new Receiver();
      }
    });
  }
}
