using System;
using System.IO;
using OpenUtau.Api;

//This phonemizer currently use an external dictionary file, which is not included in the repository.
//Please download it from https://github.com/mmemim/OpenUTAU-French-Dictionary 
//and put it into the "Dictionaries" folder
//The french G2p will need an update before migrating to it.

namespace OpenUtau.Core.Enunu
{
    [Phonemizer("Enunu French Phonemizer", "ENUNU FR", "O3", language:"FR")]
    public class EnunuFrenchPhonemizer : EnunuG2pPhonemizer
    {
        readonly string PhonemizerType = "ENUNU FR";
        protected override string GetDictionaryName() => "enudict-fr.yaml";

        protected override string[] GetBaseG2pVowels() => new string[] {
            "aa", "ai", "an", "au", "ee", "ei", "eu", "ii",
            "in", "oe", "on", "oo", "ou", "un", "uu", "uy"
        };

        protected override string[] GetBaseG2pConsonants() => new string[] {
            "bb", "ch", "dd", "ff", "gg", "gn", "jj", "kk",
            "ll", "mm", "nn", "pp", "rr", "ss", "tt", "vv", "ww", "yy", "zz"
        };

        private readonly string[] wordSeparator = new[] { "  " };

        /// <summary>
        /// Used to extract phonemes from CMU Dict word. Override if you need some extra logic
        /// </summary>
        /// <param name="phonemesString"></param>
        /// <returns></returns>
        private string[] GetDictionaryWordPhonemes(string phonemesString) {
            return phonemesString.Split(' ');
        }

        /// <summary>
        /// Parses CMU dictionary, when phonemes are separated by spaces, and word vs phonemes are separated with two spaces,
        /// and replaces phonemes with replacement table
        /// Is Running Async!
        /// </summary>
        /// <param name="dictionaryText"></param>
        /// <param name="builder"></param>
        private void ParseDictionary(string dictionaryText, G2pDictionary.Builder builder) {
            foreach (var line in dictionaryText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)) {
                if (line.StartsWith(";;;")) {
                    continue;
                }
                var parts = line.Trim().Split(wordSeparator, StringSplitOptions.None);
                if (parts.Length != 2) {
                    continue;
                }
                string key = parts[0].ToLowerInvariant();
                var values = GetDictionaryWordPhonemes(parts[1]);
                lock (builder) {
                    builder.AddEntry(key, values);
                };
            };
        }

        protected override IG2p LoadBaseG2p(){
            var dictionaryName = "cmudict_fr.txt";
            var filename = Path.Combine(DictionariesPath, dictionaryName);
            var dictionaryText = File.ReadAllText(filename);
            var builder = G2pDictionary.NewBuilder();
            var vowels = GetBaseG2pVowels();
            foreach (var vowel in vowels) {
                builder.AddSymbol(vowel, true);
            }
            var consonants = GetBaseG2pConsonants();
            foreach (var consonant in consonants) {
                builder.AddSymbol(consonant, false);
            }
            builder.AddEntry("a", new string[] { "a" });
            ParseDictionary(dictionaryText, builder);
            return builder.Build();
        }
    }
}