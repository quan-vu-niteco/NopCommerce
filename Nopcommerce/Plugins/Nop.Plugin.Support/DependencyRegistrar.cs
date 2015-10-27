using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Widgets.Support.Data;
using Nop.Plugin.Widgets.Support.Domain;
using Nop.Plugin.Widgets.Support.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.Support
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<SupportService>().As<ISupportService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<SupportObjectContext>(builder, "nop_object_context_support_zip");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<SupportItem>>()
                .As<IRepository<SupportItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_support_zip"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
