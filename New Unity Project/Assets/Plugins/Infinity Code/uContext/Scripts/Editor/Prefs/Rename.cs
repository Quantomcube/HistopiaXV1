/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        private class RenameManager : PrefManager, IHasShortcutPref
        {
            public override float order
            {
                get { return -45; }
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                return new[]
                {
                    new Shortcut("Rename Selected Items", "Everywhere", "F2"),
                };
            }
        }
    }
}