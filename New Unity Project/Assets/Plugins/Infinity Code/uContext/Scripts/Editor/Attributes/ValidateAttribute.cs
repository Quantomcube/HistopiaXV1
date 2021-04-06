/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.uContext.Attributes
{
    public abstract class ValidateAttribute : Attribute
    {
        public abstract bool Validate();
    }
}