using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

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
        var httpAccessorContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        var featureFlagController = new FeatureFlagController(featureManagementService, logger, httpAccessorContext, thothDashboardOptions);

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            #region GET

            endpoints.MapGet(basePath, async () => await featureFlagController.GetAll());

            endpoints.MapGet(basePath+ "/{name}", async (string name) => await featureFlagController.GetByName(name));

            #endregion

            #region POST

            endpoints.MapPost(basePath, async ([FromBody] FeatureFlag featureFlag) =>
                await featureFlagController.Create(featureFlag));

            #endregion

            #region PUT

            endpoints.MapPut(basePath, async ([FromBody] FeatureFlag featureFlag) => await featureFlagController.Update(featureFlag));

            #endregion

            #region DELELTE

            endpoints.MapDelete(basePath + "/{name}", async (string name) => await featureFlagController.Delete(name));

            #endregion
        });

        return app;
    }
}