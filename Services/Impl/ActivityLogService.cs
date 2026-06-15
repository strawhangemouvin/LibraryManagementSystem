using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class ActivityLogService : IActivityLogService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public List<object> GetAllLogs()
        {
            var logs = db.ActivityLogs
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    UserName = db.Users
                        .Where(u => u.Id == x.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    x.Action,
                    x.Description,
                    x.CreatedAt
                })
                .ToList<object>();

            return logs;
        }

        public List<object> GetLogsByUser(int userId)
        {
            var logs = db.ActivityLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    UserName = db.Users
                        .Where(u => u.Id == x.UserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    x.Action,
                    x.Description,
                    x.CreatedAt
                })
                .ToList<object>();

            return logs;
        }
    }
}