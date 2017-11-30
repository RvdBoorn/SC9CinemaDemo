using Sitecore.Data.Items;
using System.Collections.Generic;

namespace SC9Demo.Configuration.SiteUI.Search.Helper
{
    public class ItemIDComparer : IEqualityComparer<Item>
    {
        public bool Equals(Item x, Item y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(Item obj)
        {
            return obj.ID.GetHashCode();
        }
    }
}