package demo;

import org.omg.CORBA.ORB;
import org.omg.CORBA.ORBPackage.InvalidName;
import org.omg.IOP.Codec;
import org.omg.IOP.CodecFactory;
import org.omg.IOP.CodecFactoryHelper;
import org.omg.IOP.CodecFactoryPackage.UnknownEncoding;
import org.omg.IOP.ENCODING_CDR_ENCAPS;
import org.omg.IOP.Encoding;
import tecgraf.openbus.CallerChain;
import tecgraf.openbus.Connection;
import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.core.v2_0.services.access_control.LoginInfo;
import tecgraf.openbus.core.v2_0.services.offer_registry.ServiceOffer;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.logging.ConsoleHandler;
import java.util.logging.Formatter;
import java.util.logging.Handler;
import java.util.logging.Level;
import java.util.logging.LogRecord;
import java.util.logging.Logger;

/**
 * Classe utilit�ria para os demos Java.
 * 
 * @author Tecgraf
 */
public class Utils {

  /**
   * L� um arquivo de propriedades.
   * 
   * @param fileName o nome do arquivo.
   * @return as propriedades.
   * @throws IOException
   */
  static public Properties readPropertyFile(String fileName) throws IOException {
    InputStream in = null;
    Properties properties = new Properties();
    File file = new File(fileName);
    if (file.exists() && !file.isDirectory() && file.canRead()) {
      in = new FileInputStream(file);
    }
    else {
      in = Utils.class.getResourceAsStream(fileName);
    }
    if (in == null) {
      System.err.println(String.format(
        "O arquivo de propriedades '%s' n�o foi encontrado", fileName));
      return properties;
    }
    try {
      properties.load(in);
    }
    finally {
      try {
        in.close();
      }
      catch (IOException e) {
        System.err
          .println("Ocorreu um erro ao fechar o arquivo de propriedades");
        e.printStackTrace();
      }
    }
    return properties;
  }

  /**
   * L� todo um arquivo e retorna como um array de bytes.
   * 
   * @param path arquivo a ser lido.
   * @return o conte�do do arquivo.
   * @throws IOException
   */
  static public byte[] readFile(String path) throws IOException {
    byte[] data = null;
    File file = new File(path);
    FileInputStream is = new FileInputStream(file);
    try {
      int length = (int) file.length();
      data = new byte[length];
      int offset = is.read(data);
      while (offset < length) {
        int read = is.read(data, offset, length - offset);
        if (read < 0) {
          throw new IOException("N�o foi poss�vel ler todo o arquivo");
        }
        offset += read;
      }
    }
    finally {
      is.close();
    }
    return data;
  }

  /**
   * Configua o n�vel de log dos testes de interoperabilidade
   * 
   * @param level n�vel do log
   */
  public static void setTestLogLevel(Level level) {
    Logger logger = Logger.getLogger("tecgraf.openbus.interop");
    setLogLevel(logger, level);
  }

  /**
   * Configua o n�vel de log da biblioteca de acesso openbus
   * 
   * @param level n�vel do log
   */
  public static void setLibLogLevel(Level level) {
    Logger logger = Logger.getLogger("tecgraf.openbus.core");
    setLogLevel(logger, level);
  }

  /**
   * Configura o n�vel de log
   * 
   * @param logger logger a ser configurado
   * @param level n�vel do log.
   */
  public static void setLogLevel(Logger logger, Level level) {
    logger.setLevel(level);
    for (Handler h : logger.getHandlers()) {
      logger.removeHandler(h);
    }
    logger.setUseParentHandlers(false);
    ConsoleHandler handler = new ConsoleHandler();
    handler.setFormatter(new LogFormatter());
    handler.setLevel(level);
    logger.addHandler(handler);
  }

  /**
   * Converte uma cadeia para uma representa��o textual.
   *
   * @param chain a cadeia
   * @return uma representa��o textual da mesma.
   */
  static public String chain2str(CallerChain chain) {
    StringBuffer buffer = new StringBuffer();
    for (LoginInfo loginInfo : chain.originators()) {
      buffer.append(loginInfo.entity);
      buffer.append("->");
    }
    buffer.append(chain.caller().entity);
    return buffer.toString();
  }

  /**
   * Constr�i uma inst�ncia de {@link Codec}
   *
   * @param orb o {@link ORB}
   * @return um {@link Codec}
   * @throws UnknownEncoding
   * @throws InvalidName
   */
  public static Codec createCodec(ORB orb) throws UnknownEncoding, InvalidName {
    org.omg.CORBA.Object obj;
    obj = orb.resolve_initial_references("CodecFactory");
    CodecFactory codecFactory = CodecFactoryHelper.narrow(obj);
    byte major = 1;
    byte minor = 2;
    Encoding encoding = new Encoding(ENCODING_CDR_ENCAPS.value, major, minor);
    return codecFactory.create_codec(encoding);
  }

  /**
   * L� um arquivo de IOR e retorna a linha que representa o IOR
   *
   * @param iorfile path para arquivo
   * @return a String do IOR
   * @throws IOException
   */
  public static String file2IOR(String iorfile) throws IOException {
    BufferedReader in = null;
    try {
      in = new BufferedReader(new FileReader(iorfile));
      return in.readLine();
    }
    finally {
      if (in != null) {
        in.close();
      }
    }
  }

  /**
   * Formatador de logging
   *
   * @author Tecgraf/PUC-Rio
   */
  private static class LogFormatter extends Formatter {
    /** Formatador de data */
    SimpleDateFormat time = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");

    @Override
    public String format(LogRecord record) {
      String result =
        String.format("%s [%s] %s\n", time.format(record.getMillis()), record
          .getLevel(), record.getMessage());
      Throwable t = record.getThrown();
      return t == null ? result : result + getStackTrace(t);
    }

    /**
     * Convers�o de pilha de erro para {@link String}
     * 
     * @param t o erro.
     * @return a representa��o do erro em {@link String}
     */
    private String getStackTrace(Throwable t) {
      StringWriter sw = new StringWriter();
      t.printStackTrace(new PrintWriter(sw));
      return sw.toString();
    }
  }

  /**
   * Thread para execu��o do {@link ORB#run()}
   *
   * @author Tecgraf/PUC-Rio
   */
  public static class ORBRunThread extends Thread {
    /** o orb */
    private ORB orb;

    /**
     * Construtor
     *
     * @param orb o orb
     */
    public ORBRunThread(ORB orb) {
      this.orb = orb;
    }

    @Override
    public void run() {
      this.orb.run();
    }
  }

  /**
   * Thread de finaliza��o do ORB que pode ser inclu�da no
   * {@link Runtime#addShutdownHook(Thread)} para realizar limpezas necess�rias
   *
   * @author Tecgraf/PUC-Rio
   */
  public static class ShutdownThread extends Thread {
    /** o orb */
    private ORB orb;
    /** lista de conex�es a serem liberadas */
    private List<Connection> conns = new ArrayList<Connection>();
    /** lista de ofertas a serem liberadas */
    private List<ServiceOffer> offers = new ArrayList<ServiceOffer>();

    /**
     * Construtor
     *
     * @param orb o orb
     */
    public ShutdownThread(ORB orb) {
      this.orb = orb;
    }

    @Override
    public void run() {

      for (ServiceOffer offer : this.offers) {
        try {
          offer.remove();
        }
        catch (Exception e) {
          e.printStackTrace();
        }
      }

      for (Connection conn : this.conns) {
        try {
          conn.logout();
        }
        catch (ServiceFailure e) {
          e.printStackTrace();
        }
      }
      this.orb.shutdown(true);
      this.orb.destroy();
    }

    /**
     * Inclui uma conex�o na lista de conex�es a serem liberadas pela thread
     *
     * @param conn a conex�o
     */
    public void addConnection(Connection conn) {
      this.conns.add(conn);
    }

    /**
     * Inclui uma oferta na lista de ofertas a serem liberadas pela thread
     *
     * @param offer a oferta
     */
    public void addOffer(ServiceOffer offer) {
      this.offers.add(offer);
    }

  }
}
