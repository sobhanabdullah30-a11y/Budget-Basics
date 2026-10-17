using BudgetBasics.Application.Authentication;
using BudgetBasics.Application.Common;
using BudgetBasics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetBasics.Web.Controllers;

public sealed class AccountController(IAuthenticationService authenticationService) : Controller
{
    private const string AntiforgeryCookieName = "__Host-BudgetBasics.Antiforgery";

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authenticationService.RegisterAsync(
            model.DisplayName,
            model.Email,
            model.Password,
            cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        WriteSessionCookies(result);
        DeleteAntiforgeryCookie();
        TempData["StatusMessage"] = $"Welcome, {result.DisplayName}! Your learner account is ready.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authenticationService.LoginAsync(model.Email, model.Password, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        WriteSessionCookies(result);
        DeleteAntiforgeryCookie();
        TempData["StatusMessage"] = $"Welcome back, {result.DisplayName}.";
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home")!);
    }

    [Authorize(Policy = SecurityPolicies.AuthenticatedUser)]
    [HttpGet]
    public IActionResult Profile() => View();

    [HttpPost]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("bb_refresh", out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await authenticationService.RefreshAsync(refreshToken, cancellationToken);
        if (!result.Succeeded)
        {
            DeleteSessionCookies();
            return Unauthorized();
        }

        WriteSessionCookies(result);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue("bb_refresh", out var refreshToken))
        {
            await authenticationService.RevokeAsync(refreshToken, cancellationToken);
        }

        DeleteSessionCookies();
        DeleteAntiforgeryCookie();
        TempData["StatusMessage"] = "You have signed out safely.";
        return RedirectToAction("Index", "Home");
    }

    private void WriteSessionCookies(AuthenticationResult result)
    {
        Response.Cookies.Append("bb_access", result.AccessToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = result.AccessTokenExpiresUtc,
            IsEssential = true
        });
        Response.Cookies.Append("bb_refresh", result.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/Account",
            Expires = result.RefreshTokenExpiresUtc,
            IsEssential = true
        });
    }

    private void DeleteSessionCookies()
    {
        Response.Cookies.Delete("bb_access", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("bb_refresh", new CookieOptions { Path = "/Account" });
    }

    private void DeleteAntiforgeryCookie() =>
        Response.Cookies.Delete(AntiforgeryCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.Lax
        });
}
