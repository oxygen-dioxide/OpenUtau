using CommandLine;

namespace OpenUtau.Cli.Commands {
    [Verb("list", HelpText = "List installed voivebanks")]
    public class ListCommand : BaseCommand {
        public override bool Execute() {
            //TODO
            return true;
        }
    }
}