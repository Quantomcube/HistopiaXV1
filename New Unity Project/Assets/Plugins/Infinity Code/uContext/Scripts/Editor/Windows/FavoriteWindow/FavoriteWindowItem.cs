/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.uContext.JSON;
using UnityEditor;

namespace InfinityCode.uContext.Windows
{
    [Serializable]
    public class FavoriteWindowItem
    {
        public string title;
        public string className;

        public FavoriteWindowItem(EditorWindow window)
        {
            Type type = window.GetType();
            className = type.AssemblyQualifiedName;
            title = window.titleContent.text;
        }

        public FavoriteWindowItem(JsonItem item)
        {
            className = item.V<string>("class");
            title = item.V<string>("title");
        }

        public void Open()
        {
            Type type = Type.GetType(className);
            if (type == null) EditorUtility.DisplayDialog("Error", "Can't find the window class. Please delete the entry and add again.", "OK");
            else EditorWindow.GetWindow(type, false, title);
        }
    }
}