using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Quotations.Queries.GetQuotationById;

public record GetQuotationByIdQuery(int QuotationID) : IRequest<GetQuotationByIdResponse>;
