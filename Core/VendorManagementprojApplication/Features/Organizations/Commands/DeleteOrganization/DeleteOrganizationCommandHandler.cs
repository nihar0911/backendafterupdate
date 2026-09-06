using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.Organizations.Commands.DeleteOrganization;

public class DeleteOrganizationCommandHandler
    : IRequestHandler<DeleteOrganizationCommand, DeleteOrganizationResponse>
{
    private readonly IOrganizationRepository _repository;

    public DeleteOrganizationCommandHandler(
        IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteOrganizationResponse> Handle(
        DeleteOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organization =
            await _repository.GetByIdAsync(
                request.OrganizationID);

        if (organization == null)
            return new DeleteOrganizationResponse { Success = false };

        await _repository.DeleteAsync(organization);

        return new DeleteOrganizationResponse { Success = true };
    }
}