using System.Windows;
using TaskManagerClone.ViewModels;

namespace TaskManagerClone
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ProcessesButton_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = new ProcessListWindow();
            processWindow.Owner = this;
            processWindow.Show();
        }
    }
}