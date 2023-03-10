using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace OpenUtau.Core.Analysis {
    public abstract class PitchAnalyzerInst {
        public abstract double[] Analyze(ISampleProvider sampleProvider, double stepMs);

    }

    public abstract class PitchAnalyzer {
        public abstract string Name { get; }
        public override string ToString() => Name;
        public abstract PitchAnalyzerInst Inst();
        public double[] Analyze(ISampleProvider sampleProvider, double stepMs) {
            return Inst().Analyze(sampleProvider, stepMs);
        }
        public IEnumerable<double[]> BatchAnalyze(IEnumerable<ISampleProvider> sampleProvider, double stepMs) {
            var inst = Inst();
            return sampleProvider.Select(sample => inst.Analyze(sample, stepMs));
        }
    }

    public class CrepePitchAnalyzerInst : PitchAnalyzerInst {
        Crepe.Crepe crepe;
        
        public CrepePitchAnalyzerInst() { 
            crepe = new Crepe.Crepe();
        }

        public override double[] Analyze(ISampleProvider sampleProvider, double stepMs) {
            var signal = Core.Format.Wave.GetSignal(sampleProvider.ToMono(1, 0));
            if (signal != null) {
                return crepe.ComputeF0(signal, stepMs);
            }
            return null;
        }
    }

    public class CrepePitchAnalyzer : PitchAnalyzer {
        public override string Name { get { return "crepe"; } }
        public override PitchAnalyzerInst Inst() { return new CrepePitchAnalyzerInst(); }
    }

    public class WorldlinePitchAnalyzer : PitchAnalyzer {
        string name;
        int f0Method;

        public WorldlinePitchAnalyzer(string name, int f0Method) {
            this.name = name;
            this.f0Method = f0Method;
        }

        public override string Name { get { return name; } }
        public override PitchAnalyzerInst Inst() { return new WorldlinePitchAnalyzerInst(f0Method); }
    }
    

    public class WorldlinePitchAnalyzerInst : PitchAnalyzerInst {
        int f0Method;
        
        public WorldlinePitchAnalyzerInst(int f0Method) {
            this.f0Method = f0Method;
        }

        public override double[] Analyze(ISampleProvider sampleProvider, double stepMs) {
            var samples = Core.Format.Wave.GetSamples(sampleProvider.ToMono(1, 0));
            if (samples != null) {
                return Core.Render.Worldline.F0(samples, 44100, stepMs, f0Method);
            }
            return null;
        }
    }

    public static class PitchAnalyzerManager {
        public static List<PitchAnalyzer> analyzers = new List<PitchAnalyzer> {
            new WorldlinePitchAnalyzer("DIO", 0),
            new WorldlinePitchAnalyzer("DIO(5x)", 1),
            new WorldlinePitchAnalyzer("pYIN", 2),
            new CrepePitchAnalyzer()
        };
    }

    
}
