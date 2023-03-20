using System.Resources;
using CommandLine;

namespace OpenUtau.Cli.Commands {
    [Verb("render", HelpText = "Render a .ustx file to audio")]
    public class RenderCommand : BaseCommand {
        [Value(0, MetaName = "input", Required = true, HelpText = "The .ustx file to render")]
        public string inputFile { get; set; }

        [Value(1, MetaName = "output", Required = true, HelpText = "The output file")]
        public string outputFile { get; set; }

        public override bool Execute() {
            return true;
        }
    }
}
