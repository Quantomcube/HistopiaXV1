/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.uContext.Windows
{
    public partial class CreateBrowser
    {
        internal class CreateItem : Item
        {
            public string submenu;
            private static GUIContent plusIconContent;

            public CreateItem(string label, string submenu)
            {
                this.label = label;
                this.submenu = submenu;
                
            }

            protected override void InitContent()
            {
                if (plusIconContent == null) plusIconContent = EditorIconContents.toolbarPlus;
                _content = new GUIContent(plusIconContent);
                _content.text = label;
            }

            public override void OnClick()
            {
                if (instance.OnSelectCreate != null) instance.OnSelectCreate(submenu);
                instance.Close();
            }
        }
    }
}