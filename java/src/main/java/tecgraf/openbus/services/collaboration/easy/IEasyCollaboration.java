package tecgraf.openbus.services.collaboration.easy;

import java.util.List;

import org.omg.CORBA.Any;

import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.services.collaboration.v1_0.CollaborationSession;

/**
 * Interface para biblioteca de apoio ao servi�o de colabora��o.
 * O objetivo dessa interface � fornecer alguns m�todos a serem utilizados
 * nas atividades mais corriqueiras relacionadas ao uso de sess�es de colabora��o.
 * 
 * @author Tecgraf/PUC-Rio
 *
 */
public interface IEasyCollaboration {

  /**
   * Inicia uma sess�o de colabora��o.
   * Esse m�todo deve ser respons�vel por buscar o servi�o de colabora��o, criar 
   * uma sess�o e registr�-la no registro de sess�es. Al�m disso, um consumidor deve ser
   * cadastrado no canal de eventos da sess�o para a troca de informa��es. 
   * 
   * @return sess�o de colabora��o
   * @throws ServiceFailure
   */
  public abstract CollaborationSession startCollaboration()
    throws ServiceFailure;

  /**
   * Encerra a sess�o de colabora��o.
   * Tamb�m remove observadores e consumidores do canal de eventos.
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
   * Compartilha um objeto, desde que encapsulado com o tipo vari�vel {@link Any} de CORBA, no canal de eventos.
   * Para encapsular o objeto em um {@link Any} � preciso que o tipo esteja declarado em IDL e que se use os m�todos
   * <code>insert</code> da respectiva classe com sufixo <code>Helper</code> para inserir o objeto em um {@link Any}.
   * 
   * @param any inst�ncia do {@link Any}
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
   * Consome os objetos, encapsulados com o tipo vari�vel {@link Any} de CORBA, do canal de eventos.
   * Para obter o objeto a partir de um {@link Any} � preciso que seu tipo esteja declarado em IDL e que se use os
   * m�todos <code>extract</code> da respectiva classe com sufixo <code>Helper</code> para extra�-lo de um {@link Any}.
   * 
   * @return lista de inst�ncias de {@link Any}
   */
  public abstract List<Any> consumeAnys();

}