using CommandLine;
using OpenUtau.Core;

namespace OpenUtau.Cli.Commands {
    [Verb("clearcache", HelpText = "Clear Cache")]
    public class ClearCacheCommand : BaseCommand {
        public override bool Execute() {
            Console.WriteLine("Clearing cache...");
            PathManager.Inst.ClearCache();
            Console.WriteLine("Cache cleared.");
            return true;
        }
    }
}
