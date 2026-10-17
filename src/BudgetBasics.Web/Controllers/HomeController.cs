using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BudgetBasics.Web.Models;
using BudgetBasics.Application.Common;

namespace BudgetBasics.Web.Controllers;

public class HomeController(
    IVisitorCounterService visitorCounter,
    ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        long count;
        try
        {
            count = await visitorCounter.RecordVisitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            // Learning content and the client-side practice planner must remain
            // usable during a brief database outage.
            logger.LogError(exception, "The visitor counter could not be updated.");
            count = 0;
        }

        return View(new HomeViewModel(count));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Sitemap()
    {
        return View();
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult Feedback() => View(new FeedbackInput());

    public IActionResult Disclaimer() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
