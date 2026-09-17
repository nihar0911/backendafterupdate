using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.Features.VendorPerformance.Queries.GetOrganizationVendorsPerformance;
using VendorManagementprojApplication.Features.VendorPerformance.Queries.GetVendorPerformanceById;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorPerformanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public VendorPerformanceController(
        IMediator mediator,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> GetAll()
    {
        int? orgId = null;
        if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            orgId = _currentUserService.OrganizationID.Value;
        }

        var response = await _mediator.Send(new GetOrganizationVendorsPerformanceQuery
        {
            OrganizationID = orgId
        });
        return Ok(response.VendorPerformances);
    }

    [HttpGet("{vendorID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager")]
    public async Task<IActionResult> GetById(int vendorID)
    {
        if (string.Equals(_currentUserService.Role, "Vendor Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (!_currentUserService.VendorID.HasValue || vendorID != _currentUserService.VendorID.Value)
            {
                return Forbid();
            }
        }

        int? orgId = null;
        if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            orgId = _currentUserService.OrganizationID.Value;
        }

        var response = await _mediator.Send(new GetVendorPerformanceByIdQuery
        {
            VendorID = vendorID,
            OrganizationID = orgId
        });

        if (response.VendorPerformance == null)
            return NotFound();

        return Ok(response.VendorPerformance);
    }
}
