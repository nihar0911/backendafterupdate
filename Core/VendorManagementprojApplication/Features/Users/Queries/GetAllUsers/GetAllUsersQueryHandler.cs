using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, GetAllUsersResponse>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetAllUsersResponse> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();

        var list = users.Select(user => new UserDto
        {
            UserID = user.UserID,
            Name = user.Name,
            Email = user.Email,
            RoleID = user.RoleID,
            RoleName = user.Role?.RoleName,
            OrganizationID = user.OrganizationID,
            OutletID = user.OutletID,
            VendorID = user.VendorID
        }).ToList();

        return new GetAllUsersResponse
        {
            Users = list
        };
    }
}