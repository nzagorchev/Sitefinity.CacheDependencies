using System.Web.UI;
using Telerik.Sitefinity.Web.UI.PublicControls;

namespace SitefinityWebApp.CacheDependencies.Taxonomies
{
    public class TaxonomyControlCustom : TaxonomyControl
    {
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.Visible)
            {
                this.SubscribeCacheDependency();
            }
        }

        protected virtual void SubscribeCacheDependency()
        {
            if (!this.IsBackend())
            {
                CacheDependencyHelper.SubscribePageCacheDependency(this.TaxonomyId.ToString(), this.Taxonomy.GetType());
            }
        }
    }
}