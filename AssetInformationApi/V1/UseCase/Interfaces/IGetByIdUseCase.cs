using AssetInformationApi.V1.Boundary.Response;

namespace AssetInformationApi.V1.UseCase.Interfaces
{
    public interface IGetByIdUseCase
    {
        ResponseObject Execute(int id);
    }
}
