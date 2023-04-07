using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Thoth.Core;
using Thoth.Dashboard;
using Thoth.Dashboard.Filter;
using Thoth.Sample;
using Thoth.SQLServer;

var builder = WebApplication.CreateBuilder(args);

//Testing container
if (builder.Environment.IsEnvironment("Testing"))
{
    if (args.Any(x => x.Contains("UseThothJwtAuthorization") || x.Contains("UseThothJwtAuthorizationWithRoles")))
    {
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtConfiguration.HmacKey)),
                    ValidIssuer = JwtConfiguration.Issuer,
                    ValidAudience = JwtConfiguration.Audience,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                };
            });
        builder.Services.AddAuthorization();
    }
    
    builder.Services.AddControllers();
    builder.Services.AddThoth(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("SqlContext");
        })
        .UseSqlServer();
    
    builder.Services.AddSwaggerGen();
    
    var app = builder.Build();
    
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.UseHttpsRedirection();
    
    if (args.Any(x => x.Contains("UseThothJwtAuthorization") || x.Contains("UseThothJwtAuthorizationWithRoles")))
        app.UseAuthentication();
    
    app.UseAuthorization();
    app.MapControllers();
    app.UseThothDashboard(options =>
    {
        options.RoutePrefix = "/thoth";
        if (args.Any(x => x.Contains("UseThothJwtAuthorization")))
        {
            options.Authorization = new[] {new ThothJwtAuthorizationFilter(new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey
                (
                    Encoding.UTF8.GetBytes(JwtConfiguration.HmacKey)
                ),
                ValidAudience = JwtConfiguration.Audience,
                ValidIssuer = JwtConfiguration.Issuer
            }, cookieOptions: new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                Secure = false,
                HttpOnly = true
            })};
            options.ClaimsToRegisterOnLog = new[] { ClaimTypes.Email, ClaimTypes.NameIdentifier };
        }
        
        if (args.Any(x => x.Contains("UseThothJwtAuthorizationWithRoles")))
            options.Authorization = new[] {new ThothJwtAuthorizationFilter(new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey
                (
                    Encoding.UTF8.GetBytes(JwtConfiguration.HmacKey)
                ),
                ValidAudience = JwtConfiguration.Audience,
                ValidIssuer = JwtConfiguration.Issuer
            }, cookieOptions: new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                Secure = false,
                HttpOnly = true
            }, allowedRoles: new []{ "Admin" })};
    });
    
    app.Run();
}
else
{
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddThoth(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("SqlContext");
        })
        .UseSqlServer();

    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.UseThothDashboard();

    app.Run();   
}

public abstract partial class Program
{
}