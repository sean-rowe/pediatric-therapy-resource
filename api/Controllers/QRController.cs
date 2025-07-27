using Microsoft.AspNetCore.Mvc;

namespace UPTRMS.Api.Controllers;

[ApiController]
[Route("api/qr")]
public class QRController : ControllerBase
{
    [HttpGet("scan/{code}")]
    public IActionResult ScanQRCode(string code)
    {
        return StatusCode(501, new { message = "QR code scanning not yet implemented - UPTRMS in development" });
    }
}