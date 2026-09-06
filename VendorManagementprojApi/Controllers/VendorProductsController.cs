using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Features.VendorProducts.Commands.CreateVendorProduct;
using VendorManagementprojApplication.Features.VendorProducts.Commands.DeleteVendorProduct;
using VendorManagementprojApplication.Features.VendorProducts.Commands.UpdateVendorProduct;
using VendorManagementprojApplication.Features.VendorProducts.Queries.GetAllVendorProducts;
using VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductById;
using VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorsByProduct;
using VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductsByProductName;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/vendorproducts")]
[Authorize]
public class VendorProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IVendorProductRepository _repository;

    public VendorProductsController(
        IMediator mediator,
        IVendorProductRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpGet]
    [Authorize(
        Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _mediator.Send(new GetAllVendorProductsQuery());

        return Ok(response);
    }

    [HttpGet("product/search")]
    [Authorize(
        Roles = "Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> SearchByProductName( [FromQuery] string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return BadRequest(
                "Product name is required.");
        }

        var response =
            await _mediator.Send(
                new GetVendorProductsByProductNameQuery(productName));

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Create(
        CreateVendorProductCommand command)
    {
        var response =
            await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.VendorProduct!.VendorProductID },
            response);
    }

    [HttpGet("search")]
    [Authorize(
        Roles = "Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> Search(
        [FromQuery] string productName,
        [FromQuery] int outletID)
    {
        var response =
            await _mediator.Send(
                new GetVendorsByProductQuery
                {
                    ProductName = productName,
                    OutletID = outletID
                });

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(
        Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(int id)
    {
        var response =
            await _mediator.Send(
                new GetVendorProductByIdQuery(id));

        if (response.VendorProduct == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Update(
        int id,
        UpdateVendorProductCommand command)
    {
        command.VendorProductID = id;

        var response =
            await _mediator.Send(command);

        if (response.VendorProduct == null)
            return NotFound();

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var response =
            await _mediator.Send(
                new DeleteVendorProductCommand(id));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }
}