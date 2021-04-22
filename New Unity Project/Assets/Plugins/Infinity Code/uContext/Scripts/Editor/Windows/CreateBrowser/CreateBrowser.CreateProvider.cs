/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        public class CreateProvider : Provider
        {
            public string[] skipMenuItems = {
                "GameObject/Create Empty Child".ToLower(),
                "GameObject/Group".ToLower(),
                "GameObject/Ungroup".ToLower()
            };

            public override float order
            {
                get { return 0; }
            }

            public override string title
            {
                get { return instance.createLabel; }
            }

            public override void Cache()
            {
                items = new List<Item>();

                foreach (string submenu in Unsupported.GetSubmenus("GameObject"))
                {
                    string l = submenu.ToLower();
                    if (l == "GameObject/Center On Children".ToLower()) break;
                    if (skipMenuItems.Contains(l)) continue;

                    string menuItemStr = submenu.Substring(11);
                    if (menuItemStr == "Create Empty") menuItemStr = "Empty GameObject";
                    string[] parts = menuItemStr.Split('/');
                    if (parts.Length == 1) items.Add(new CreateItem(menuItemStr, submenu));
                    else
                    {
                        CreateItemFolder root = items.FirstOrDefault(i => i.label == parts[0]) as CreateItemFolder;
                        if (root != null) root.Add(parts, 0, submenu);
                        else items.Add(new CreateItemFolder(parts, 0, submenu));
                    }
                }

                items = items.OrderBy(o =>
                {
                    if (o is FolderItem) return 0;
                    return -1;
                }).ThenBy(o => o.label).ToList();
            }

            public override int IndexOf(Item item)
            {
                return items.IndexOf(item);
            }
        }
    }
}