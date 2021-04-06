/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Attributes;
using UnityEngine;

namespace InfinityCode.uContext.Actions
{
    [RequireMultipleGameObjects]
    public class Group : ActionItem
    {
        public override float order
        {
            get { return -895; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.group, "Group");
        }

        public override void Invoke()
        {
            Tools.Group.GroupSelection();
        }
    }
}