using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BudgetBasics.Application.Authentication;
using BudgetBasics.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BudgetBasics.Infrastructure.Identity;

internal sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IOptions<JwtOptions> jwtOptions) : IAuthenticationService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<AuthenticationResult> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return new AuthenticationResult(false, "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = displayName.Trim(),
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return new AuthenticationResult(false, string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        await userManager.AddToRoleAsync(user, "Learner");
        return await IssueSessionAsync(user, null, cancellationToken);
    }

    public async Task<AuthenticationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            if (user is not null)
            {
                await userManager.AccessFailedAsync(user);
            }

            return new AuthenticationResult(false, "Email or password is incorrect.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new AuthenticationResult(false, "This account is temporarily locked. Please try again later.");
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await IssueSessionAsync(user, null, cancellationToken);
    }

    public async Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);
        var stored = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            return new AuthenticationResult(false, "Refresh session is invalid.");
        }

        if (!stored.IsActive(DateTimeOffset.UtcNow))
        {
            var family = await dbContext.RefreshTokens
                .Where(token => token.UserId == stored.UserId && token.FamilyId == stored.FamilyId && token.RevokedUtc == null)
                .ToListAsync(cancellationToken);
            foreach (var token in family)
            {
                token.RevokedUtc = DateTimeOffset.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return new AuthenticationResult(false, "This session has expired or was already used. Please sign in again.");
        }

        return await IssueSessionAsync(stored.User, stored, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);
        var stored = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
        if (stored is null)
        {
            return;
        }

        var family = await dbContext.RefreshTokens
            .Where(token => token.UserId == stored.UserId && token.FamilyId == stored.FamilyId && token.RevokedUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var token in family)
        {
            token.RevokedUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthenticationResult> IssueSessionAsync(
        ApplicationUser user,
        RefreshToken? previous,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var accessExpiry = now.AddMinutes(_jwt.AccessTokenMinutes);
        var refreshExpiry = now.AddDays(_jwt.RefreshTokenDays);
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("name", user.DisplayName)
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            _jwt.Issuer,
            _jwt.Audience,
            claims,
            now.UtcDateTime,
            accessExpiry.UtcDateTime,
            credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newRefresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(rawRefreshToken),
            FamilyId = previous?.FamilyId ?? Guid.NewGuid().ToString("N"),
            CreatedUtc = now,
            ExpiresUtc = refreshExpiry
        };

        if (previous is not null)
        {
            previous.RevokedUtc = now;
            previous.ReplacedByTokenHash = newRefresh.TokenHash;
        }

        dbContext.RefreshTokens.Add(newRefresh);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthenticationResult(
            true,
            "Authentication successful.",
            user.DisplayName,
            accessToken,
            rawRefreshToken,
            accessExpiry,
            refreshExpiry);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
