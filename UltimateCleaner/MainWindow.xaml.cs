using MemoryCleaner.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace MemoryCleaner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
