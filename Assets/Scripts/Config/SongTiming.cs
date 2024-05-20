using System;

namespace BeatWalker.Config
{
    public class SongTiming
    {
        internal SongTiming(int index, float time, float duration, string type) : this(index, time, duration, type.ToLower() switch
        {
            "lefttap" => SongTimingType.LeftTap,
            "righttap" => SongTimingType.RightTap,
            "hold" => SongTimingType.Hold,
            _ => throw new ArgumentOutOfRangeException()
        })
        {
        }

        private SongTiming(int index, float time, float duration, SongTimingType type)
        {
            if (duration < 0) throw new ArgumentOutOfRangeException(nameof(duration));
            if (time < 0) throw new ArgumentOutOfRangeException(nameof(time));

            Time = time;
            Duration = duration;
            Type = type;
            Index = index;
        }

        public float Time { get; }
        public float Duration { get; }
        public SongTimingType Type { get; }
        public int Index { get; }
    }
}