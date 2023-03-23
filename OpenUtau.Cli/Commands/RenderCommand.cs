using CommandLine;
using OpenUtau.Core;
using OpenUtau.Core.Format;
using OpenUtau.Core.Render;
using OpenUtau.Core.Ustx;
using OpenUtau.Classic;
using NAudio.Wave;

namespace OpenUtau.Cli.Commands {
    [Verb("render", HelpText = "Render a .ustx file to audio")]
    public class RenderCommand : BaseCommand {
        [Value(0, MetaName = "input", Required = true, HelpText = "The .ustx file to render")]
        public string inputFile { get; set; }

        [Value(1, MetaName = "output", Required = true, HelpText = "The output file")]
        public string outputFile { get; set; }

        public override bool Execute() {
            //Initialize Docmanager
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            DocManager.Inst.Initialize();
            DocManager.Inst.PostOnUIThread = action => { };
            //Load project
            var project = Ustx.Load(inputFile);
            if (project == null) return false;
            DocManager.Inst.ExecuteCmd(new LoadProjectNotification(project));
            //Load Voicebanks
            foreach(var track in project.tracks) {
                if (track.singer == null) {
                    continue;
                }
                var singer = track.singer;
                foreach(var basePath in new string[] {
                    PathManager.Inst.SingersPathOld,
                    PathManager.Inst.SingersPath,
                    PathManager.Inst.AdditionalSingersPath,
                }) {
                    var filePath = Path.Join(basePath, singer, VoicebankLoader.kCharTxt);
                    if (File.Exists(filePath)) {
                        var loader = new VoicebankLoader(basePath);
                        var voicebank = new Voicebank();
                        VoicebankLoader.LoadInfo(voicebank, filePath, basePath);
                        track.Singer = voicebank.SingerType == USingerType.Enunu
                            ? new Core.Enunu.EnunuSinger(voicebank) as USinger
                            : new ClassicSinger(voicebank) as USinger;
                        Console.WriteLine("Loaded voicebank " + singer);
                        break;
                    }
                }
            }
            //phonemize

            //render
            PlaybackManager.Inst.RenderMixdownWait(project, outputFile);
            return true;
        }
    }
}
