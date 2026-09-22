using MediatR;

namespace VendorManagementprojApplication.Features.Quotations.Commands.DeleteQuotation;

public record DeleteQuotationCommand(int QuotationID) : IRequest<DeleteQuotationResponse>;
