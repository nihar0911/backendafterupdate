using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Invoices.Commands.ApproveInvoice;
using VendorManagementprojApplication.Features.Invoices.Commands.CreateInvoice;
using VendorManagementprojApplication.Features.Invoices.Commands.MarkInvoicePaid;
using VendorManagementprojApplication.Features.Invoices.Commands.RejectInvoice;
using VendorManagementprojApplication.Features.Invoices.Queries.GetAllInvoices;
using VendorManagementprojApplication.Features.Invoices.Queries.GetInvoiceById;
using VendorManagementprojApplication.Features.Payments.Queries.GetPayments;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllInvoicesQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("{invoiceID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(int invoiceID)
    {
        try
        {
            var result = await _mediator.Send(new GetInvoiceByIdQuery
            {
                InvoiceID = invoiceID
            });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("{invoiceID:int}/approve")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Approve(int invoiceID)
    {
        try
        {
            var result = await _mediator.Send(new ApproveInvoiceCommand
            {
                InvoiceID = invoiceID
            });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("{invoiceID:int}/reject")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Reject(int invoiceID, [FromBody] RejectInvoiceRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RejectInvoiceCommand
            {
                InvoiceID = invoiceID,
                Reason = request?.Reason ?? string.Empty
            });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("{invoiceID:int}/download")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> DownloadPdf(int invoiceID)
    {
        try
        {
            var result = await _mediator.Send(new GetInvoiceByIdQuery
            {
                InvoiceID = invoiceID
            });

            if (result?.Invoice == null || string.IsNullOrWhiteSpace(result.Invoice.InvoiceDocumentBase64))
            {
                return NotFound("Invoice document not found.");
            }

            byte[] pdfBytes = Convert.FromBase64String(result.Invoice.InvoiceDocumentBase64);
            string fileName = string.IsNullOrWhiteSpace(result.Invoice.InvoiceFileName) ? $"Invoice-{invoiceID}.pdf" : result.Invoice.InvoiceFileName;
            string contentType = string.IsNullOrWhiteSpace(result.Invoice.InvoiceContentType) ? "application/pdf" : result.Invoice.InvoiceContentType;

            return File(pdfBytes, contentType, fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("pay")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> MarkPaid([FromBody] MarkInvoicePaidCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("payments")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetPayments()
    {
        var result = await _mediator.Send(new GetPaymentsQuery());
        return Ok(result);
    }
}

public class RejectInvoiceRequest
{
    public string Reason { get; set; } = string.Empty;
}