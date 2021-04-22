/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class Log
    {
        public static void Add(Exception exception)
        {
            if (Prefs.showExceptionsInConsole) Debug.LogException(exception);
            /*if (Prefs.sendExceptionsToServer)
            {
                // TODO: Implement this
            }*/
        }
    }
}