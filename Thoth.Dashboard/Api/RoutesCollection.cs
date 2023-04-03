using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Dashboard.Api;

public static class RoutesCollection
{
    public static WebApplication InjectThothDashboardRoutes(this WebApplication app, IServiceScope scope, string routePrefix)
    {
        var basePath = $"{routePrefix}-api/FeatureFlag";
        var featureManagementService = scope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
        var featureFlagController = new FeatureFlagController(featureManagementService);

        app.UseRouting();

        #region GET

        app.MapGet(basePath, async () => await featureFlagController.GetAll());

        app.MapGet(basePath+ "/{name}", async (string name) => await featureFlagController.GetByName(name));

        #endregion

        #region POST

        app.MapPost(basePath, async ([FromBody] FeatureFlag featureFlag) =>
            await featureFlagController.Create(featureFlag));

        #endregion

        #region PUT

        app.MapPut(basePath, async ([FromBody] FeatureFlag featureFlag) => await featureFlagController.Update(featureFlag));

        #endregion

        #region DELELTE

        app.MapDelete(basePath + "/{name}", async (string name) => await featureFlagController.Delete(name));

        #endregion

        return app;
    }
}