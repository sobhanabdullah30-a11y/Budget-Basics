using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BudgetBasics.Web.Models;
using BudgetBasics.Application.Common;
using Microsoft.AspNetCore.OutputCaching;

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

    [OutputCache(PolicyName = "public-pages")]
    public IActionResult Privacy()
    {
        return View();
    }

    [OutputCache(PolicyName = "public-pages")]
    public IActionResult Sitemap()
    {
        return View();
    }

    [OutputCache(PolicyName = "public-pages")]
    public IActionResult About() => View();

    [OutputCache(PolicyName = "public-pages")]
    public IActionResult Contact() => View();

    public IActionResult Feedback() => View(new FeedbackInput());

    [OutputCache(PolicyName = "public-pages")]
    public IActionResult Disclaimer() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
