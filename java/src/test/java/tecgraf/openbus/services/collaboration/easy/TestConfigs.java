package tecgraf.openbus.services.collaboration.easy;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;
import java.util.logging.Level;

/**
 * Utilit�rio de configura��o de testes
 *
 * @author Tecgraf/PUC-Rio
 */
public class TestConfigs {
  /** Host */
  public String bushost;
  /** Porta */
  public int busport;
  /** Referencia */
  public String busref;
  /** Certificado */
  public String buscrt;
  /** Host */
  public String bus2host;
  /** Porta */
  public int bus2port;
  /** Referencia */
  public String bus2ref;
  /** Certificado */
  public String bus2crt;
  /** Admin */
  public String admin;
  /** Senha Admin */
  public byte[] admpsw;
  /** Dominio */
  public String domain;
  /** Usuario */
  public String user;
  /** Senha */
  public byte[] password;
  /** Sistema */
  public String system;
  /** Chave privada */
  public String syskey;
  /** Path para arquivo sharedauth */
  public String sharedauth;
  /** Nivel de log do teste */
  public Level testlog;
  /** N�vel de log da tecgraf.openbus.services.collaboration.easy */
  public Level log;
  /** Propriedades do ORB */
  public String orbprops;

  //Java's particular props
  /** Chave privada errada */
  public String wrongkey;
  /** Sistema sem certificado cadastrado */
  public String wrongsystem;
  /** Tempo de espera entre chamadas */
  public int sleeptime;

  /**
   * Construtor.
   * 
   * @param props propriedades a serem carregadas
   */
  private TestConfigs(Properties props) {
    bushost = props.getProperty("bus.host.name", "localhost");
    busport = Integer.valueOf(props.getProperty("bus.host.port", "2089"));
    busref = props.getProperty("bus.reference.path", "BUS01.ior");
    buscrt = props.getProperty("bus.certificate.path", "BUS01.crt");
    bus2host = props.getProperty("bus2.host.name", bushost);
    Integer port2 = busport + 1;
    bus2port =
      Integer.valueOf(props.getProperty("bus2.host.port", port2.toString()));
    bus2ref = props.getProperty("bus2.reference.path", "BUS02.ior");
    bus2crt = props.getProperty("bus2.certificate.path", "BUS02.crt");

    admin = props.getProperty("admin.enitiy.name", "admin");
    admpsw = props.getProperty("admin.password", admin).getBytes();
    domain = props.getProperty("user.password.domain", "testing");
    user = props.getProperty("user.entity.name", "testuser");
    password = props.getProperty("user.password", user).getBytes();
    system = props.getProperty("system.entity.name", "testsyst");
    syskey = props.getProperty("system.private.key", "testsyst.key");
    sharedauth = props.getProperty("system.sharedauth", "sharedauth.dat");

    testlog =
      parseLevelFromNumber(Integer.valueOf(props.getProperty(
        "openbus.test.verbose", "0")));
    log =
      parseLevelFromNumber(Integer.valueOf(props.getProperty(
        "openbus.log.level", "0")));
    orbprops = props.getProperty("jacorb.properties", "/jacorb.properties");

    wrongkey = props.getProperty("system.wrong.key", "wrong.key");
    wrongsystem = props.getProperty("system.wrong.name", "nocertsyst");

    sleeptime = Integer.valueOf(props.getProperty("sleep.ms.time", "1000"));
  }

  /**
   * Recupera o arquivo de configura��es atrav�s da vari�vel de ambiente
   * OPENBUS_TESTCFG, ou do arquivo padr�o "/test.properties"
   * 
   * @return as configura��es
   * @throws IOException
   */
  public static TestConfigs readConfigsFile() throws IOException {
    String path = System.getenv("OPENBUS_TESTCFG");
    if (path == null) {
      path = "/test.properties";
    }
    return new TestConfigs(TestConfigs.readPropertyFile(path));
  }

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
      in = TestConfigs.class.getResourceAsStream(fileName);
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
   * Convers�o do n�vel de log de n�mero para o tipo {@link Level}
   * 
   * @param level n�mero do n�vel de log
   * @return o n�vel de log
   */
  private Level parseLevelFromNumber(Integer level) {
    Map<Integer, Level> levels = new HashMap<Integer, Level>();
    levels.put(0, Level.OFF);
    levels.put(1, Level.SEVERE);
    levels.put(2, Level.WARNING);
    levels.put(3, Level.INFO);
    levels.put(4, Level.CONFIG);
    levels.put(5, Level.FINE);
    levels.put(6, Level.FINER);
    levels.put(7, Level.FINEST);
    return levels.get(level);
  }
}
