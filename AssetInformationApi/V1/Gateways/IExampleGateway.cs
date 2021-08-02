using System.Collections.Generic;
using AssetInformationApi.V1.Domain;

namespace AssetInformationApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
