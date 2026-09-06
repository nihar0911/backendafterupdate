using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandHandler
    : IRequestHandler<CreateOrganizationCommand, CreateOrganizationResponse>
{
    private readonly IOrganizationRepository _repository;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateOrganizationResponse> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existing =
                await _repository.GetByEmailAsync(request.Email);

            if (existing != null)
            {
                throw new InvalidOperationException(
                    "An organization with this email already exists.");
            }
        }

        var organization = new Organization
        {
            OrganizationName = request.OrganizationName,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email
        };

        await _repository.AddAsync(organization);

        var dto = new OrganizationDto
        {
            OrganizationID = organization.OrganizationID,
            OrganizationName = organization.OrganizationName,
            Address = organization.Address,
            Phone = organization.Phone,
            Email = organization.Email
        };

        return new CreateOrganizationResponse
        {
            Organization = dto
        };
    }
}
