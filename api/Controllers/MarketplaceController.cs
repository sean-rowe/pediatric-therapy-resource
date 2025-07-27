using Microsoft.AspNetCore.Mvc;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/marketplace")]
public class MarketplaceController : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        throw new NotImplementedException("GetProducts endpoint not yet implemented");
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        throw new NotImplementedException("GetProduct endpoint not yet implemented");
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseProduct()
    {
        throw new NotImplementedException("PurchaseProduct endpoint not yet implemented");
    }

    [HttpGet("seller/dashboard")]
    public async Task<IActionResult> GetSellerDashboard()
    {
        throw new NotImplementedException("GetSellerDashboard endpoint not yet implemented");
    }

    [HttpPost("seller/apply")]
    public async Task<IActionResult> ApplyToBeSeller()
    {
        throw new NotImplementedException("ApplyToBeSeller endpoint not yet implemented");
    }
}