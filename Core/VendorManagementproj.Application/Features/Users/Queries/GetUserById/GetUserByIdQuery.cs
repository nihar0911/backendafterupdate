using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<GetUserByIdResponse>
{
    public int UserID { get; }

    public GetUserByIdQuery(int userID)
    {
        UserID = userID;
    }
}