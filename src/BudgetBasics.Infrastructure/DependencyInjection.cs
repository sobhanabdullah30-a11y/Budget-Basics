using System.ComponentModel.DataAnnotations;
using System.Text;
using BudgetBasics.Application.Common;
using BudgetBasics.Application.Authentication;
using BudgetBasics.Application.Feedback;
using BudgetBasics.Application.Planning;
using BudgetBasics.Application.Assistant;
using BudgetBasics.Application.Learning;
using BudgetBasics.Infrastructure.Assistant;
using BudgetBasics.Infrastructure.Identity;
using BudgetBasics.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BudgetBasics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("The Jwt configuration section is required.");

        Validator.ValidateObject(jwt, new ValidationContext(jwt), validateAllProperties: true);

        // AuthenticationService consumes IOptions<JwtOptions>. Register the same
        // configuration section used by JwtBearer so token creation and token
        // validation always use identical settings.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("bb_access", out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        if (!context.Response.HasStarted &&
                            context.Request.Headers.Accept.Any(value => value?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true))
                        {
                            context.HandleResponse();
                            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                            context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(SecurityPolicies.AdministratorOnly, policy => policy.RequireRole("Administrator"))
            .AddPolicy(SecurityPolicies.AuthenticatedUser, policy => policy.RequireAuthenticatedUser());

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IVisitorCounterService, VisitorCounterService>();
        services.AddScoped<IExpensePlannerService, ExpensePlannerService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ILearningTipService, LearningTipService>();
        services.AddSingleton<BudgetBeeService>();
        services.Configure<GeminiOptions>(options =>
        {
            var section = configuration.GetSection(GeminiOptions.SectionName);
            options.Enabled = section.GetValue("Enabled", true);
            options.ApiKey = section["ApiKey"] ?? configuration["GEMINI_API_KEY"] ?? string.Empty;
            options.Model = section["Model"] ?? "gemini-3.8-flash";
            options.TimeoutSeconds = section.GetValue("TimeoutSeconds", 15);
            options.MaxOutputTokens = section.GetValue("MaxOutputTokens", 350);
        });
        services.AddHttpClient<IBudgetBeeService, GeminiBudgetBeeService>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        });

        return services;
    }
}
