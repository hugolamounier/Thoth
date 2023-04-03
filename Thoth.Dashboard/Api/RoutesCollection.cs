using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Dashboard.Api;

public static class RoutesCollection
{
    public static IApplicationBuilder InjectThothDashboardRoutes(this IApplicationBuilder app, IServiceScope scope, string routePrefix)
    {
        var basePath = $"{routePrefix}-api/FeatureFlag";
        var featureManagementService = scope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
        var featureFlagController = new FeatureFlagController(featureManagementService);

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