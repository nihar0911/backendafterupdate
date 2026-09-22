using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.Products.Commands.CreateProduct;
using VendorManagementproj.Application.Features.Products.Commands.DeleteProduct;
using VendorManagementproj.Application.Features.Products.Commands.UpdateProduct;
using VendorManagementproj.Application.Features.Products.Queries.GetAllProducts;
using VendorManagementproj.Application.Features.Products.Queries.GetProductById;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
   
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _mediator.Send(
                new GetAllProductsQuery());

        return Ok(response);
    }
  
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(int id)
    {
        var response =
            await _mediator.Send(
                new GetProductByIdQuery(id));

        if (response.Product == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateProductCommand command)
    {
        var response =
            await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Product.ProductID },
            response);
    }

   [HttpPut("{id:int}")]
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductCommand command)
    {
        command.ProductID = id;

        var response =
            await _mediator.Send(command);

        if (response.Product == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        var prod = await _mediator.Send(new GetProductByIdQuery(id));
        if (prod.Product == null) return NotFound();

        var updateCmd = new UpdateProductCommand
        {
            ProductID = id,
            ProductName = prod.Product.ProductName,
            Category = prod.Product.Category,
            Unit = prod.Product.Unit,
            TaxRateID = prod.Product.TaxRateID,
            Status = "Active"
        };
        var res = await _mediator.Send(updateCmd);
        return Ok(res);
    }

    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var prod = await _mediator.Send(new GetProductByIdQuery(id));
        if (prod.Product == null) return NotFound();

        var updateCmd = new UpdateProductCommand
        {
            ProductID = id,
            ProductName = prod.Product.ProductName,
            Category = prod.Product.Category,
            Unit = prod.Product.Unit,
            TaxRateID = prod.Product.TaxRateID,
            Status = "Inactive"
        };
        var res = await _mediator.Send(updateCmd);
        return Ok(res);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var response =
            await _mediator.Send(
                new DeleteProductCommand(id));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }
}