using System.Windows;
using MemoryCleaner.ViewModels;

namespace MemoryCleaner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
