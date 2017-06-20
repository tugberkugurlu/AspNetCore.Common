using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using TurquoiseSoft.AspNetCore.Common.Configuration.Internal;
using TurquoiseSoft.AspNetCore.Common.Configuration.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureConfigTypes(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
        
            var assemblies = DefaultAssemblyLocator.Locate();
            var configTypes = ConfigSectionAttribute.GetAllAppliedTypes(assemblies);            
            MethodInfo method = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethod("Configure", new Type[] { typeof(IServiceCollection), typeof(IConfiguration) });
            
            foreach (var configTypeInfo in configTypes)
            {
                var configSectionAttribute = configTypeInfo.GetCustomAttribute<ConfigSectionAttribute>();
                var configSection = configuration.GetSection(configSectionAttribute.SectionName);
                
                // https://github.com/aspnet/Options/blob/1.0.0-rc2/src/Microsoft.Extensions.Options.ConfigurationExtensions/OptionsServiceCollectionExtensions.cs#L12-L27
                MethodInfo genericMethod = method.MakeGenericMethod(configTypeInfo.AsType());
                genericMethod.Invoke(null, new object[] { services, configSection });
            }
            
            return services;
        }
    }
}