using System.Windows;
using TaskManagerClone.ViewModels;

namespace TaskManagerClone
{
    public partial class ProcessListWindow : Window
    {
        private readonly ProcessListViewModel _viewModel;

        public ProcessListWindow()
        {
            InitializeComponent();
            _viewModel = new ProcessListViewModel();
            DataContext = _viewModel;
        }

        private void EndTask_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedProcess != null)
            {
                var result = MessageBox.Show($"Are you sure you want to end {_viewModel.SelectedProcess.Name}?", 
                    "Confirm End Task", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.EndTask(_viewModel.SelectedProcess);
                }
            }
        }

        private void OpenLocation_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedProcess != null)
            {
                _viewModel.OpenFileLocation(_viewModel.SelectedProcess);
            }
        }

        private void SearchOnline_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedProcess != null)
            {
                _viewModel.SearchOnline(_viewModel.SelectedProcess);
            }
        }
    }
}
