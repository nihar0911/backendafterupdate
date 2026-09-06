using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApplication.Features.Outlets.Commands.DeleteOutlet;

public class DeleteOutletCommandHandler
    : IRequestHandler<DeleteOutletCommand, DeleteOutletResponse>
{
    private readonly IOutletRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteOutletCommandHandler(
        IOutletRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteOutletResponse> Handle(
        DeleteOutletCommand request,
        CancellationToken cancellationToken)
    {
        var outlet =
            await _repository.GetByIdAsync(
                request.OutletID);

        if (outlet == null)
            return new DeleteOutletResponse { Success = false };

        if (_currentUserService.IsAdmin)
        {
            var res = await _repository.DeleteAsync(
                request.OutletID);
            return new DeleteOutletResponse { Success = res };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new DeleteOutletResponse { Success = false };

            if (outlet.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new DeleteOutletResponse { Success = false };
            }

            var res = await _repository.DeleteAsync(
                request.OutletID);
            return new DeleteOutletResponse { Success = res };
        }

        return new DeleteOutletResponse { Success = false };
    }
}