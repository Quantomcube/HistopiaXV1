/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.uContext.Actions;
using UnityEngine;

namespace InfinityCode.uContext.ProHighlight
{
#if !UCONTEXT_PRO
    public class History : ActionItem, IValidatableLayoutItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return 800; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.history, "History (Available in uContext PRO)");
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