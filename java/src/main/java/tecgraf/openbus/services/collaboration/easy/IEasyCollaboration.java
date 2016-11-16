package tecgraf.openbus.services.collaboration.easy;

import java.util.List;

import org.omg.CORBA.Any;

import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationSession;

/**
 * Interface para biblioteca de apoio ao serviço de colaboração.
 * O objetivo dessa interface é fornecer alguns métodos a serem utilizados
 * nas atividades mais corriqueiras relacionadas ao uso de sessões de colaboração.
 * 
 * @author Tecgraf/PUC-Rio
 *
 */
public interface IEasyCollaboration {

  /**
   * Inicia uma sessão de colaboração.
   * Esse método deve ser responsável por buscar o serviço de colaboração, criar 
   * uma sessão e registrá-la no registro de sessões. Além disso, um consumidor deve ser
   * cadastrado no canal de eventos da sessão para a troca de informações. 
   * 
   * @return sessão de colaboração
   * @throws ServiceFailure
   */
  public abstract CollaborationSession startCollaboration()
    throws ServiceFailure;

  /**
   * Encerra a sessão de colaboração.
   * Também remove observadores e consumidores do canal de eventos.
   * 
   * @throws ServiceFailure
   */
  public abstract void exitCollaboration() throws ServiceFailure;

  /**
   * Compartilha um datakey no canal de eventos.
   * 
   * @param key datakey
   * @throws ServiceFailure
   */
  public abstract void shareDataKey(byte[] key) throws ServiceFailure;

  /**
   * Compartilha uma lista de datakeys no canal de eventos.
   * 
   * @param keys lista de datakeys
   * @throws ServiceFailure
   */
  public abstract void shareDataKeys(List<byte[]> keys) throws ServiceFailure;
  
  /**
   * Compartilha um objeto, desde que encapsulado com o tipo variável {@link Any} de CORBA, no canal de eventos.
   * Para encapsular o objeto em um {@link Any} é preciso que o tipo esteja declarado em IDL e que se use os métodos
   * <code>insert</code> da respectiva classe com sufixo <code>Helper</code> para inserir o objeto em um {@link Any}.
   * 
   * @param any instância do {@link Any}
   * @throws ServiceFailure
   */
  public abstract void shareAny(Any any) throws ServiceFailure;

  /**
   * Consome os datakeys no canal de eventos.
   * 
   * @return lista de array de bytes
   */
  public abstract List<byte[]> consumeDataKeys();

  /**
   * Consome os objetos, encapsulados com o tipo variável {@link Any} de CORBA, do canal de eventos.
   * Para obter o objeto a partir de um {@link Any} é preciso que seu tipo esteja declarado em IDL e que se use os
   * métodos <code>extract</code> da respectiva classe com sufixo <code>Helper</code> para extraí-lo de um {@link Any}.
   * 
   * @return lista de instâncias de {@link Any}
   */
  public abstract List<Any> consumeAnys();

}