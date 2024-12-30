using System.Collections.Generic;
using OpenUtau.Api;
using System.Linq;
using System.Text;
using System.Globalization;

namespace OpenUtau.Core.LyricPreProcessing {
    public abstract class G2pLyricPreProcessor: LyricPreProcessor{
        
        
        public virtual IG2p G2p { get; }

        public static List<string> HandleWord(string word){
            var phonemes = G2p.Query(word);
            var syllableCount = phonemes.Count(phoneme => G2p.IsVowel(phoneme));
            if(syllableCount <= 1){
                return new List<string>{word};
            }
            var result = Enumerable.Repeat("+", syllableCount).ToList();
            result[0]=word;
            return result;
        }

        public static List<string> Split(string? text) {
            if(text == null){
                return new List<string>();
            }
            var lyrics = new List<string>();
            var builder = new StringBuilder();
            var etor = StringInfo.GetTextElementEnumerator(text);
            while (etor.MoveNext()) {
                string ele = etor.GetTextElement();
                if (whitespace.IsMatch(ele)) {
                    if (builder.Length > 0) {
                        lyrics.AddRange(HandleWord(builder.ToString()));
                        builder.Clear();
                    }
                } else if (contracted.IsMatch(ele)) {
                    builder.Append(ele);
                    lyrics.AddRange(HandleWord(builder.ToString()));
                    builder.Clear();
                } else if (standalone.IsMatch(ele)) {
                    if (builder.Length > 0) {
                        lyrics.AddRange(HandleWord(builder.ToString()));
                        builder.Clear();
                    }
                    builder.Append(ele);
                } else if (ele == "\"") {
                    while (etor.MoveNext()) {
                        string ele1 = etor.GetTextElement();
                        if (ele1 == "\"") {
                            lyrics.Add(builder.ToString());
                            builder.Clear();
                            break;
                        } else {
                            builder.Append(ele1);
                        }
                    }
                } else {
                    if (builder.Length > 0 && standalone.IsMatch(builder.ToString())) {
                        lyrics.Add(builder.ToString());
                        builder.Clear();
                    }
                    builder.Append(ele);
                }
            }
            if (builder.Length > 0) {
                lyrics.AddRange(HandleWord(builder.ToString()));
                builder.Clear();
            }
            return lyrics;
        }
    }
}