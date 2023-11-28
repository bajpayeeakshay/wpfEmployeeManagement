using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data;
using System.ComponentModel;
using System.IO;
using UPSTask.Services.Services;
using UPSTask.Model;
using UPSTask.ExtensionMethods;
using System.Text.RegularExpressions;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using DataGrid = System.Windows.Controls.DataGrid;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UPSTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool IsMaximised = false;
        private const string URL = "https://gorest.co.in/public/v2/";
        private const string API_TOKEN = "0bf7fb56e6a27cbcadc402fc2fce8e3aa9ac2b40d4190698eb4e8df9284e2023";
        private int CurrentPage = 1;
        private IEmployeeService _employeeService;
        //private List<ComboBoxItem> GenderComboBoxValue = new List<ComboBoxItem> {
        //                                                        new ComboBoxItem {
        //                                                            Content = "",
        //                                                            HorizontalContentAlignment = HorizontalAlignment.Center,
        //                                                            VerticalContentAlignment = VerticalAlignment.Center },
        //                                                        new ComboBoxItem { 
        //                                                            Content = "female", 
        //                                                            HorizontalContentAlignment = HorizontalAlignment.Center, 
        //                                                            VerticalContentAlignment = VerticalAlignment.Center }, 
        //                                                        new ComboBoxItem { Content = "male",
        //                                                            HorizontalContentAlignment = HorizontalAlignment.Center,
        //                                                            VerticalContentAlignment = VerticalAlignment.Center } };

        //private List<ComboBoxItem> StatusComboBoxValue = new List<ComboBoxItem> {
        //                                                        new ComboBoxItem {
        //                                                            Content = "",
        //                                                            HorizontalContentAlignment = HorizontalAlignment.Center,
        //                                                            VerticalContentAlignment = VerticalAlignment.Center },
        //                                                        new ComboBoxItem {
        //                                                            Content = "active",
        //                                                            HorizontalContentAlignment = HorizontalAlignment.Center,
        //                                                            VerticalContentAlignment = VerticalAlignment.Center },
        //                                                        new ComboBoxItem { Content = "inactive",
        //                                                            HorizontalContentAlignment = HorizontalAlignment.Center,
        //                                                            VerticalContentAlignment = VerticalAlignment.Center } };

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            InitializeComponent();
            //GenderComboBox.ItemsSource = GenderComboBoxValue;
            //StatusComboBox.ItemsSource = StatusComboBoxValue;
            var userData = _employeeService.GetuserGridModelAsync().Result;
            employeeDetailsDataGrid.ItemsSource = userData;
        }

        public MainWindow()
        {
            var userData = _employeeService.GetuserGridModelAsync().Result;
            employeeDetailsDataGrid.ItemsSource = userData;
        }

        #region Action_Events
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

        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.Dispatcher.Invoke((Action)(() =>
            //{//this refer to form in WPF application 
            //    ApiResponse.Text = null;
            //}));

            DataGrid gd = (DataGrid)sender;
            UserGridModel rv = (UserGridModel)gd.SelectedItem;

            //if (rv != null)
            //{

            //    //var index = GenderComboBox.Items
            //    //GenderComboBox.Items.IndexOf(GenderComboBoxValue.Where(t => t.Content != null && t.Content.ToString() == rv.gender));
            //    IdVal.Content = rv.id;
            //    NameVal.Text = rv.name;
            //    EmailVal.Text = rv.email;
            //    GenderComboBox.SelectedIndex = GenderComboBoxValue.FindIndex(t => t.Content != null && t.Content.ToString() == rv.gender);
            //    StatusComboBox.SelectedIndex = StatusComboBoxValue.FindIndex(t => t.Content != null && t.Content.ToString() == rv.status);
            //}
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentPage > 1)
            {
                CurrentPage -= 1;
                //PageField.Text = CurrentPage.ToString();
                var userData = _employeeService.GetuserGridModelAsync().Result;
                employeeDetailsDataGrid.ItemsSource = userData;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage += 1;
            //PageField.Text = CurrentPage.ToString();
            var userData = _employeeService.GetuserGridModelAsync().Result;
            employeeDetailsDataGrid.ItemsSource = userData;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var newPage = PageField.Text;
            //if(newPage.Length > 0)
            //{
            //    CurrentPage = Convert.ToInt32(newPage.Trim());
            //}
            var userData = _employeeService.GetuserGridModelAsync().Result;
            employeeDetailsDataGrid.ItemsSource = userData;

        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialogPath = new SaveFileDialog();

            saveFileDialogPath.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialogPath.FilterIndex = 2;
            saveFileDialogPath.RestoreDirectory = true;
            saveFileDialogPath.FileName = "EmployeeData";
            saveFileDialogPath.DefaultExt = ".csv";

            if(saveFileDialogPath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var filepath = saveFileDialogPath.FileName;
                // Create the CSV file to which grid data will be exported.
                StreamWriter sw = new StreamWriter(new FileStream(filepath, FileMode.Create, FileAccess.Write));

                try
                {
                    var items = employeeDetailsDataGrid.ItemsSource as IList<UserGridModel>;
                    var json = JsonConvert.SerializeObject(items);
                    List<UserGridModel> UserList = JsonConvert.DeserializeObject<List<UserGridModel>>(json);
                    DataTable dt = UserList.ToDataTable<UserGridModel>();
                    int iColCount = dt.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        sw.Write(dt.Columns[i]);
                        if (i < iColCount - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                    // Now write all the rows.
                    foreach (DataRow dr in dt.Rows)
                    {
                        for (int i = 0; i < iColCount; i++)
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {
                                sw.Write(dr[i].ToString());
                            }
                            if (i < iColCount - 1)
                            {
                                sw.Write(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                    FileSavePath.Text = $"File successfully saved to - {filepath}";
                    sw.Close();
                }
                catch (Exception ex)
                {
                    FileSavePath.Text = $"Error - {ex.Message}";
                }
            }
        }

        //public void btnUpdateClick(object sender, RoutedEventArgs e)
        //{
        //    UpdateData();
        //}

        //public void btnDeleteClick(object sender, RoutedEventArgs e)
        //{
        //    Delete_ClickAsync();
        //    ResetForm();
        //}

        private void FindByIdButton_Click(object sender, RoutedEventArgs e)
        {
            int result = 0;
            NoRecordsFound.Visibility = Visibility.Hidden;
            if (Int32.TryParse(txtSearch.Text, out result))
            {
                GetByUserId(result);
            }
            else
            {
                txtSearch.Text = string.Empty;
                var userData = _employeeService.GetuserGridModelAsync().Result;
                employeeDetailsDataGrid.ItemsSource = userData;
                employeeDetailsDataGrid.Visibility = Visibility.Visible;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = 1;
            //PageField.Text = CurrentPage.ToString();
            var userData = _employeeService.GetuserGridModelAsync().Result;
            
            this.Dispatcher.Invoke((Action)(() =>
            {
                txtSearch.Text = "";
                ResetForm();
                if (userData != null)
                {
                    NoRecordsFound.Visibility = Visibility.Hidden;
                    employeeDetailsDataGrid.Visibility = Visibility.Visible;
                    employeeDetailsDataGrid.ItemsSource = userData;
                }
                else
                {
                    employeeDetailsDataGrid.ItemsSource = null;
                    employeeDetailsDataGrid.Visibility = Visibility.Hidden;
                    NoRecordsFound.Visibility = Visibility.Visible;
                }
            }));
        }

        private void ResetForm()
        {
            //IdVal.Content = "";
            //NameVal.Text = "";
            //EmailVal.Text = "";
            //GenderComboBox.SelectedIndex = GenderComboBoxValue.FindIndex(t => t.Content != null && t.Content.ToString() == "");
            //StatusComboBox.SelectedIndex = StatusComboBoxValue.FindIndex(t => t.Content != null && t.Content.ToString() == "");
            //ApiResponse.Text = "";
            //FileSavePath.Text = "";
        }
        #endregion Action_Events

        #region intermediary methods
        //public async void UpdateData()
        //{
        //    var result = await Update_ClickAsync().ConfigureAwait(false);
        //}

        private async void GetByUserId(int userId)
        {
            var data = await _employeeService.GetUserGridModelAsyncByUserId(userId);
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (data != null)
                {
                    employeeDetailsDataGrid.ItemsSource = new List<UserGridModel>() { data };
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

        #endregion intermediary methods

        #region API_METHODS

        ///// <summary>
        ///// Get All Users
        ///// </summary>
        ///// <returns>List of Users</returns>
        //public async Task<IEnumerable<UserGridModel>> GetuserGridModelAsync()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        this.Dispatcher.Invoke((Action)(() =>
        //        {//this refer to form in WPF application 
        //            ApiResponse.Text = null;
        //        }));

        //        client.BaseAddress = new Uri(URL);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        HttpResponseMessage response = client.GetAsync($"users?page={CurrentPage}").Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseResultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        //            try
        //            {
        //                var responseResult = JsonConvert.DeserializeObject<IEnumerable<UserGridModel>>(responseResultString);
        //                return responseResult;
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.Write(ex.Message);
        //                return null;
        //            }
        //        }
        //    }

        //    return null;
        //}

        ///// <summary>
        ///// Get User By UserId
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns>Single Row of User</returns>
        //public async Task<UserGridModel> GetUserGridModelAsyncByUserId(int userId)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        this.Dispatcher.Invoke((Action)(() =>
        //        {//this refer to form in WPF application 
        //            ApiResponse.Text = null;
        //        }));

        //        client.BaseAddress = new Uri(URL);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        HttpResponseMessage response = client.GetAsync($"users/{userId}").Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseResultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        //            try
        //            {
        //                var responseResult = JsonConvert.DeserializeObject<UserGridModel>(responseResultString);
        //                return responseResult;
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.Write(ex.Message);
        //                return null;
        //            }
        //        }
        //    }

        //    return null;
        //}

        /// <summary>
        /// Update User
        /// </summary>
        /// <returns>Updated Record</returns>
        //private async Task<UserGridModel> Update_ClickAsync()
        //{
        //    try
        //    {
        //        //var user = new UserGridModel()
        //        //{
        //        //    id = Convert.ToInt32(IdVal.Content.ToString()),
        //        //    name = NameVal.Text,
        //        //    email = EmailVal.Text,
        //        //    gender = GenderComboBox.Text,
        //        //    status = StatusComboBox.Text
        //        //};

        //        using (var client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(URL);
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            HttpResponseMessage response = await client.PutAsJsonAsync($"users/{user.id}", user).ConfigureAwait(false);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                this.Dispatcher.Invoke((Action)(() =>
        //                {//this refer to form in WPF application 
        //                    employeeDetailsDataGrid.ItemsSource = _employeeService.GetuserGridModelAsync().Result;
        //                    //ApiResponse.Text = $"Upated Successfully.";
        //                }));

        //                return await response.Content.ReadAsAsync<UserGridModel>();
        //            }
        //            else
        //            {
        //                this.Dispatcher.Invoke((Action)(() =>
        //                {
        //                    //ApiResponse.Text = $"Error - {response.ReasonPhrase}";
        //                }));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Dispatcher.Invoke((Action)(() =>
        //        {
        //            //ApiResponse.Text = $"Error - {ex.Message}";
        //        }));
        //    }
        //    return null;
        //}

        /// <summary>
        /// Delete the selected User
        /// </summary>
        //private async void Delete_ClickAsync()
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(URL);
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            HttpResponseMessage response = await client.DeleteAsync($"users/{Convert.ToInt32(IdVal.Content.ToString())}").ConfigureAwait(false);

        //            if (response.IsSuccessStatusCode)
        //            {

        //                this.Dispatcher.Invoke((Action)(() =>
        //                {
        //                    employeeDetailsDataGrid.ItemsSource = _employeeService.GetuserGridModelAsync().Result;
        //                    ApiResponse.Text = $"Deleted Successfully.";
        //                }));
        //            }
        //            else
        //            {
        //                this.Dispatcher.Invoke((Action)(() =>
        //                {
        //                    ApiResponse.Text = $"Error - {response.ReasonPhrase}";
        //                }));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Dispatcher.Invoke((Action)(() =>
        //        {
        //            ApiResponse.Text = $"Error - {ex.Message}";
        //        }));
        //    }
        //}

        #endregion API_METHODS

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
