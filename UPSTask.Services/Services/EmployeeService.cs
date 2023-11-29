using System.Net.Http.Headers;
using UPSTask.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UPSTask.ExtensionMethods;

namespace UPSTask.Services.Services
{
    public class EmployeeService : IEmployeeService
    {
        private const string URL = "https://gorest.co.in/public/v2/";
        private const string API_TOKEN = "0bf7fb56e6a27cbcadc402fc2fce8e3aa9ac2b40d4190698eb4e8df9284e2023";

        public async Task<RequestResult<IEnumerable<EmployeeGridModel>?>> GetEmployeeGridModelAsync(int? pageNumber)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync($"users?page={pageNumber ?? 1}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseResultString = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var responseResult = JsonConvert.DeserializeObject<IEnumerable<EmployeeGridModel>>(responseResultString);
                        return RequestResult.Success(responseResult);
                    }
                    catch (Exception ex)
                    {
                        return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError(ex.Message));
                    }
                }
            }

            return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError("No Result Found"));
        }

        public async Task<RequestResult<IEnumerable<EmployeeGridModel>?>> GetEmployeeGridModelAsyncByEmployeeId(string value, string? field)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response;

                if(field == null)
                {
                    response = client.GetAsync($"users/{value}").Result;
                }
                else
                {
                    response = client.GetAsync($"users?{field.ToLower()}={value}").Result;
                }
                

                if (response.IsSuccessStatusCode)
                {
                    var responseResultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    try
                    {
                        var responseResult = JsonConvert.DeserializeObject<IEnumerable<EmployeeGridModel>>(responseResultString);
                        return RequestResult.Success(responseResult);
                    }
                    catch (Exception ex)
                    {
                        return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError(ex.Message));
                    }
                }
            }

            return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError("No Result Found"));
        }

        public async Task<RequestResult<bool>> DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_TOKEN);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.DeleteAsync($"users/{Convert.ToInt32(employeeId.ToString())}").ConfigureAwait(false);

                    if(response.IsSuccessStatusCode)
                    {
                        return RequestResult.Success(true);
                    }
                    else
                    {
                        return RequestResult.Fail<bool>(new RequestError(response.ReasonPhrase));
                    }
                }
            }
            catch(Exception ex)
            {
                return RequestResult.Fail<bool>(new RequestError(ex.Message));
            }
        }

        public RequestResult<bool> ExportPDF(string filePath, IEnumerable<EmployeeGridModel> employeeGrid)
        {
            // Create the CSV file to which grid data will be exported.
            StreamWriter sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write));

            try
            {
                DataTable dt = employeeGrid.ToDataTable<EmployeeGridModel>();
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
                sw.Close();

                return RequestResult.Success(true);
            }
            catch (Exception ex)
            {
                return RequestResult.Fail<bool>(new RequestError(ex.Message));
            }
        }
    }
}
