using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using OpenUtau.Api;
using OpenUtau.Core.G2p;

namespace OpenUtau.Core.Enunu
{
    [Phonemizer("Enunu English Phonemizer", "ENUNU EN", "O3", language:"EN")]
    public class EnunuEnglishPhonemizer: EnunuG2pPhonemizer
    {
        readonly string PhonemizerType = "ENUNU EN";
        protected override string GetDictionaryName()=>"enudict-en.yaml";
        protected override IG2p LoadBaseG2p() => new ArpabetG2p();
        protected override string[] GetBaseG2pVowels() => new string[] {
            "aa", "ae", "ah", "ao", "aw", "ay", "eh", "er", 
            "ey", "ih", "iy", "ow", "oy", "uh", "uw"
        };

        protected override string[] GetBaseG2pConsonants() => new string[] {
            "b", "ch", "d", "dh", "f", "g", "hh", "jh", "k", "l", "m", "n", 
            "ng", "p", "r", "s", "sh", "t", "th", "v", "w", "y", "z", "zh"
        };
        
        protected override IG2p LoadG2p() {
            var g2ps = new List<IG2p>();
            // Load dictionary from plugin folder.
            string path = Path.Combine(PluginDir, "arpasing.yaml");
            if (File.Exists(path)) {
                try {
                    g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(path)).Build());
                } catch (Exception e) {
                    Log.Error(e, $"Failed to load {path}");
                }
            }

            // Load dictionary from singer folder.
            if (singer != null && singer.Found && singer.Loaded) {
                string file = Path.Combine(singer.Location, "arpasing.yaml");
                if (File.Exists(file)) {
                    try {
                        g2ps.Add(G2pDictionary.NewBuilder().Load(File.ReadAllText(file)).Build());
                    } catch (Exception e) {
                        Log.Error(e, $"Failed to load {file}");
                    }
                }
            }

            // Load base g2p.
            g2ps.Add(new ArpabetG2p());
            return new G2pFallbacks(g2ps.ToArray());
        }
    }
}