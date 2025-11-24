using System.Windows;
using System.Windows.Media.Animation;

namespace WpfBingo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly BingoViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new BingoViewModel();
        DataContext = _viewModel;

        // Subscribe to property changed to trigger animation
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(BingoViewModel.CurrentNumber) && _viewModel.CurrentNumber.HasValue)
            {
                var storyboard = (Storyboard)FindResource("NumberDrawAnimation");
                storyboard.Begin();
            }
        };
    }
}