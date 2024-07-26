using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.GemVisaPayment.Services;

namespace Nop.Plugin.Payments.GemVisaPayment.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1000;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<GemVisaPaymentService>().As<IGemVisaPaymentService>().InstancePerLifetimeScope();
        }
    }
}
