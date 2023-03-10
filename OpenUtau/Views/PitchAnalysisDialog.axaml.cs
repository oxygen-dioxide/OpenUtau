using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;

namespace OpenUtau.App.Views {
    public partial class PitchAnalysisDialog : Window {
        
        public PitchAnalysisDialog() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        void OkButtonClick(object sender, RoutedEventArgs e) {//object? sender, RoutedEventArgs e
            var viewModel = this.DataContext as PitchAnalysisViewModel;
            
            if (viewModel == null) {
                return;
            }
            viewModel.Analyze();
            Close();
        }
    }
}
