using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Services;
using Nop.Plugin.Payments.AfterpayPayment.Controllers;
using Nop.Plugin.Payments.AfterpayPayment.Services;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.Payments.AfterpayPayment.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => int.MaxValue;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<AfterpayUpdateService>().As<IAfterpayUpdateService>().InstancePerLifetimeScope();
            builder.RegisterType<AfterpayPaymentService>().As<IAfterpayPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<OverridenShoppingCartController>().As<ShoppingCartController>().PropertiesAutowired();
        }
    }
}
