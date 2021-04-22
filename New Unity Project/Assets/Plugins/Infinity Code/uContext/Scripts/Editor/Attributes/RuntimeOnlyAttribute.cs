/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.uContext.Attributes
{
    public class RuntimeOnlyAttribute : ValidateAttribute
    {
        public RuntimeOnlyAttribute()
        {
        }

        public override bool Validate()
        {
            return EditorApplication.isPlaying;
        }
    }
}