using OpenUtau.Api;
using OpenUtau.Core.G2p;

namespace OpenUtau.Core.Enunu
{
    [Phonemizer("Enunu Spanish Phonemizer", "ENUNU ES", "O3", language:"ES")]
    public class EnunuSpanishPhonemizer: EnunuG2pPhonemizer
    {
        protected override string GetDictionaryName()=>"enudict-es.yaml";
        protected override IG2p LoadBaseG2p() => new SpanishG2p();
        protected override string[] GetBaseG2pVowels() => new string[] {
            "a", "e", "i", "o", "u"
        };

        protected override string[] GetBaseG2pConsonants() => new string[] {
            "b", "B", "ch", "d", "D", "f", "g", "G", "gn", "I", "k", "l", "ll", 
            "m", "n", "p", "r", "rr", "s", "t", "U", "w", "x", "y", "Y", "z"
        };
    }
}