using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Products.Commands.CreateProduct;
using VendorManagementprojApplication.Features.Products.Commands.DeleteProduct;
using VendorManagementprojApplication.Features.Products.Commands.UpdateProduct;
using VendorManagementprojApplication.Features.Products.Queries.GetAllProducts;
using VendorManagementprojApplication.Features.Products.Queries.GetProductById;

namespace VendorManagementprojApi.Controllers;

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