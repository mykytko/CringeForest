using Microsoft.AspNetCore.Mvc;

namespace WebInterface.Pages;

public class IndexController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View("/");
    }
}