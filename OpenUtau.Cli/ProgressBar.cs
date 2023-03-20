using System.Runtime.CompilerServices;
using ShellProgressBar;

namespace OpenUtau.Cli.Util {
    public class UShellProgressBar : ProgressBar {
        static ProgressBarOptions options = new ProgressBarOptions {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };
        static int maxTicks = 100;
        static string message = "";
        IProgress<float> iprogress;

        public UShellProgressBar() 
            : base(maxTicks, message, options) {
            iprogress = this.AsProgress<float>();
        }
        
        public void Notify(double progress, string info) {
            Tick(info);
            iprogress.Report((float)progress/maxTicks);
        }
    }
}
