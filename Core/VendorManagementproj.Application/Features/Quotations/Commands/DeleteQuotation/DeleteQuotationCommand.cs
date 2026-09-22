using MediatR;

namespace VendorManagementproj.Application.Features.Quotations.Commands.DeleteQuotation;

public record DeleteQuotationCommand(int QuotationID) : IRequest<DeleteQuotationResponse>;
