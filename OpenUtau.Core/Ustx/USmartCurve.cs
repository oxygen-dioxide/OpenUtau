using System.Linq;

namespace OpenUtau.Core.Ustx{
    class USmartCurvePiece{
        public List<int> xs = new List<int>();
        public List<int> ys = new List<int>();
    }

    class USmartCurve{
        [YamlIgnore] public UExpressionDescriptor descriptor;

        /// <summary>
        /// List of curve pieces edited by the user.
        /// </summary>
        public List<USmartCurvePiece> pieces = new List<USmartCurvePiece>();
        public string abbr;

        [YamlIgnore] public bool IsEmpty => pieces.Count == 0;

        public USmartCurve(UExpressionDescriptor descriptor) {
            Trace.Assert(descriptor != null);
            this.descriptor = descriptor;
            abbr = descriptor.abbr;
        }
    }
}