/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class AudioUtils
    {
        private static MethodInfo isClipPlaying;
        private static MethodInfo playClip;
        private static MethodInfo stopAllClips;
        private static MethodInfo stopClip;

        public static bool IsClipPlaying(AudioClip clip)
        {
            if (isClipPlaying == null) isClipPlaying = EditorTypes.audioUtils.GetMethod("IsClipPlaying", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(AudioClip)}, null);
            return (bool)isClipPlaying.Invoke(null, new object[]
            {
                clip
            });
        }

        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            if (playClip == null) playClip = EditorTypes.audioUtils.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(AudioClip), typeof(int), typeof(bool)}, null);
            playClip.Invoke(null, new object[]
            {
                clip, startSample, loop
            });
        }

        public static void StopAllClips()
        {
            if (stopAllClips == null) stopAllClips = EditorTypes.audioUtils.GetMethod("StopAllClips", BindingFlags.Static | BindingFlags.Public);
            stopAllClips.Invoke(null, new object[0]);
        }

        public static void StopClip(AudioClip clip)
        {
            if (stopClip == null) stopClip = EditorTypes.audioUtils.GetMethod("StopClip", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(AudioClip) }, null);
            stopClip.Invoke(null, new object[]
            {
                clip
            });
        }
    }
}