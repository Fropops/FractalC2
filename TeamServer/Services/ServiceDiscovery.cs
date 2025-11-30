
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TeamServer.Services
{
    public static class ServiceDiscovery
    {
        public static Dictionary<Type, Type> DiscoverInjectableServices(Assembly assembly)
        {
            var result = new Dictionary<Type, Type>();

            // Récupérer toutes les interfaces avec l'attribut InjectableService
            var injectableInterfaces = assembly.GetTypes()
                .Where(t => t.IsInterface &&
                           t.GetCustomAttribute<InjectableServiceAttribute>() != null)
                .ToList();

            foreach (var serviceInterface in injectableInterfaces)
            {
                var attribute = serviceInterface.GetCustomAttribute<InjectableServiceAttribute>();

                // Trouver l'implémentation correspondante
                var implementation = assembly.GetTypes()
                    .FirstOrDefault(t =>
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.GetCustomAttribute<InjectableServiceImplementationAttribute>()?.ServiceType == serviceInterface);

                if (implementation != null)
                {
                    result.Add(serviceInterface, implementation);
                }
                else
                {
                    // Alternative : chercher par convention (implémente l'interface)
                    implementation = assembly.GetTypes()
                        .FirstOrDefault(t =>
                            t.IsClass &&
                            !t.IsAbstract &&
                            serviceInterface.IsAssignableFrom(t) &&
                            t.GetCustomAttribute<InjectableServiceImplementationAttribute>() != null);

                    if (implementation != null)
                    {
                        result.Add(serviceInterface, implementation);

                    }
                }
            }

            return result;
        }
    }
}

