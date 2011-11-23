using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Caliburn.Micro;
using Castle.Core;
using Castle.Windsor;
using Adalbertus.BudgetPlanner.ViewModels;

namespace Adalbertus.BudgetPlanner
{
    /// <summary>
    /// http://www.iamnotmyself.com/2010/07/31/AnImplementationOfCastleWindsorBootstrapperForCaliburnMicro.aspx
    /// </summary>
    public class CastleBootstrapper : Bootstrapper<IShellViewModel>
    {
        private IWindsorContainer container;

        protected override void Configure()
        {
            container = new ApplicationContainer();            
        }

        protected override object GetInstance(Type service, string key)
        {
            object result = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                result = container.Resolve(service);
            }
            else
            {
                result = container.Resolve(key, service);
            }

            return result;
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[]
                        {
                            Assembly.GetExecutingAssembly(),
                        };

        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var result = (IEnumerable<object>)container.ResolveAll(service);
            return result;
        }

        protected override void BuildUp(object instance)
        {
            container.GetType().GetProperties()
                 .Where(property => property.CanWrite && property.PropertyType.IsPublic)
                 .Where(property => container.Kernel.HasComponent(property.PropertyType))
                 .ForEach(property => property.SetValue(instance, container.Resolve(property.PropertyType), null)
                 );
        }
    }

}
