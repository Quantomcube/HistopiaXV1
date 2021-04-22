/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    [InitializeOnLoad]
    public static class HierarchyItemDrawer
    {
        private static List<Item> items;

        static HierarchyItemDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int id, Rect rect)
        {
            if (items == null) return;

            foreach (Item item in items)
            {
                if (item.action != null)
                {
                    try
                    {
                        rect = item.action(id, rect);
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }
                }
            }
        }

        public static void Register(string id, Func<int, Rect, Rect> action, float order)
        {
            if (string.IsNullOrEmpty(id)) throw new Exception("ID cannot be empty");

            if (items == null) items = new List<Item>();
            int hash = id.GetHashCode();
            foreach (Item item in items)
            {
                if (item.hash == hash && item.id == id)
                {
                    item.action = action;
                    item.order = order;
                    return;
                }
            }
            items.Add(new Item
            {
                id = id,
                hash = hash,
                action = action,
                order = order
            });

            items = items.OrderBy(i => i.order).ToList();
            items.Sort(delegate(Item i1, Item i2)
            {
                if (i1.order == i2.order) return 0;
                if (i1.order > i2.order) return 1;
                return -1;
            });
        }

        private class Item
        {
            public int hash;
            public string id;
            public Func<int, Rect, Rect> action;
            public float order;
        }
    }
}