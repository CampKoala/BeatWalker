using System;
using System.Linq;

namespace BeatWalker.Utils
{
    public class SongTimingConfig
    {
        private const string StartTimeHeader = "StartTime";
        private const string DurationHeader = "Duration";
        private const string TypeHeader = "Type";
        private readonly Timing[] _timings;

        public Timing this[int i] => _timings[i];
        public int Count => _timings.Length;

        public SongTimingConfig(string file)
        {
            _timings = CsvReader.Read(file)
                .Select(l => new Timing((float)l[StartTimeHeader], (float)l[DurationHeader], (string)l[TypeHeader]))
                .OrderBy(t => t.StartTime)
                .ToArray();
        }

        public class Timing
        {
            public Timing(float startTime, float duration, string type)
            {
                if (duration < 0) throw new ArgumentOutOfRangeException(nameof(duration));
                if (startTime < 0) throw new ArgumentOutOfRangeException(nameof(startTime));

                StartTime = startTime;
                Duration = duration;
                Type = type.ToLower() switch
                {
                    "lefttap" => TimingType.LeftTap,
                    "righttap" => TimingType.RightTap, 
                    "hold" => TimingType.Hold,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            public float StartTime { get; }
            public float Duration { get; }
            public TimingType Type { get; }
        }

        public enum TimingType
        {
            LeftTap = -1,
            RightTap = 1,
            Hold = 0,
        }
    }
}