using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Caliburn.Micro;
using Castle.Core;
using Castle.Windsor;
using Adalbertus.BudgetPlanner.ViewModels;
using Microsoft.Windows.Controls;

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

        static CastleBootstrapper()
        {
            LogManager.GetLog = type => new Core.NLogLogger(type);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.GetLog(typeof(Core.NLogLogger)).Error(e.ExceptionObject as Exception);
            var sb = new StringBuilder();
            sb.AppendLine("Wystąpił nieokreślony błąd. Niestety aplikacja musi zostać zamknięta.");
            sb.AppendLine();
            sb.AppendLine("Proszę o przesłanie pliku z zebranymi informacjami o błędzie do autora.");
            sb.AppendLine(string.Format("Plik z informacjami: {0}", Core.NLogLogger.LogFileName));
            sb.AppendLine();
            sb.AppendLine("Być może uruchomienie aplikacji ze starszą wersją bazy danych umożliwi jej działanie.");
            sb.AppendLine("Starsze wersje baz znajdują się w katalogu Archiwum.");
            MessageBox.Show(sb.ToString(), "Błąd", System.Windows.MessageBoxButton.OK);
            System.Windows.Application.Current.Shutdown();
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
