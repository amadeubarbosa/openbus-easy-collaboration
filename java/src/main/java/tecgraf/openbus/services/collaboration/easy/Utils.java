package tecgraf.openbus.services.collaboration.easy;

import tecgraf.openbus.core.v2_0.services.ServiceFailure;
import tecgraf.openbus.core.v2_0.services.offer_registry.OfferRegistry;
import tecgraf.openbus.core.v2_0.services.offer_registry.ServiceOfferDesc;
import tecgraf.openbus.core.v2_0.services.offer_registry.ServiceProperty;

import java.util.ArrayList;
import java.util.List;

/**
 * Métodos utilitários ao uso da biblioteca e conceitos OpenBus
 *
 * @author Tecgraf/PUC-Rio
 */
class Utils {

  /**
   * Busca por uma propriedade dentro da lista de propriedades
   * 
   * @param props a lista de propriedades
   * @param key a chave da propriedade buscada
   * @return o valor da propriedade ou <code>null</code> caso não encontrada
   */
  static String findProperty(ServiceProperty[] props, String key) {
    for (int i = 0; i < props.length; i++) {
      ServiceProperty property = props[i];
      if (property.name.equals(key)) {
        return property.value;
      }
    }
    return null;
  }

  /**
   * Realiza um busca por ofertas com as propriedades solicitadas, repeitando as
   * regras de retentativas e espera entre tentativas. Caso as ofertas não sejam
   * encontradas lança-se uma exceção de {@link IllegalStateException}
   * 
   * @param offers serviço de registro de ofertas do barramento
   * @param search propriedades da busca
   * @param count número mínimo de ofertas que se espera encontrar
   * @param tries número de tentativas
   * @param interval intervalo de espera entre as tentativas (em segundos)
   * @return ofertas encontradas na busca.
   * @throws ServiceFailure
   */
  static List<ServiceOfferDesc> findOffer(OfferRegistry offers,
    ServiceProperty[] search, int count, int tries, int interval)
    throws ServiceFailure {
    List<ServiceOfferDesc> found = new ArrayList<ServiceOfferDesc>();
    for (int i = 0; i < tries; i++) {
      found.clear();
      try {
        Thread.sleep(interval * 1000);
      }
      catch (InterruptedException e1) {
        // continue...
      }
      ServiceOfferDesc[] services = offers.findServices(search);
      if (services.length > 0) {
        for (ServiceOfferDesc offerDesc : services) {
          try {
            if (!offerDesc.service_ref._non_existent()) {
              found.add(offerDesc);
            }
          }
          catch (Exception e) {
            continue;
          }
        }
      }
      if (found.size() >= count) {
        return found;
      }
    }
    StringBuffer buffer = new StringBuffer();
    for (ServiceOfferDesc desc : found) {
      String name =
        Utils.findProperty(desc.properties, "openbus.offer.entity");
      String login =
        Utils.findProperty(desc.properties, "openbus.offer.login");
      buffer.append(String.format("\n - %s (%s)", name, login));
    }
    String msg =
      String
        .format(
          "não foi possível encontrar ofertas: found (%d) expected(%d) tries (%d) time (%d)%s",
          found.size(), count, tries, tries * interval, buffer.toString());
    throw new IllegalStateException(msg);
  }

}
