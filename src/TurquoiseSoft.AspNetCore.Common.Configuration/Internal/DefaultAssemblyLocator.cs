using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace TurquoiseSoft.AspNetCore.Common.Configuration.Internal
{
    /// <summary>
    /// Locates the assemblies based on <code>RuntimeEnvironment.GetRuntimeIdentifier()</code>.
    /// </summary>
    internal static class DefaultAssemblyLocator
    {
        public static IEnumerable<Assembly> Locate() 
        {               
            return DependencyContext.Default
                .GetRuntimeAssemblyNames(RuntimeEnvironment.GetRuntimeIdentifier())
                .Select(assemblyName => Assembly.Load(assemblyName));    
        }
    }
}