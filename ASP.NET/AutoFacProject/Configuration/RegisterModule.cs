using Autofac;
using AutoFac_Test.Services;

namespace AutoFac_Test.Configurations
{
    public class RegisterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EmployeeService>().As<IEmployeeService>();

            // Other Lifetime
            // Transient
            //builder.RegisterType<EmployeeService>().As<IEmployeeService>()
            //    .InstancePerDependency();

            //// Scoped
            builder.RegisterType<EmployeeService>().As<IEmployeeService>()
                .InstancePerLifetimeScope();


            //// Singleton
            //builder.RegisterType<EmployeeService>().As<IEmployeeService>()
            //    .SingleInstance();

        }
    }
}