using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.ComponentModel;
using System.IO;
using UPSTask.Services.Services;
using UPSTask.Model;
using UPSTask.ExtensionMethods;
using System.Text.RegularExpressions;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UPSTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool IsMaximised = false;
        private int CurrentPage = 1;
        private IEmployeeService _employeeService;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            InitializeComponent();
            var userData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
            employeeDetailsDataGrid.ItemsSource = userData.GetResult();
        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ClickCount==2)
            {
                if(IsMaximised)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1250;
                    this.Height = 900;

                    IsMaximised = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximised = true;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentPage > 1)
            {
                CurrentPage -= 1;
                var userData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
                employeeDetailsDataGrid.ItemsSource = userData.GetResult();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage += 1;
            var userData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
            employeeDetailsDataGrid.ItemsSource = userData.GetResult();
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialogPath = new SaveFileDialog();

            saveFileDialogPath.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialogPath.FilterIndex = 2;
            saveFileDialogPath.RestoreDirectory = true;
            saveFileDialogPath.FileName = "EmployeeData";
            saveFileDialogPath.DefaultExt = ".csv";

            if (saveFileDialogPath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var filepath = saveFileDialogPath.FileName;

                var employees = employeeDetailsDataGrid.ItemsSource as IEnumerable<EmployeeGridModel>;

                var result = _employeeService.ExportPDF(filepath, employees);

                if (result.IsSuccess)
                {
                    FileSavePath.Text = $"File successfully saved to - {filepath}";
                }
                else
                {
                    FileSavePath.Text = $"Error occurred while savig the file - {result.Error}";
                }
            }
        }

        private void FindByIdButton_Click(object sender, RoutedEventArgs e)
        {
            NoRecordsFound.Visibility = Visibility.Hidden;
            if (txtSearch.Text.Length > 0)
            {
                GetByEmployeeId();
            }
            else
            {
                txtSearch.Text = string.Empty;
                var employeeData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
                employeeDetailsDataGrid.ItemsSource = employeeData.GetResult();
                employeeDetailsDataGrid.Visibility = Visibility.Visible;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = 1;
            var employeeData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
            
            this.Dispatcher.Invoke((Action)(() =>
            {
                txtSearch.Text = "";
                if (employeeData != null)
                {
                    NoRecordsFound.Visibility = Visibility.Hidden;
                    employeeDetailsDataGrid.ItemsSource = employeeData.GetResult();
                    employeeDetailsDataGrid.Visibility = Visibility.Visible;
                    txtSearch.Text = null;
                }
                else
                {
                    NoRecordsFound.Visibility = Visibility.Visible;
                    employeeDetailsDataGrid.ItemsSource = null;
                    employeeDetailsDataGrid.Visibility = Visibility.Hidden;
                    txtSearch.Text = null;
                }
            }));
        }

        private async void GetByEmployeeId()
        {
            ComboBoxItem typeItem = (ComboBoxItem)SearchField.SelectedItem;
            string selectedValue = typeItem.Content.ToString();

            var data = await _employeeService.GetEmployeeGridModelAsyncByEmployeeId(txtSearch.Text, selectedValue);
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (data != null)
                {
                    employeeDetailsDataGrid.ItemsSource = data.GetResult();
                    employeeDetailsDataGrid.Visibility = Visibility.Visible;
                    NoRecordsFound.Visibility = Visibility.Hidden;
                }
                else
                {
                    employeeDetailsDataGrid.ItemsSource = null;
                    employeeDetailsDataGrid.Visibility = Visibility.Hidden;
                    NoRecordsFound.Visibility = Visibility.Visible;
                }
            }));
        }

        private async void Delete_ClickAsync(object sender, RoutedEventArgs e)
        {
            EmployeeGridModel selectedEmployeeModel = (EmployeeGridModel)employeeDetailsDataGrid.SelectedItem;
            var result = await _employeeService.DeleteEmployeeAsync(selectedEmployeeModel.Id);

            if (result.IsSuccess)
            {
                this.Dispatcher.Invoke((Action)(async () =>
                {
                    var employeeResult = await _employeeService.GetEmployeeGridModelAsync(CurrentPage);
                    employeeDetailsDataGrid.ItemsSource = employeeResult.GetResult();
                    ApiResponse.Text = $"Employee: {selectedEmployeeModel.Name}, Deleted Successfully.";
                }));
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    ApiResponse.Text = $"Error - {result.Error}";
                }));
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)SearchField.SelectedItem;
            string selectedValue = typeItem.Content.ToString();

            if(selectedValue != "Name")
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }
    }
}
