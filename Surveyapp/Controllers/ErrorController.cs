using Surveyapp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class StatusCodeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public StatusCodeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [AllowAnonymous]
    [HttpGet("/StatusCode/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        IStatusCodeReExecuteFeature reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        _logger.LogInformation($"Unexpected Status Code: {statusCode}, OriginalPath: {reExecute.OriginalPath}");
        return View(statusCode);
    }
}