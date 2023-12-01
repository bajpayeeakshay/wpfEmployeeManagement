using UPSTask.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UPSTask.ExtensionMethods;
using System.Linq;

namespace UPSTask.Services.Services
{
    public class EmployeeService : IEmployeeService
    {
        private HttpClient _httpClient;
        public EmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RequestResult<IEnumerable<EmployeeGridModel>?>> GetEmployeeGridModelAsync(int? pageNumber)
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync($"users?page={pageNumber ?? 1}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseResultString = await response.Content.ReadAsStringAsync();

                    var responseResult = JsonConvert.DeserializeObject<IEnumerable<EmployeeGridModel>>(responseResultString);
                    return RequestResult.Success(responseResult);
                }
            }
            catch(Exception ex)
            {
                return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError(ex.Message));
            }

            return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError("No Result Found"));
        }

        public async Task<RequestResult<IEnumerable<EmployeeGridModel>?>> GetEmployeeGridModelAsyncByEmployeeId(string value, string? field)
        {
            try
            {
                string endpoint = field == null ? $"users/{value}" : $"users?{field.ToLower()}={value}";
                HttpResponseMessage response = _httpClient.GetAsync(endpoint).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseResultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var responseResult = JsonConvert.DeserializeObject<IEnumerable<EmployeeGridModel>>(responseResultString);
                    return RequestResult.Success(responseResult);
                }
            }
            catch (Exception ex)
            {
                return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError(ex.Message));
            }

            return RequestResult.Fail<IEnumerable<EmployeeGridModel>?>(new RequestError("No Result Found"));
        }

        public async Task<RequestResult<bool>> DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"users/{Convert.ToInt32(employeeId.ToString())}").ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return RequestResult.Success(true);
                }
                else
                {
                    return RequestResult.Fail<bool>(new RequestError(response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                return RequestResult.Fail<bool>(new RequestError(ex.Message));
            }
        }

        public RequestResult<bool> ExportPDF(string filePath, IEnumerable<EmployeeGridModel> employeeGrid)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write)))
                {
                    DataTable dt = employeeGrid.ToDataTable<EmployeeGridModel>();

                    // Write header
                    sw.WriteLine(string.Join(",", dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName)));

                    // Write rows
                    foreach (DataRow dr in dt.Rows)
                    {
                        sw.WriteLine(string.Join(",", dr.ItemArray.Select(value => value.ToString())));
                    }
                }

                return RequestResult.Success(true);
            }
            catch (Exception ex)
            {
                return RequestResult.Fail<bool>(new RequestError(ex.Message));
            }
        }
    }
}
