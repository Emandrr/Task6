using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Task6.Hubs;
using Task6.Models;
using Task6.Contracts;
using Task6.Services;
namespace Task6.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PresentationService _presentationService;
    public HomeController(ILogger<HomeController> logger, PresentationService presentationService)
    {
        _logger = logger;
        _presentationService = presentationService;
    }
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,Author")] PresentationDTO presentation)
    {
        if (ModelState.IsValid)
        {
            await _presentationService.Create(presentation);

            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Index()
    {
        return View(_presentationService.ListAll());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [HttpGet("Home/RedirectToPresentation/{id}")]
    public IActionResult RedirectToPresentation(int id)
    {
        if (id == 0) id = _presentationService.GetAll();
        return RedirectToAction("Index", "Edit", new { id = id });
    }
}
