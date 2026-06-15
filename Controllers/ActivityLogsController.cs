using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    [TokenAuth]
    public class ActivityLogsController : ApiController
    {
        private readonly IActivityLogService activityLogService;

        public ActivityLogsController()
        {
            activityLogService = new ActivityLogService();
        }

        [HttpGet]
        [Route("api/activitylogs/GetAll")]
        public IHttpActionResult GetAll()
        {
            var logs = activityLogService.GetAllLogs();
            return Ok(logs);
        }

        [HttpGet]
        [Route("api/activitylogs/GetByUser/{userId}")]
        public IHttpActionResult GetByUser(int userId)
        {
            var logs = activityLogService.GetLogsByUser(userId);
            return Ok(logs);
        }
    }
}