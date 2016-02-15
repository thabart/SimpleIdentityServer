using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Practices.Unity;

using SimpleIdentityServer.Common;

namespace SimpleIdentityServer.Api
{
    public static class ModuleLoader
    {
        public static void LoadContainer(IUnityContainer container)
        {
            var applicationCatalog = new ApplicationCatalog();
            var importDef = BuildImportDefinition();
            try
            {
                using (var aggregateCatalog = new AggregateCatalog())
                {
                    aggregateCatalog.Catalogs.Add(applicationCatalog);
                    using (var compositionContainer = new CompositionContainer(aggregateCatalog))
                    {
                        var t = compositionContainer.GetExports<IModule>();
                        var exports = compositionContainer.GetExports(importDef);
                        var modules =
                            exports.Select(export => export.Value as IModule).Where(m => m != null);
                        var registrar = new ModuleRegister(container);
                        foreach (var module in modules)
                        {
                            module.Initialize(registrar);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                var builder = new StringBuilder();
                foreach (Exception loaderException in typeLoadException.LoaderExceptions)
                {
                    builder.AppendFormat("{0}\n", loaderException.Message);
                }
                throw new TypeLoadException(builder.ToString(), typeLoadException);
            }
        }

        private static ImportDefinition BuildImportDefinition()
        {
            return new ImportDefinition(
                def => true, typeof (IModule).FullName, ImportCardinality.ZeroOrMore, false, false);
        }
    }
}