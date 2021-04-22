/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Actions;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    public class Replace : ActionItem, IValidatableLayoutItem
    {
        public override float order
        {
            get { return -905; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.replace, "Replace (Available in uContext PRO)");
        }

        public override void Invoke()
        {
            Links.OpenPro();
        }

        public bool Validate()
        {
            return Prefs.proHighlight;
        }
    }
#endif
}