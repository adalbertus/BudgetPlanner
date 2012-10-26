using Caliburn.Micro;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.ViewModels;
using Adalbertus.BudgetPlanner.Services;

namespace Adalbertus.BudgetPlanner
{
    internal class ApplicationContainer : WindsorContainer
    {
        public ApplicationContainer()
        {
            //Register(
            //    Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifeStyle.Is(LifestyleType.Singleton),
            //    Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifeStyle.Is(LifestyleType.Singleton),
            //    AllTypes.FromAssemblyContaining<App>().Pick().Configure(x => x.LifeStyle.Is(LifestyleType.Singleton)),
            //    Component.For<ISessionFactory>().Instance(SessionFactory.Create()).LifeStyle.Is(LifestyleType.Singleton)
            //);

            Register(
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<IDatabase>().ImplementedBy<Database.Database>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<IConfigurationManager>().ImplementedBy<ConfigurationManager>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<ICachedService>().ImplementedBy<Database.CachedService>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<IFileService>().ImplementedBy<FileService>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<IExportService>().ImplementedBy<ExportService>().LifeStyle.Is(LifestyleType.Singleton),
                
                AllTypes.FromAssemblyContaining<App>().Pick().WithService.DefaultInterfaces().Configure(x => x.LifeStyle.Is(LifestyleType.Singleton))
            );
        }
    }
}
