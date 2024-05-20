using System;
using System.Linq;
using BeatWalker.Utils;

namespace BeatWalker.Config
{
    public class SongTimingConfig
    {
        public const string TimeHeader = "Time";
        public const string DurationHeader = "Duration";
        public const string TypeHeader = "Type";
        public const string IndexField = "Index";
        public static readonly string Header = $"{TimeHeader},{DurationHeader},{TypeHeader}";
        
        private readonly SongTiming[] _timings;

        public SongTiming this[int i] => _timings[i];
        public int Count => _timings.Length;

        public SongTimingConfig(string song)
        {
            _timings = CsvReader.Read($"Songs/{song}")
                .Select(l => new SongTiming(Convert.ToInt32(l[IndexField]), Convert.ToSingle(l[TimeHeader]), Convert.ToSingle(l[DurationHeader]), (string)l[TypeHeader]))
                .OrderBy(t => t.Time)
                .ToArray();
        }
    }
}