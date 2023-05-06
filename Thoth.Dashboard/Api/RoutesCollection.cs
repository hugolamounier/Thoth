using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.Dashboard.Api;

public static class RoutesCollection
{
    public static IApplicationBuilder InjectThothDashboardRoutes(
        this IApplicationBuilder app,
        ThothDashboardOptions thothDashboardOptions)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            var scope = endpoints.ServiceProvider.CreateScope();
            var basePath = $"{thothDashboardOptions.RoutePrefix}-api/FeatureFlag";
            var featureManagementService = scope.ServiceProvider.GetRequiredService<IThothFeatureManager>();
            var controller = new FeatureManagerController(featureManagementService, thothDashboardOptions);
            
            #region GET

            endpoints.MapGet(basePath, async () => await controller.GetAll());

            endpoints.MapGet(basePath+ "/{name}", async (string name) =>
                await controller.GetByName(name));

            #endregion

            #region POST

            endpoints.MapPost(basePath, async ([FromBody] FeatureManager featureFlag) =>
                await controller.Create(featureFlag));

            #endregion

            #region PUT

            endpoints.MapPut(basePath, async ([FromBody] FeatureManager featureFlag) =>
                await controller.Update(featureFlag));

            #endregion

            #region DELELTE

            endpoints.MapDelete(basePath + "/{name}", async (string name) =>
                await controller.Delete(name));

            #endregion
        });

        return app;
    }
}