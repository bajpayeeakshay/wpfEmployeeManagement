using System.Collections.Generic;
using System.Threading.Tasks;
using UPSTask.Model;

namespace UPSTask.Services.Services
{
    public interface IEmployeeService
    {
        public Task<IEnumerable<UserGridModel>?> GetuserGridModelAsync();

        public Task<UserGridModel?> GetUserGridModelAsyncByUserId(int userId);
    }
}
