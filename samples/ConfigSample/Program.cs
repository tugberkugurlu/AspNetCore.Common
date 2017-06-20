using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using TurquoiseSoft.AspNetCore.Common.Configuration.Metadata;

namespace ConfigSample
{
    class Program
    {
        static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
            
            return ValidateAndRun(configuration, config =>
            {
                var ownerSettings = config.GetConfigType<OwnerSettings>();
                Console.WriteLine("Hello World {0}!", ownerSettings.Name);

                return 0;
            });
        }
        
        private static int ValidateAndRun(IConfiguration configuration, Func<IConfiguration, int> continuationFunc)
        {
            IEnumerable<ValidationResult> validationResults;
            
            try
            {
                validationResults = configuration.ValidateConfigTypes().ToArray();
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Cannot start without the necessary config values: {ex.Message}");
                return 2;
            }

            if(validationResults.Any())
            {
                foreach (var validationResult in validationResults)
                {
                    Console.Error.WriteLine(validationResult.ErrorMessage);
                }
                
                return 3;
            }
            
            return continuationFunc(configuration);  
        }
    }

    [ConfigSection("Owner")]
    public class OwnerSettings
    {
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        
        [Range(0, 120)]
        public int Age { get; set; }
    }
}
