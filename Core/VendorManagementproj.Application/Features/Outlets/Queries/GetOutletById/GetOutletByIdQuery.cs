using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Outlets.Queries.GetOutletById;

public record GetOutletByIdQuery(int OutletID) : IRequest<GetOutletByIdResponse>;
