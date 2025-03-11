using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using OpenUtau.App.ViewModels;
using OpenUtau.Core.Ustx;

namespace OpenUtau.App.Controls {
    public partial class ExpSelector : UserControl {
        public static readonly DirectProperty<ExpSelector, int> IndexProperty =
            AvaloniaProperty.RegisterDirect<ExpSelector, int>(
                nameof(Index),
                o => o.Index,
                (o, v) => o.Index = v);
        
        public int Index {
            get => index;
            set => SetAndRaise(IndexProperty, ref index, value);
        }

        public UVoicePart? Part {
            get => part;
            set{
                part = value;
                ((ExpSelectorViewModel)DataContext!).Part = value;
                ((ExpSelectorViewModel)DataContext!).RefreshDescriptors();
            }
        }

        private int index;

        private UVoicePart? part;

        public ExpSelector() {
            InitializeComponent();
            DataContext = new ExpSelectorViewModel();
            ((ExpSelectorViewModel)DataContext!).Index = Index;
            ((ExpSelectorViewModel)DataContext!).Part = Part;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == IndexProperty) {
                ((ExpSelectorViewModel)DataContext!).Index = Index;
            }
        }

        private void TextBlockPointerPressed(object sender, PointerPressedEventArgs e) {
            ((ExpSelectorViewModel)DataContext!).OnSelected(true);
        }

        public void SelectExp() {
            ((ExpSelectorViewModel)DataContext!).OnSelected(false);
        }
    }
}
