using UnityEngine;

namespace LD60.Levels
{
    public static class LevelDurationUtils
    {
        public static int DurationToInt(float duration) => Mathf.RoundToInt(duration * 100);
        public static float IntDurationToFloat(int duration) => duration * .01f;

        public static string FormatLevelDuration(float duration)
        {
            if (duration > 60)
            {
                return $"{(int)duration / 60:00}:{duration % 60:00.00}";
            }

            return $"{duration:00.00} seconds";
        }
    }
}