using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojApplication.Features.VendorFeedback.Commands.CreateVendorFeedback;
using VendorManagementprojApplication.Features.VendorFeedback.Queries.GetAllVendorFeedback;
using VendorManagementprojApplication.Features.VendorFeedback.Queries.GetEligibleReviewOrders;
using VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackById;
using VendorManagementprojApplication.Features.VendorFeedback.Queries.GetVendorFeedbackByVendor;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorFeedbackController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IVendorFeedbackRepository _feedbackRepository;
    private readonly ICurrentUserService _currentUserService;

    public VendorFeedbackController(
        IMediator mediator,
        IVendorFeedbackRepository feedbackRepository,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _feedbackRepository = feedbackRepository;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> Create([FromBody] CreateVendorFeedbackCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllVendorFeedbackQuery());
        return Ok(response);
    }

    [HttpGet("{feedbackID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(int feedbackID)
    {
        try
        {
            var response = await _mediator.Send(new GetVendorFeedbackByIdQuery
            {
                FeedbackID = feedbackID
            });

            if (response.Feedback == null)
                return NotFound();

            if (string.Equals(_currentUserService.Role, "Vendor Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (_currentUserService.VendorID.HasValue && response.Feedback.VendorID != _currentUserService.VendorID.Value)
                {
                    return Forbid();
                }
            }

            if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
            {
                var feedbackEntity = await _feedbackRepository.GetByIdAsync(feedbackID);
                if (feedbackEntity?.Outlet != null && feedbackEntity.Outlet.OrganizationID != _currentUserService.OrganizationID.Value)
                {
                    return Forbid();
                }
            }

            if (_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager)
            {
                if (!_currentUserService.OutletID.HasValue || response.Feedback.OutletID != _currentUserService.OutletID.Value)
                {
                    return Forbid();
                }
            }

            return Ok(response.Feedback);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("vendor/{vendorID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetByVendor(int vendorID, [FromQuery] int? productId = null)
    {
        if (string.Equals(_currentUserService.Role, "Vendor Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (!_currentUserService.VendorID.HasValue || vendorID != _currentUserService.VendorID.Value)
            {
                return Forbid();
            }
        }

        var response = await _mediator.Send(new GetVendorFeedbackByVendorQuery
        {
            VendorID = vendorID,
            ProductID = productId
        });

        return Ok(response.Feedback);
    }

    [HttpGet("eligible-orders")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetEligibleOrders()
    {
        var response = await _mediator.Send(new GetEligibleReviewOrdersQuery());
        return Ok(response.Orders);
    }
}

