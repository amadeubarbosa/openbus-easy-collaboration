package lib;

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
public interface EasyCollabing {

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