using CommandLine;
using OpenUtau.Core;
using OpenUtau.Classic;

namespace OpenUtau.Cli.Commands {
    [Verb("check", HelpText = "Run voicebank error checker for a voicebank")]
    public class CheckCommand : BaseCommand {
        [Value(0, MetaName = "voicebankName", Required = true, HelpText = "The relative path to the voicebank")]
        public string voicebankName { get; set; }

        [Value(1, MetaName = "output", Required = false, HelpText = "The output error report file")]
        public string? outputFile { get; set; }

        public override bool Execute() {
            var basePath = PathManager.Inst.SingersPath;
            var vbPath = Path.Join(basePath, voicebankName);
            var checker = new VoicebankErrorChecker(vbPath, basePath);
            checker.Check();
            if(outputFile == null) {
                outputFile = Path.Join(basePath, voicebankName, "errors.txt");
            }
            using (var stream = File.Open(outputFile, FileMode.Create)) {
                using (var writer = new StreamWriter(stream)) {
                    writer.WriteLine($"------ Informations ------");
                    writer.WriteLine();
                    for (var i = 0; i < checker.Infos.Count; i++) {
                        writer.WriteLine($"--- Info {i + 1} ---");
                        writer.WriteLine(checker.Infos[i].ToString());
                    }
                    writer.WriteLine();
                    writer.WriteLine($"------ Errors ------");
                    writer.WriteLine($"Total errors: {checker.Errors.Count}");
                    writer.WriteLine();
                    for (var i = 0; i < checker.Errors.Count; i++) {
                        writer.WriteLine($"--- Error {i + 1} ---");
                        writer.WriteLine(checker.Errors[i].ToString());
                    }
                }
            }
            Console.WriteLine($"Voicebank error report saved to {outputFile}");
            //TODO
            return true;
        }
    }
}