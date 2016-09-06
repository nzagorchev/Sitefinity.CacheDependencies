using SitefinityWebApp.CacheDependencies;
using System;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Taxonomies;
using Telerik.Sitefinity.Taxonomies.Data;

namespace SitefinityWebApp
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Bootstrapper.Initialized += Bootstrapper_Initialized;
        }

        private void Bootstrapper_Initialized(object sender, Telerik.Sitefinity.Data.ExecutedEventArgs e)
        {
            if (e.CommandName == "Bootstrapped")
            {
                TaxonomyManager.Executed += new EventHandler<Telerik.Sitefinity.Data.ExecutedEventArgs>(this.TaxonomyManager_Executed);
                TaxonomyManager.Executing += new EventHandler<Telerik.Sitefinity.Data.ExecutingEventArgs>(this.TaxonomyManager_Executing);
            }
        }

        private void TaxonomyManager_Executing(object sender, ExecutingEventArgs e)
        {
            if (e.CommandName == "CommitTransaction" || e.CommandName == "FlushTransaction")
            {
                var provider = ((Telerik.Sitefinity.Data.DataProviderBase)(sender));
                CacheDependencyHelper.GetCacheDependencyHelper(provider).DirtyItems = provider.GetDirtyItems();
            }
        }

        private void TaxonomyManager_Executed(object sender, ExecutedEventArgs e)
        {
            if (e.CommandName == "CommitTransaction")
            {
                var provider = sender as OpenAccessTaxonomyProvider;
                var helper = CacheDependencyHelper.GetCacheDependencyHelper(provider);
                helper.GetInvalidatedDirtyItems(provider);

                helper.NotifyInvalidatedItems(provider);
            }
            else if (e.CommandName == "FlushTransaction")
            {
                var provider = sender as OpenAccessTaxonomyProvider;
                var helper = CacheDependencyHelper.GetCacheDependencyHelper(provider);
                helper.GetInvalidatedDirtyItems(provider);
            }
        }
    }
}