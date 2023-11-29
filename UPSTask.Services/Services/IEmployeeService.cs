using System.Collections.Generic;
using System.Threading.Tasks;
using UPSTask.Model;

namespace UPSTask.Services.Services
{
    public interface IEmployeeService
    {
        public Task<RequestResult<IEnumerable<EmployeeGridModel>?>> GetEmployeeGridModelAsync(int? pageNumber);

        public Task<RequestResult<IEnumerable<EmployeeGridModel>?>> GetEmployeeGridModelAsyncByEmployeeId(string value, string? field);

        public Task<RequestResult<bool>> DeleteEmployeeAsync(int employeeId);

        public RequestResult<bool> ExportPDF(string filePath, IEnumerable<EmployeeGridModel> employeeGrid);
    }
}
