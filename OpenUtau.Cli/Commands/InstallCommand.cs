using ShellProgressBar;
using System.Text;
using CommandLine;
using OpenUtau.Core;
using OpenUtau.Cli.Util;

namespace OpenUtau.Cli.Commands {
    [Verb("install", HelpText = "Install a voicebank")]
    public class InstallCommand : BaseCommand {
        [Value(0, MetaName = "archivePath", Required = true, HelpText = "Path to the voicebank archive file")]
        public string archiveFilePath { get; set; }

        [Option('a', "archiveEncoding", Required = false, HelpText = "Encoding of the name of the files in the archive")]
        public string archiveEncodingStr { get; set; }

        [Option('t', "textEncoding", Required = false, HelpText = "Encoding of the text files in the voicebank")]
        public string textEncodingStr { get; set; }

        Encoding getEncoding(string encodingStr) {
            if (encodingStr == null) {
                return Encoding.UTF8;
            }
            return Encoding.GetEncoding(encodingStr);
        }

        public override bool Execute() {
            if (archiveFilePath.EndsWith(Core.Vogen.VogenSingerInstaller.FileExt)) {
                Console.WriteLine("Installing vogen singer from " + archiveFilePath + "...");
                Core.Vogen.VogenSingerInstaller.Install(archiveFilePath);
                return true;
            }
            var archiveEncoding = getEncoding(archiveEncodingStr);
            var textEncoding = getEncoding(textEncodingStr);
            Console.WriteLine("Installing Classic singer package from " + archiveFilePath);
            Console.WriteLine("Using Archive encoding " + archiveEncoding.BodyName);
            Console.WriteLine("Using Text encoding " + textEncoding.BodyName);
            var basePath = PathManager.Inst.SingersInstallPath;
            var progressBar = new UShellProgressBar();
            var installer = new Classic.VoicebankInstaller(basePath, progressBar.Notify, 
                getEncoding(archiveEncodingStr), getEncoding(textEncodingStr));
            //TODO
            //installer.Install(archiveFilePath);
            return true;
        }
    }
}
