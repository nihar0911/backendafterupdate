using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetOutletById;

public record GetOutletByIdQuery(int OutletID) : IRequest<GetOutletByIdResponse>;
