using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, DeleteDiscountResponse>
{
    private readonly IDiscountRepository _repository;

    public DeleteDiscountCommandHandler(IDiscountRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteDiscountResponse> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteAsync(request.DiscountID);
        return new DeleteDiscountResponse
        {
            Success = success
        };
    }
}
