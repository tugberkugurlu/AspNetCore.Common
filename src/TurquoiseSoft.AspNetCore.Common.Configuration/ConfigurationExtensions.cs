using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using TurquoiseSoft.AspNetCore.Common.Configuration.Internal;
using TurquoiseSoft.AspNetCore.Common.Configuration.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static TConfigType GetConfigType<TConfigType>(this IConfiguration configuration)
        {
            var assemblies = DefaultAssemblyLocator.Locate();
            var configTypes = ConfigSectionAttribute.GetAllAppliedTypes(assemblies);
            var configType = configTypes.FirstOrDefault(x => x.AsType() == typeof(TConfigType));
            
            if(configType != null)
            {   
                var configSectionAttribute = configType.GetCustomAttribute<ConfigSectionAttribute>();
                var configInstance = Activator.CreateInstance(typeof(TConfigType));
                var section = configuration.GetSection(configSectionAttribute.SectionName);
                
                ConfigurationBinder.Bind(section, configInstance);
                
                return (TConfigType)configInstance;   
            }
            else 
            {
                throw new InvalidOperationException($"'{typeof(TConfigType).FullName}' is not recognized as a config type.");    
            }
        }
        
        /// <remarks>
        /// For usage of <code>ILibraryManager</code>, see: https://github.com/aspnet/SignalR-Server/blob/dae3e5331081093e038ba0d8bc74ccd8d9046a35/src/Microsoft.AspNetCore.SignalR.Server/Hubs/DefaultAssemblyLocator.cs
        /// </remarks>
        public static IEnumerable<ValidationResult> ValidateConfigTypes(this IConfiguration configuration) 
        {
            var assemblies = DefaultAssemblyLocator.Locate();
            var configTypes = ConfigSectionAttribute.GetAllAppliedTypes(assemblies);
            
            return configTypes.Select(configTypeInfo => 
            {   
                var configSectionAttribute = configTypeInfo.GetCustomAttribute<ConfigSectionAttribute>();
                var configInstance = Activator.CreateInstance(configTypeInfo.AsType());
                var section = configuration.GetSection(configSectionAttribute.SectionName);

                ConfigurationBinder.Bind(section, configInstance);

                if(configInstance != null)
                {
                    return Validate(configInstance);
                }
                else 
                {
                    throw new InvalidOperationException($"No config values for {configTypeInfo.FullName}.");
                }
                
            }).SelectMany(validationResults => validationResults);
        }
        
        private static IEnumerable<ValidationResult> Validate(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            
            ICollection<ValidationResult> validationResults = new Collection<ValidationResult>();
            var validationContext = new ValidationContext(instance);
            Validator.TryValidateObject(instance, validationContext, validationResults, true);
            
            return validationResults;
        }
    }
}