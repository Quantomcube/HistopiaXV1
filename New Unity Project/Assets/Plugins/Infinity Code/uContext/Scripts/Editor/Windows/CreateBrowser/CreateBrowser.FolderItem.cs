/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        internal abstract class FolderItem : Item
        {
            protected static GUIContent folderIconContent;
            public List<Item> children;

            public override void Dispose()
            {
                base.Dispose();

                if (children != null)
                {
                    foreach (Item child in children) child.Dispose();
                    children = null;
                }
            }

            public override void Filter(string pattern, List<Item> filteredItems)
            {
                foreach (Item child in children) child.Filter(pattern, filteredItems);
            }

            public override void OnClick()
            {
                selectedFolder = this;
                instance.selectedIndex = 0;
                selectedItem = children[0];
            }
        }
    }
}