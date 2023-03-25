using CommandLine;
using OpenUtau.Core;

namespace OpenUtau.Cli.Commands {
    [Verb("clear", HelpText = "Clear Cache")]
    public class ClearCommand : BaseCommand {
        public override bool Execute() {
            Console.WriteLine("Clearing cache...");
            PathManager.Inst.ClearCache();
            Console.WriteLine("Cache cleared.");
            return true;
        }
    }
}
