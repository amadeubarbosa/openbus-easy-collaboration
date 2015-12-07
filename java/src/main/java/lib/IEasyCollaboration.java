package lib;

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
public interface EasyCollabing {

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
   * Compartilha um array de bytes no canal de eventos.
   * 
   * @param key array de bytes
   * @throws ServiceFailure
   */
  public abstract void shareDataKey(byte[] key) throws ServiceFailure;

  /**
   * Compartilha um objeto qualquer no canal de eventos.
   * 
   * @param any objeto
   * @throws ServiceFailure
   */
  public abstract void shareAny(Any any) throws ServiceFailure;

  /**
   * Consome os arrays de bytes no canal de eventos.
   * 
   * @return lista de array de bytes
   */
  public abstract List<byte[]> consumeDataKey();

  /**
   * Consome os objetos no canal de eventos.
   * 
   * @return lista de objetos
   */
  public abstract List<Any> consumeAny();

}