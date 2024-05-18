using System;
using System.Linq;

namespace BeatWalker
{
    public class SongConfig
    {
        private const string StartTimeHeader = "StartTime";
        private const string DurationHeader = "Duration";
        private readonly Timing[] _timings;

        public Timing this[int i] => _timings[i];
        public int Count => _timings.Length;

        public SongConfig(string file)
        {
            _timings = CSVReader.Read(file)
                .Select(l => new Timing((float)l[StartTimeHeader], (float)l[DurationHeader]))
                .OrderBy(t => t.StartTime)
                .ToArray();
        }

        public class Timing
        {
            public Timing(float startTime, float duration)
            {
                if (duration < 0) throw new ArgumentOutOfRangeException(nameof(duration));
                if (startTime < 0) throw new ArgumentOutOfRangeException(nameof(startTime));

                StartTime = startTime;
                Duration = duration;
            }

            public float StartTime { get; }
            public float Duration { get; }
        }
    }
}