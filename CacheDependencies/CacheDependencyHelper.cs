using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.OA;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web;

namespace SitefinityWebApp.CacheDependencies
{
    public class CacheDependencyHelper
    {
        internal static Dictionary<Guid, CacheDependencyHelper> CacheDependencyHelpers
        {
            get
            {
                if (cacheDependencyHelpers == null)
                {
                    cacheDependencyHelpers = new Dictionary<Guid, CacheDependencyHelper>();
                }
                return cacheDependencyHelpers;
            }
            set { cacheDependencyHelpers = value; }
        }

        public static CacheDependencyHelper GetCacheDependencyHelper(DataProviderBase provider)
        {
            if (CacheDependencyHelpers.ContainsKey(provider.Id))
            {
                return CacheDependencyHelpers[provider.Id];
            }
            else
            {
                CacheDependencyHelpers.Add(provider.Id, new CacheDependencyHelper());
                return GetCacheDependencyHelper(provider);
            }
        }

        public void GetInvalidatedDirtyItems(DataProviderBase dataProvider)
        {
            this.UpdateInternalDirtyItemsCache(dataProvider, this.DirtyItems);
        }

        public void NotifyInvalidatedItems(DataProviderBase dataProvider)
        {
            if (!dataProvider.SuppressNotifications)
            {
                this.OAContext = ((IOpenAccessDataProvider)dataProvider).GetContext();
                if (this.InvalidatedItems.Count > 0)
                {
                    CacheDependency.Notify(this.InvalidatedItems);
                    this.InvalidatedItems.Clear();
                }
            }
        }

        private void UpdateInternalDirtyItemsCache(DataProviderBase dataProvider, IList dirtyItems)
        {
            if (!dataProvider.SuppressNotifications && Bootstrapper.IsDataInitialized)
            {
                this.OAContext = ((IOpenAccessDataProvider)dataProvider).GetContext();
                foreach (var item in dirtyItems)
                {
                    this.InvalidatedItems.AddRange(CacheDependencyHelper.GetKeysOfDependentObjects(item));
                }
            }
        }

        internal static IList<CacheDependencyKey> GetKeysOfDependentObjects(object item)
        {
            Guid itemId = GetId(item);
            var keys = new List<CacheDependencyKey>();
            keys.Add(new CacheDependencyKey() { Type = item.GetType(), Key = itemId.ToString() });

            return keys;
        }

        internal static Guid GetId(object item)
        {
            var id = Guid.Empty;

            var idProperty = TypeDescriptor.GetProperties(item)["Id"];
            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                id = (Guid)idProperty.GetValue(item);
            }

            return id;
        }

        internal List<CacheDependencyKey> InvalidatedItems
        {
            get
            {
                if (SystemManager.CurrentHttpContext == null) return new List<CacheDependencyKey>();

                var key = string.Concat(this.OAContext.GetHashCode(), "_InvalidatedObjects");

                var items = SystemManager.CurrentHttpContext.Items[key] as List<CacheDependencyKey>;
                if (items == null)
                {
                    items = new List<CacheDependencyKey>();
                    SystemManager.CurrentHttpContext.Items.Add(key, items);
                }
                return items;
            }
        }

        public static void SubscribePageCacheDependency(string id, Type type)
        {
            var objects = GetCacheDependencyObjects(id, type);

            if (!SystemManager.CurrentHttpContext.Items.Contains(PageCacheDependencyKeys.PageData))
            {
                SystemManager.CurrentHttpContext.Items.Add(PageCacheDependencyKeys.PageData, new List<CacheDependencyKey>());
            }
            ((List<CacheDependencyKey>)SystemManager.CurrentHttpContext.Items[PageCacheDependencyKeys.PageData]).AddRange(objects);
        }

        public static IList<CacheDependencyKey> GetCacheDependencyObjects(string id, Type type)
        {
            var cacheDependencyNotifiedObjects = new List<CacheDependencyKey>();

            AddCachedItem(cacheDependencyNotifiedObjects, id, type);

            return cacheDependencyNotifiedObjects;
        }

        public static void AddCachedItem(List<CacheDependencyKey> cacheDependencyNotifiedObjects, string key, Type type)
        {
            if (!cacheDependencyNotifiedObjects.Any(itm => itm.Key == key && itm.Type == type))
            {
                cacheDependencyNotifiedObjects.Add(new CacheDependencyKey() { Key = key, Type = type });
            }
        }

        public System.Collections.IList DirtyItems { get; set; }
        internal SitefinityOAContext OAContext { get; set; }

        private static Dictionary<Guid, CacheDependencyHelper> cacheDependencyHelpers;
    }
}