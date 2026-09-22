using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Quotations.Queries.GetQuotationById;

public record GetQuotationByIdQuery(int QuotationID) : IRequest<GetQuotationByIdResponse>;
