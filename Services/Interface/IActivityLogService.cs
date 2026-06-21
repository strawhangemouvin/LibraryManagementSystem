using LibraryManagementSystem.Models.ViewModel;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IActivityLogService
    {
        List<ActivityLogViewModel> GetAllLogs();
        List<ActivityLogViewModel> GetLogsByUser(int userId);
    }
}