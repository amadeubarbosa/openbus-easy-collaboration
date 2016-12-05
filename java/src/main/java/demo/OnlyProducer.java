package demo;

import demo.Utils.ORBRunThread;
import demo.Utils.ShutdownThread;
import org.omg.CORBA.ORB;
import tecgraf.openbus.Connection;
import tecgraf.openbus.OpenBusContext;
import tecgraf.openbus.core.ORBInitializer;
import tecgraf.openbus.services.collaboration.easy.EasyCollaboration;
import tecgraf.openbus.services.collaboration.easy.IEasyCollaboration;

import javax.swing.BorderFactory;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JTextField;
import javax.swing.SwingUtilities;
import javax.swing.SwingWorker;
import javax.swing.border.Border;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.ArrayList;

public class OnlyProducer extends JFrame implements ActionListener {

  private static Connection conn;
  private static OpenBusContext context;
  private static IEasyCollaboration easy;
  private final GridBagConstraints constraints;
  public final Border border = BorderFactory.createLoweredBevelBorder();
  protected final JTextField keyText;
  final JButton startButton, stopButton, sendButton;

  private StartTask startTask;
  private StopTask stopTask;
  private SendTask sendTask;

  private JTextField makeText() {
    JTextField t = new JTextField(20);
    t.setToolTipText("DICA: Você pode usar ; para separar vários dados que serão enviados como um array de strings!!");
    t.setEditable(true);
    t.setHorizontalAlignment(JTextField.RIGHT);
    t.setBorder(border);
    getContentPane().add(t, constraints);
    return t;
  }

  private JButton makeButton(String caption) {
    JButton b = new JButton(caption);
    b.setActionCommand(caption);
    b.addActionListener(this);
    getContentPane().add(b, constraints);
    return b;
  }

  public OnlyProducer() {
    super(OnlyProducer.class.getSimpleName());
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
    sendButton = makeButton("Send");
    sendButton.setEnabled(false);

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
      sendButton.setEnabled(true);
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
      sendButton.setEnabled(false);
    }

  }

  private class SendTask extends SwingWorker<Void, Void> {

    @Override
    protected Void doInBackground() throws Exception {
      String[] array = keyText.getText().split(";");
      if (array.length > 1) {
        ArrayList<byte[]> list = new ArrayList<byte[]>();
        for (String term : array) {
          list.add(term.getBytes());
        }
        easy.shareDataKeys(list);
      } else {
        easy.shareDataKey(array[0].getBytes());
      }
      return null;
    }

    @Override
    protected void done() {
      keyText.setText(null);
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
      case "Send":
        (sendTask = new SendTask()).execute();
        break;
    }

  }

  public static void main(String[] args) throws Exception {
    Configs configs = Configs.readConfigsFile();
    String entity = configs.user;
    byte[] password = configs.password;
    String host = configs.bushost;
    int port = configs.busport;
//    Utils.setLibLogLevel(Level.FINE);

    ORB orb = ORBInitializer.initORB(args);
    new ORBRunThread(orb).start();
    ShutdownThread shutdown = new ShutdownThread(orb);
    Runtime.getRuntime().addShutdownHook(shutdown);

    context = (OpenBusContext) orb.resolve_initial_references("OpenBusContext");
    conn = context.createConnection(host, port);
    context.setDefaultConnection(conn);
    conn.loginByPassword(entity, password);
    shutdown.addConnection(conn);

    easy = new EasyCollaboration(context, null, null);

    SwingUtilities.invokeLater(new Runnable() {
      public void run() {
        new OnlyProducer();
      }
    });
  }
}
