using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Thoth.Core;
using Thoth.Dashboard;
using Thoth.Dashboard.Filter;
using Thoth.Sample;
using Thoth.SQLServer;
using Thoth.Tests.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (args.Any(x => x.Contains("UseThothJwtAuthorization")))
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
builder.Services.AddThoth(options => { options.ConnectionString = builder.Configuration.GetConnectionString("SqlContext"); })
    .UseSqlServer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

if (args.Any(x => x.Contains("UseThothJwtAuthorization")))
    app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();
app.UseThothDashboard(options =>
{
    options.RoutePrefix = "/thoth";
    if (args.Any(x => x.Contains("UseThothAuthorization")))
        options.Authorization = new[] {new ThothAuthorizationFilter()};

    if (args.Any(x => x.Contains("UseThothJwtAuthorization")))
        options.Authorization = new[] {new ThothJwtAuthorizationFilter()};
});

app.Run();

public abstract partial class Program
{
}