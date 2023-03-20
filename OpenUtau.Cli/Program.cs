// See https://aka.ms/new-console-template for more information
using System.Resources;
using CommandLine;
using CommandLine.Text;

using OpenUtau.Cli.Commands;

namespace OpenUtau.Cli {
    class Program {
        private static int Main(string[] args) {
            Parser myParser = new Parser(config => {
                config.HelpWriter = null;
                config.CaseSensitive = false;
            });

            var types = new Type[] {
                typeof(ClearCacheCommand),
                typeof(InstallCommand),
                typeof(RenderCommand) };

            var parserResult = myParser.ParseArguments(args, types);

            BaseCommand command = parserResult.MapResult(c => c as BaseCommand, err => null);

            if (command == null) {
                DisplayHelp(parserResult as NotParsed<object>);
                DisplayParsingErrors(parserResult as NotParsed<object>);
                return args.Any() ? 1 : 0;
            }

            return command.Execute() ? 0 : 1;
        }

        private static void DisplayHelp(NotParsed<object> result) {
            var helpText = HelpText.AutoBuild(
                result,
                h => {
                    h.AddDashesToOption = true;
                    h.AdditionalNewLineAfterOption = false;
                    h.AddNewLineBetweenHelpSections = true;
                    h.MaximumDisplayWidth = 100;
                    h.AutoHelp = false;
                    h.AutoVersion = false;
                    return h;
                },
                e => e,
                verbsIndex: true);
            Console.WriteLine(helpText);
            Console.WriteLine();
        }

        private static void DisplayParsingErrors<T>(NotParsed<T> result) {
            if (!result.Errors.Any(
                e => e is NoVerbSelectedError ||
                (e is BadVerbSelectedError badVerbError && badVerbError.Token == "-?") ||
                (e is UnknownOptionError unknownOptionError && unknownOptionError.Token == "?"))) {
                var builder = SentenceBuilder.Create();
                var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

                /*foreach (var error in errorMessages) {
                    Logger.Warn(error);
                }*/
            }
        }
    }
}
