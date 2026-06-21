using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class ActivityLogService : IActivityLogService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public List<ActivityLogViewModel> GetAllLogs()
        {
            var logs = db.ActivityLogs
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLogViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = db.Users
                        .Where(u => u.Id == x.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    Action = x.Action,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return logs;
        }

        public List<ActivityLogViewModel> GetLogsByUser(int userId)
        {
            var logs = db.ActivityLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLogViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = db.Users
                        .Where(u => u.Id == x.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    Action = x.Action,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return logs;
        }
    }
}