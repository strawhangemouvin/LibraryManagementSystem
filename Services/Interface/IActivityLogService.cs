using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IActivityLogService
    {
        List<object> GetAllLogs();
        List<object> GetLogsByUser(int userId);
    }
}