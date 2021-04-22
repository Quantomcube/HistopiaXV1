/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        public class PrefabProvider: Provider
        {
            public override float order
            {
                get { return 1; }
            }

            public override string title
            {
                get { return instance.prefabsLabel; }
            }

            public override void Cache()
            {
                items = new List<Item>();

                string[] blacklist = Prefs.createBrowserBlacklist.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                bool hasBlacklist = blacklist.Length > 0;

                string[] assets = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
                foreach (string guid in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (hasBlacklist)
                    {
                        if (blacklist.Any(b => assetPath.StartsWith(b))) continue;
                    }
                    string shortPath = assetPath.Substring(7);
                    string[] parts = shortPath.Split('/');
                    if (parts.Length == 1)
                    {
                        if (shortPath.Length < 8) continue;
                        items.Add(new PrefabItem(shortPath, assetPath));
                    }
                    else
                    {
                        PrefabItemFolder root = items.FirstOrDefault(i => i.label == parts[0]) as PrefabItemFolder;
                        if (root != null)
                        {
                            root.Add(parts, 0, assetPath);
                        }
                        else
                        {
                            items.Add(new PrefabItemFolder(parts, 0, assetPath));
                        }
                    }
                }

                assets = AssetDatabase.FindAssets("t:model", new[] { "Assets" });
                foreach (string guid in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (hasBlacklist)
                    {
                        if (blacklist.Any(b => assetPath.StartsWith(b))) continue;
                    }
                    string shortPath = assetPath.Substring(7);
                    string[] parts = shortPath.Split('/');
                    if (parts.Length == 1)
                    {
                        if (shortPath.Length < 8) continue;
                        items.Add(new PrefabItem(shortPath, assetPath));
                    }
                    else
                    {
                        PrefabItemFolder root = items.FirstOrDefault(i => i.label == parts[0]) as PrefabItemFolder;
                        if (root != null)
                        {
                            root.Add(parts, 0, assetPath);
                        }
                        else
                        {
                            items.Add(new PrefabItemFolder(parts, 0, assetPath));
                        }
                    }
                }

                items = items.OrderBy(o =>
                {
                    if (o is FolderItem) return 0;
                    return -1;
                }).ThenBy(o => o.label).ToList();
            }
        }
    }
}