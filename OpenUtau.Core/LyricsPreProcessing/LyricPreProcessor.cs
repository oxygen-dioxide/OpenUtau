using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Core.LyricPreProcessing {
    public abstract class LyricPreProcessor {
        public static Regex whitespace = new Regex(@"\s");
        public static Regex standalone = new Regex(
            @"\p{IsCJKUnifiedIdeographs}|\p{IsHiragana}|\p{IsKatakana}|\p{IsHangulSyllables}");
        public static Regex contracted = new Regex(@"[ゃゅょぁぃぅぇぉャュョァィゥェォ]");
        
        public string Name { get; }
        
        public static List<string> Split(string? text);
    }
}