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
using System.Text.RegularExpressions;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.Windows.Controls;
using System.Linq;

namespace UPSTask
{
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
            ResetFields();
            Close();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
            if (CurrentPage > 1)
            {
                CurrentPage -= 1;
                var userData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
                employeeDetailsDataGrid.ItemsSource = userData.GetResult();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
            CurrentPage += 1;
            var userData = _employeeService.GetEmployeeGridModelAsync(CurrentPage).Result;
            employeeDetailsDataGrid.ItemsSource = userData.GetResult();
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialogPath = new SaveFileDialog();
            ResetFields();

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
                ResetFields();
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
                }
                else
                {
                    NoRecordsFound.Visibility = Visibility.Visible;
                    employeeDetailsDataGrid.ItemsSource = null;
                    employeeDetailsDataGrid.Visibility = Visibility.Hidden;
                }
                ResetFields();
            }));
        }

        private async void GetByEmployeeId()
        {
            ComboBoxItem typeItem = (ComboBoxItem)SearchField.SelectedItem;
            string selectedValue = typeItem.Content.ToString();

            var result = await _employeeService.GetEmployeeGridModelAsyncByEmployeeId(txtSearch.Text, selectedValue);
            ResetFields();
            this.Dispatcher.Invoke((Action)(() =>
            {
                var data = result.GetResult();
                if (data != null && data.Any() && data.Count() > 0)
                {
                    employeeDetailsDataGrid.ItemsSource = data;
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
            ResetFields();

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
            
            if(typeItem != null)
            {
                var selectedValue = typeItem.Content;

                if (selectedValue.ToString() == "Name")
                {
                    Regex regex = new Regex("/^[A-Za-z]+$/");
                    e.Handled = regex.IsMatch(e.Text);
                }
                else
                {
                    Regex regex = new Regex("^[0-9]+$");
                    e.Handled = regex.IsMatch(e.Text);
                }
            }
            else
            {
                Regex regex = new Regex("^[0-9]+$");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void ResetFields()
        {
            txtSearch.Text = null;
            ApiResponse.Text = null;
            FileSavePath.Text = null;
            SearchField.SelectedItem = null;
        }

        private void SearchFieldSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)SearchField.SelectedItem;
            if(typeItem != null)
            {
                var selectedItem = typeItem.Content.ToString();
                ComboBoxTextBlock.Text = selectedItem;
            }
        }
    }
}
