using System.Windows;
using ExchangeRateWpf.ViewModels;

namespace ExchangeRateWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(Chart);
        }
    }
}
