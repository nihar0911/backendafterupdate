using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetAllQuotations;

public record GetAllQuotationsQuery : IRequest<GetAllQuotationsResponse>;
