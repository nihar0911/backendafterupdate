using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Queries.GetAllQuotations;

public record GetAllQuotationsQuery : IRequest<GetAllQuotationsResponse>;
