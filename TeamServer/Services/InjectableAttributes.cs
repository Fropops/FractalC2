using System;
namespace TeamServer.Services
{

    [AttributeUsage(AttributeTargets.Interface)]
    public class InjectableServiceAttribute : Attribute
    {
        public InjectableServiceAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InjectableServiceImplementationAttribute : Attribute
    {
        public Type ServiceType { get; set; }

        public InjectableServiceImplementationAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }


}
