/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.uContext.Tools
{
    public static partial class WindowHistory
    {
        public abstract class Provider
        {
            public abstract float order { get; }

            public abstract void GenerateMenu(GenericMenu menu, ref bool hasItems);
        }
    }
}