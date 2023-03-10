using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using OpenUtau.Classic;
using OpenUtau.Core;
using OpenUtau.Core.Analysis;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Format;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Melanchall.DryWetMidi.Core;

namespace OpenUtau.App.ViewModels {

    public struct WavePartOption {
        public UWavePart wavePart;
        override public string ToString() => wavePart.name;

        public WavePartOption(UWavePart wavePart) {
            this.wavePart = wavePart;
        }
    }

    public class PitchAnalysisViewModel : ViewModelBase {
        public List<PitchAnalyzer> analyzers { get; set; }
        [Reactive] public PitchAnalyzer analyzer { get; set; }
        public List<WavePartOption> waveParts { get; set; }
        [Reactive] public WavePartOption wavePart { get; set; }

        NotesViewModel notesViewModel;
        public PitchAnalysisViewModel(NotesViewModel notesViewModel) {
            analyzers = PitchAnalyzerManager.analyzers;
            analyzer = analyzers[0];
            waveParts = waveParts = DocManager.Inst.Project.parts
                .Select(part => part as UWavePart)
                .Where(part => part != null)
                .Select(part=>new WavePartOption(part)).ToList();
            wavePart = waveParts[0];
            this.notesViewModel = notesViewModel;
        }

        public void Analyze() {
            //prepare part and note
            var part = notesViewModel.Part;
            var selectedNotes = notesViewModel.Selection.ToList();
            var notes = selectedNotes.Count > 0 ? selectedNotes : part.notes.ToList();
            var positions = notes.Select(n => n.position + part.position).ToHashSet();
            var docManager = DocManager.Inst;
            var project = docManager.Project;

            //Analyze pitch from audio
            double stepMs = Frq.kHopSize * 1000.0 / 44100;
            string file = wavePart.wavePart.FilePath;
            if (!File.Exists(file)) {
                throw new FileNotFoundException(string.Format("File {0} missing!", file));
            }
            ISampleProvider sampleProvider;
            float[] tones;
            using (var waveStream = Core.Format.Wave.OpenFile(file)) {
                sampleProvider = waveStream.ToSampleProvider().ToMono(1, 0);
                tones = analyzer.Analyze(sampleProvider, stepMs)
                    .Select(f => (float)MusicMath.FreqToTone(f)).ToArray();
            }
            
            //prepare timeline
            var timeAxis = project.timeAxis;
            var offsetMs = wavePart.wavePart.position;
            var ticks = Enumerable.Range(0, tones.Length)
                .Select(x=>timeAxis.MsPosToTickPos(offsetMs+x*stepMs)).ToArray();

            //put pitch curves into part
            float minPitD = -1200;
            if (project.expressions.TryGetValue(Ustx.PITD, out var descriptor)) {
                minPitD = descriptor.min;
            }

            var phrases = notesViewModel.Part.renderPhrases
                .Where(phrase => phrase.notes.Any(n => positions.Contains(phrase.position + n.position))).ToList();
            docManager.StartUndoGroup();
            int i = 0;
            foreach (var phrase in phrases) {
                int? lastX = null;
                int? lastY = null;

                // TODO: Optimize interpolation and command.
                while(ticks[i]<=phrase.end) {
                    if (ticks[i] < phrase.position - phrase.leading) {
                        ++i;
                        continue;
                    }
                    if (tones[i] < 0) {
                        ++i;
                        continue;
                    }
                    int x = (int)ticks[i] - part.position;
                    int pitchIndex = Math.Clamp((x - (phrase.position - part.position - phrase.leading)) / 5, 0, phrase.pitches.Length - 1);
                    float basePitch = phrase.pitchesBeforeDeviation[pitchIndex];
                    int y = (int)(tones[i] * 100 - basePitch);
                    lastX ??= x;
                    lastY ??= y;
                    if (y > minPitD) {
                        docManager.ExecuteCmd(new SetCurveCommand(
                            project, part, Ustx.PITD, x, y, lastX.Value, lastY.Value));
                    }
                    lastX = x;
                    lastY = y;
                    ++i;
                }
            }
            docManager.EndUndoGroup();
        }
    }
}
