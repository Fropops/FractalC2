using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    public static class ServiceProvider
    {
        private static Dictionary<Type, object> instances = new Dictionary<Type, object>();
        public static void RegisterSingleton<T>(T service)
        {
            if (instances.ContainsKey(typeof(T)))
                throw new ApplicationException($"Service Provider : {typeof(T).ToString()} is already registerd!");

            instances.Add(typeof(T), service);
        }


        public static T GetService<T>()
        {
            return (T)instances[typeof(T)];
        }
    }
}
