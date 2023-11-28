using System.Net.Http.Headers;
using UPSTask.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UPSTask.Services.Services
{
    public class EmployeeService : IEmployeeService
    {
        private const string URL = "https://gorest.co.in/public/v2/";
        private const string API_TOKEN = "0bf7fb56e6a27cbcadc402fc2fce8e3aa9ac2b40d4190698eb4e8df9284e2023";
        private int CurrentPage = 1;

        /// <summary>
        /// Get All Users
        /// </summary>
        /// <returns>List of Users</returns>
        public async Task<IEnumerable<UserGridModel>?> GetuserGridModelAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync($"users?page={CurrentPage}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseResultString = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var responseResult = JsonConvert.DeserializeObject<IEnumerable<UserGridModel>>(responseResultString);
                        return responseResult;
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get User By UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Single Row of User</returns>
        public async Task<UserGridModel?> GetUserGridModelAsyncByUserId(int userId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync($"users/{userId}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseResultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    try
                    {
                        var responseResult = JsonConvert.DeserializeObject<UserGridModel>(responseResultString);
                        return responseResult;
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                        return null;
                    }
                }
            }

            return null;
        }

        ///// <summary>
        ///// Update User
        ///// </summary>
        ///// <returns>Updated Record</returns>
        //private async Task<UserGridModel> Update_ClickAsync(UserGridModel userGridModel)
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(URL);
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            HttpResponseMessage response = await client.PutAsJsonAsync($"users/{userGridModel.id}", userGridModel).ConfigureAwait(false);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                this.Dispatcher.Invoke((Action)(() =>
        //                {//this refer to form in WPF application 
        //                    UserDataGrid.ItemsSource = GetuserGridModelAsync().Result;
        //                    ApiResponse.Text = $"Upated Successfully.";
        //                }));

        //                return await response.Content.ReadAsAsync<UserGridModel>();
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
        //    return null;
        //}
    }
}
