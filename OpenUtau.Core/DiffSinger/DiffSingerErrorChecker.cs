using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenUtau.Api;
using OpenUtau.Classic;

namespace OpenUtau.Core.DiffSinger{
    public class DiffSingerVoicebankError {
        public string? message;
        public Exception? e;

        public override string ToString() {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(message)) {
                builder.AppendLine(message);
            }
            if (e != null) {
                builder.AppendLine(e.ToString());
            }
            return builder.ToString();
        }


    }

    public class DiffSingerErrorChecker {
        public List<DiffSingerVoicebankError> Errors = new List<DiffSingerVoicebankError>();
        public List<DiffSingerVoicebankError> Infos = new List<DiffSingerVoicebankError>();

        readonly string path;
        readonly DiffSingerSinger singer;
        public DiffSingerErrorChecker(string path, string basePath) {
            this.path = path;
            string charTxt = Path.Combine(path, VoicebankLoader.kCharTxt);
            var voicebank = new Voicebank() {
                File = charTxt,
                BasePath = basePath,
            };
            singer = new DiffSingerSinger(voicebank);
        }

        public DiffSingerErrorChecker(DiffSingerSinger singer){
            this.singer = singer;
            this.path = singer.Location;
        }

        public void Check(){
            
        }


        public void AsianDictionaryEntriesCheck(string filename, G2pDictionaryData data, string[] requiredEntries){
            /*try{
                var data = Yaml.DefaultDeserializer.Deserialize<G2pDictionaryData>(File.ReadAllText(filename));
            } catch (Exception e) {
                Errors.Add(new DiffSingerVoicebankError() {
                    message = $"Failed to load dictionary {filename}",
                    e = e,
                });
            }*/
            HashSet<string> entries = data.entries.Select(e => e.grapheme).ToHashSet();
            var missingEntries = requiredEntries.Where(e=>!entries.Contains(e)).ToList();
            if(missingEntries.Any()){
                Errors.Add(new DiffSingerVoicebankError{
                    message = $"Missing entries in dictionary {filename}: {String.Join(", ", missingEntries)}"
                });
            }
        }

        public static string[] ChinesePinyins = {
            "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao", "bei",
            "ben", "beng", "bi", "bian", "biao", "bie", "bin", "bing", "bo", "bu", 
            "ca", "cai", "can", "cang", "cao", "ce", "cei", "cen", "ceng", "cha", 
            "chai", "chan", "chang", "chao", "che", "chen", "cheng", "chi", "chong", 
            "chou", "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", "chuo", 
            "ci", "cong", "cou", "cu", "cuan", "cui", "cun", "cuo", "da", "dai", "dan", 
            "dang", "dao", "de", "dei", "den", "deng", "di", "dia", "dian", "diao", 
            "die", "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", 
            "e", "ei", "en", "eng", "er", "fa", "fan", "fang", "fei", "fen", "feng", 
            "fo", "fou", "fu", "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", 
            "geng", "gong", "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", 
            "guo", "ha", "hai", "han", "hang", "hao", "he", "hei", "hen", "heng", 
            "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo", 
            "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", 
            "jiu", "ju", "jv", "juan", "jvan", "jue", "jve", "jun", "jvn", "ka", "kai", 
            "kan", "kang", "kao", "ke", "kei", "ken", "keng", "kong", "kou", "ku", 
            "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan", 
            "lang", "lao", "le", "lei", "leng", "li", "lia", "lian", "liang", "liao", 
            "lie", "lin", "ling", "liu", "lo", "long", "lou", "lu", "luan", "lun", 
            "luo", "lv", "lve", "ma", "mai", "man", "mang", "mao", "me", "mei", "men", 
            "meng", "mi", "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", 
            "mu", "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ni", 
            "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nou", "nu", 
            "nuan", "nun", "nuo", "nv", "nve", "o", "ou", "pa", "pai", "pan", "pang", 
            "pao", "pei", "pen", "peng", "pi", "pian", "piao", "pie", "pin", "ping", 
            "po", "pou", "pu", "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", 
            "qing", "qiong", "qiu", "qu", "qv", "quan", "qvan", "que", "qve", "qun", 
            "qvn", "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", 
            "ru", "rua", "ruan", "rui", "run", "ruo", "sa", "sai", "san", "sang", 
            "sao", "se", "sen", "seng", "sha", "shai", "shan", "shang", "shao", "she", 
            "shei", "shen", "sheng", "shi", "shou", "shu", "shua", "shuai", "shuan", 
            "shuang", "shui", "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", 
            "sun", "suo", "ta", "tai", "tan", "tang", "tao", "te", "tei", "teng", "ti", 
            "tian", "tiao", "tie", "ting", "tong", "tou", "tu", "tuan", "tui", "tun", 
            "tuo", "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu", "xi", 
            "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu", "xu", 
            "xv", "xuan", "xvan", "xue", "xve", "xun", "xvn", "ya", "yan", "yang", 
            "yao", "ye", "yi", "yin", "ying", "yo", "yong", "you", "yu", "yv", "yuan", 
            "yvan", "yue", "yve", "yun", "yvn", "za", "zai", "zan", "zang", "zao", 
            "ze", "zei", "zen", "zeng", "zha", "zhai", "zhan", "zhang", "zhao", "zhe", 
            "zhei", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", 
            "zhuan", "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", 
            "zuan", "zui", "zun", "zuo"};
    }
}