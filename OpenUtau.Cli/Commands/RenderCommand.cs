using CommandLine;

using OpenUtau.Core;
using OpenUtau.Core.Format;
using OpenUtau.Classic;

namespace OpenUtau.Cli.Commands {
    [Verb("render", HelpText = "Render a .ustx file to audio")]
    public class RenderCommand : BaseCommand {
        [Value(0, MetaName = "input", Required = true, HelpText = "The .ustx file to render")]
        public string inputFile { get; set; }

        [Value(1, MetaName = "output", Required = true, HelpText = "The output file")]
        public string outputFile { get; set; }

        public override bool Execute() {
            //Initialize
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            Console.WriteLine("Loading Singers");
            ToolsManager.Inst.Initialize();
            SingerManager.Inst.Initialize();
            DocManager.Inst.Initialize();

            DocManager.Inst.PostOnUIThread = action => { };
            //Load project
            var project = Ustx.Load(inputFile);
            if (project == null) return false;
            //Load Voicebanks
            /*
            foreach(var track in project.tracks) {
                if (track.singer == null) {
                    continue;
                }
                var singerId = track.singer;
                Voicebank voicebank = null;
                foreach(var basePath in new string[] {
                    PathManager.Inst.SingersPathOld,
                    PathManager.Inst.SingersPath,
                    PathManager.Inst.AdditionalSingersPath,
                }) {
                    var filePath = Path.Join(basePath, singerId, VoicebankLoader.kCharTxt);
                    if (File.Exists(filePath)) {
                        var loader = new VoicebankLoader(basePath);
                        voicebank = new Voicebank();
                        VoicebankLoader.LoadInfo(voicebank, filePath, basePath);
                        var Singer = voicebank.SingerType == USingerType.Enunu
                            ? new Core.Enunu.EnunuSinger(voicebank) as USinger
                            : new ClassicSinger(voicebank) as USinger;
                        Console.WriteLine("Loaded voicebank " + singerId);
                        SingerManager.Inst.Singers[singerId] = Singer;
                        break;
                    }
                }
            }*/
            project = Ustx.Load(inputFile);
            if (project == null) return false;
            DocManager.Inst.ExecuteCmd(new LoadProjectNotification(project));
            //phonemize
            Thread.Sleep(10000);
            //render
            PlaybackManager.Inst.RenderMixdownWait(project, outputFile);
            return true;
        }
    }
}
