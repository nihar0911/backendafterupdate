using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserByIdResponse> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserID);

        if (user == null)
            return new GetUserByIdResponse { User = null };

        return new GetUserByIdResponse
        {
            User = new UserDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName,
                OrganizationID = user.OrganizationID,
                OutletID = user.OutletID,
                VendorID = user.VendorID
            }
        };
    }
}