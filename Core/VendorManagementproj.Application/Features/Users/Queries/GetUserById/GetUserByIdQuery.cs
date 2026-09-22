using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<GetUserByIdResponse>
{
    public int UserID { get; }

    public GetUserByIdQuery(int userID)
    {
        UserID = userID;
    }
}