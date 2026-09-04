using System;
using System.Collections.Generic;
using System.Linq;

namespace Commander
{
    /// <summary>
    /// Lightweight service locator that keeps track of registered singletons
    /// and disposes them when the application shuts down.
    /// </summary>
    public class ServiceProvider : IDisposable
    {
        private static readonly ServiceProvider _instance = new ServiceProvider();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private bool _disposed;

        public static ServiceProvider Instance => _instance;

        public static void RegisterSingleton<T>(T service)
        {
            _instance.Register<T>(service);
        }

        public static T GetService<T>()
        {
            return _instance.Get<T>();
        }

        private void Register<T>(T service)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProvider));

            if (_instances.ContainsKey(typeof(T)))
                throw new ApplicationException($"Service Provider : {typeof(T)} is already registered!");

            _instances.Add(typeof(T), service);
        }

        private T Get<T>()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceProvider));

            return (T)_instances[typeof(T)];
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            List<object> services;
            lock (_instances)
            {
                services = _instances.Values.ToList();
                _instances.Clear();
            }

            foreach (var service in services)
            {
                if (service is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Best-effort disposal: do not let one failing service
                        // prevent the others from being released.
                    }
                }
            }
        }
    }
}
