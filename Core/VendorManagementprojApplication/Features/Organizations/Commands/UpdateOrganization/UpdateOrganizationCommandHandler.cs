using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommandHandler
    : IRequestHandler<UpdateOrganizationCommand, UpdateOrganizationResponse>
{
    private readonly IOrganizationRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOrganizationCommandHandler(
        IOrganizationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateOrganizationResponse> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organization =
            await _repository.GetByIdAsync(
                request.OrganizationID);

        if (organization == null)
            return new UpdateOrganizationResponse { Organization = null };

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new UpdateOrganizationResponse { Organization = null };

            if (organization.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new UpdateOrganizationResponse { Organization = null };
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existing =
                await _repository.GetByEmailAsync(
                    request.Email);

            if (existing != null &&
                existing.OrganizationID !=
                request.OrganizationID)
            {
                throw new InvalidOperationException(
                    "An organization with this email already exists.");
            }
        }

        organization.OrganizationName =
            request.OrganizationName;

        organization.Address =
            request.Address;

        organization.Phone =
            request.Phone;

        organization.Email =
            request.Email;

        await _repository.UpdateAsync(
            organization);

        return new UpdateOrganizationResponse
        {
            Organization = new OrganizationDto
            {
                OrganizationID =
                    organization.OrganizationID,

                OrganizationName =
                    organization.OrganizationName,

                Address =
                    organization.Address,

                Phone =
                    organization.Phone,

                Email =
                    organization.Email
            }
        };
    }
}