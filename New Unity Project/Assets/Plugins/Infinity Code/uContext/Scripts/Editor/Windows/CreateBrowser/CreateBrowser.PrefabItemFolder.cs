/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        internal class PrefabItemFolder: FolderItem
        {
            public PrefabItemFolder(string[] parts, int index, string path)
            {
                children = new List<Item>();
                label = parts[index];

                if (parts.Length == index + 2)
                {
                    string part = parts[index + 1];
                    if (part.Length < 8) return;

                    PrefabItem child = new PrefabItem(part, path);
                    child.parent = this;
                    children.Add(child);
                }
                else
                {
                    PrefabItemFolder child = new PrefabItemFolder(parts, index + 1, path);
                    child.parent = this;
                    children.Add(child);
                }
            }

            public void Add(string[] parts, int index, string path)
            {
                string next = parts[index + 1];
                PrefabItemFolder folder = children.FirstOrDefault(c => c.label == next) as PrefabItemFolder;
                if (folder != null) folder.Add(parts, index + 1, path);
                else
                {
                    if (parts.Length == index + 2)
                    {
                        if (next.Length < 8) return;

                        PrefabItem child = new PrefabItem(next, path);
                        child.parent = this;
                        children.Add(child);
                    }
                    else
                    {
                        PrefabItemFolder child = new PrefabItemFolder(parts, index + 1, path);
                        child.parent = this;
                        children.Add(child);
                    }
                }
            }

            protected override void InitContent()
            {
                if (folderIconContent == null) folderIconContent = EditorIconContents.folder;
                _content = new GUIContent(folderIconContent);
                _content.text = label;
            }
        }
    }
}