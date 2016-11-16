using System.Collections.Generic;
using tecgraf.openbus.core.v2_0.services;
using tecgraf.openbus.services.collaboration.v1_0;
using omg.org.CORBA;

namespace tecgraf.openbus.easycollab{
  /// <summary>
  /// Interface de apoio ao Serviço de Colaboração.
  /// </summary>
  /// Interface de apoio ao Serviço de Colaboração.
  /// O objetivo dessa interface é fornecer alguns métodos a serem utilizados
  /// nas atividades mais corriqueiras relacionadas ao uso de sessões de colaboração.
  public interface IEasyCollaboration{
    /// <summary>
    /// Inicia uma sessão de colaboração.
    /// </summary>
    /// Inicia uma sessão de colaboração.
    /// Esse método deve ser responsável por buscar o serviço de colaboração, criar 
    /// uma sessão e registrá-la no registro de sessões. Além disso, um consumidor deve ser
    /// cadastrado no canal de eventos da sessão para a troca de informações.
    /// <returns>A sessão de colaboração.</returns>
    /// <exception cref="ServiceFailure">Pode ser lançada pelo Serviço de Colaboração, indicando uma falha ao criar ou registrar a sessão.</exception>
    CollaborationSession StartCollaboration();

    /// <summary>
    /// Encerra a sessão de colaboração.
    /// </summary>
    /// Encerra a sessão de colaboração.
    /// Também remove observadores e consumidores do canal de eventos.
    /// <exception cref="ServiceFailure">Pode ser lançada pelo Serviço de Colaboração, indicando uma falha ao sair da sessão.</exception>
    void ExitCollaboration();

    /// <summary>
    /// Compartilha vários datakeys no canal de eventos.
    /// </summary>
    /// <param name="keys">Os datakeys.</param>
    /// <exception cref="ServiceFailure">Pode ser lançada pelo Serviço de Colaboração, indicando uma falha ao compartilhar as datakeys.</exception>
    void ShareDataKeys(List<byte[]> keys);

    /// <summary>
    /// Compartilha um objeto, desde que encapsulado com o tipo variável Any de CORBA, no canal de eventos.
    /// </summary>
    /// <param name="any">Um objeto a ser compartilhado.</param>
    /// <exception cref="ServiceFailure">Pode ser lançada pelo Serviço de Colaboração, indicando uma falha ao compartilhar um objeto.</exception>
    void Share(Any any);

    /// <summary>
    /// Consome os datakeys no canal de eventos.
    /// </summary>
    /// <returns>Lista de datakeys.</returns>
    /// <exception cref="ServiceFailure">Pode ser lançada pelo Serviço de Colaboração, indicando uma falha ao fornecer datakeys.</exception>
    List<byte[]> ConsumeDataKeys();

    /// <summary>
    /// Consome objetos, encapsulados com o tipo variável Any de CORBA, do canal de eventos.
    /// </summary>
    /// <returns>Lista de Any.</returns>
    /// <exception cref="ServiceFailure">Pode ser lançada pelo Serviço de Colaboração, indicando uma falha ao fornecer objetos.</exception>
    List<object> ConsumeAnys();
  }
}
