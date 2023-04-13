using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.Dashboard.Api;

public static class RoutesCollection
{
    public static IApplicationBuilder InjectThothDashboardRoutes(
        this IApplicationBuilder app,
        IServiceScope scope,
        ThothDashboardOptions thothDashboardOptions)
    {
        var basePath = $"{thothDashboardOptions.RoutePrefix}-api/FeatureFlag";
        var featureManagementService = scope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FeatureFlagController>>();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            #region GET

            endpoints.MapGet(basePath, async (HttpContext httpContext) =>
            {
                return await new FeatureFlagController(featureManagementService, logger, httpContext, thothDashboardOptions)
                    .GetAll();
            });

            endpoints.MapGet(basePath+ "/{name}", async (HttpContext httpContext, string name) => 
                await new FeatureFlagController(featureManagementService, logger, httpContext, thothDashboardOptions).GetByName(name));

            #endregion

            #region POST

            endpoints.MapPost(basePath, async (HttpContext httpContext, [FromBody] FeatureManager featureFlag) =>
                await new FeatureFlagController(featureManagementService, logger, httpContext, thothDashboardOptions).Create(featureFlag));
                

            #endregion

            #region PUT

            endpoints.MapPut(basePath, async (HttpContext httpContext, [FromBody] FeatureManager featureFlag) =>
                await new FeatureFlagController(featureManagementService, logger, httpContext, thothDashboardOptions).Update(featureFlag));

            #endregion

            #region DELELTE

            endpoints.MapDelete(basePath + "/{name}", async (HttpContext httpContext, string name) =>
                await new FeatureFlagController(featureManagementService, logger, httpContext, thothDashboardOptions).Delete(name));

            #endregion
        });

        return app;
    }
}