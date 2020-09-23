using AzureArtifactManager.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace AzureArtifactManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }
    }
}
