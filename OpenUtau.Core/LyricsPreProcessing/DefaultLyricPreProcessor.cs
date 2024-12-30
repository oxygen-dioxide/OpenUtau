namespace OpenUtau.Core.LyricPreProcessing {
    public class DefaultLyricPreProcessor: LyricPreProcessor{

        public override string Name => "Default";

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
                        lyrics.Add(builder.ToString());
                        builder.Clear();
                    }
                } else if (contracted.IsMatch(ele)) {
                    builder.Append(ele);
                    lyrics.Add(builder.ToString());
                    builder.Clear();
                } else if (standalone.IsMatch(ele)) {
                    if (builder.Length > 0) {
                        lyrics.Add(builder.ToString());
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
                lyrics.Add(builder.ToString());
                builder.Clear();
            }
            return lyrics;
        }
    }
}