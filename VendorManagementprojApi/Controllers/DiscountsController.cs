using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Discounts.Commands.CreateDiscount;
using VendorManagementprojApplication.Features.Discounts.Commands.DeleteDiscount;
using VendorManagementprojApplication.Features.Discounts.Commands.UpdateDiscount;
using VendorManagementprojApplication.Features.Discounts.Queries.GetAllDiscounts;
using VendorManagementprojApplication.Features.Discounts.Queries.GetDiscountById;
using VendorManagementprojApplication.Features.Discounts.Queries.GetEffectivePrice;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiscountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllDiscountsQuery());
        return Ok(response);
    }

    [HttpGet("{discountID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetById(int discountID)
    {
        var response = await _mediator.Send(new GetDiscountByIdQuery(discountID));

        if (response.Discount == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Create(CreateDiscountCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { discountID = response.Discount.DiscountID },
                response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{discountID:int}")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Update(
        int discountID,
        UpdateDiscountCommand command)
    {
        try
        {
            command.DiscountID = discountID;
            var response = await _mediator.Send(command);

            if (response.Discount == null)
                return NotFound();

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{discountID:int}")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Delete(int discountID)
    {
        var response = await _mediator.Send(new DeleteDiscountCommand(discountID));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }

    [HttpGet("effective-price/{vendorID:int}/{productID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetEffectivePrice(
        int vendorID,
        int productID,
        [FromQuery] decimal quantity)
    {
        try
        {
            var response = await _mediator.Send(new GetEffectivePriceQuery(vendorID, productID, quantity));

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}