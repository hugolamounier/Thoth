#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Core.Models.Entities;

namespace Thoth.Core.Interfaces;

public interface IThothFeatureManager
{
    /// <summary>
    ///     Checks if a feature is enabled.
    /// </summary>
    /// <param name="featureName"></param>
    /// <returns></returns>
    Task<bool> IsEnabledAsync(string featureName);

    /// <summary>
    ///     Retrieves the value of an Environment variable
    /// </summary>
    /// <param name="featureName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ThothException"></exception>
    Task<T> GetEnvironmentValueAsync<T>(string featureName);

    /// <summary>
    ///     Retrieves the feature by its Id
    /// </summary>
    /// <param name="featureName"></param>
    /// <returns></returns>
    /// <exception cref="ThothException"></exception>
    Task<FeatureManager?> GetAsync(string featureName);

    /// <summary>
    ///     Retrieves all features
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<FeatureManager>> GetAllAsync();

    /// <summary>
    ///     Creates a new feature
    /// </summary>
    /// <param name="featureManager"></param>
    /// <returns></returns>
    /// <exception cref="ThothException"></exception>
    Task<bool> AddAsync(FeatureManager featureManager);

    /// <summary>
    ///     Updates an existing feature
    /// </summary>
    /// <param name="featureManager"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(FeatureManager featureManager);

    /// <summary>
    ///     Deletes an existing feature
    /// </summary>
    /// <param name="featureName"></param>
    /// <param name="auditExtras"></param>
    /// <returns></returns>
    /// <exception cref="ThothException"></exception>
    Task<bool> DeleteAsync(string featureName, string auditExtras = "");
}