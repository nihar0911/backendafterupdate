using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorPerformanceController : ControllerBase
{
    private readonly IVendorPerformanceService _performanceService;
    private readonly ICurrentUserService _currentUserService;

    public VendorPerformanceController(
        IVendorPerformanceService performanceService,
        ICurrentUserService currentUserService)
    {
        _performanceService = performanceService;
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

        var list = await _performanceService.GetOrganizationVendorsPerformanceAsync(orgId);
        return Ok(list);
    }

    [HttpGet("{vendorID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager")]
    public async Task<IActionResult> GetById(int vendorID)
    {
        if (string.Equals(_currentUserService.Role, "Vendor Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (_currentUserService.VendorID.HasValue && vendorID != _currentUserService.VendorID.Value)
            {
                return Forbid();
            }
        }

        int? orgId = null;
        if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            orgId = _currentUserService.OrganizationID.Value;
        }

        var summary = await _performanceService.GetVendorPerformanceAsync(vendorID, orgId);
        if (summary == null)
            return NotFound();

        return Ok(summary);
    }
}
