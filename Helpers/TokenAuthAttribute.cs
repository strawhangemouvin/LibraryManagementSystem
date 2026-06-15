using LibraryManagementSystem.Services.Context;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace LibraryManagementSystem.Helpers
{
    public class TokenAuthAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!actionContext.Request.Headers.Contains("Authorization"))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    "Token tidak ditemukan. Silakan login terlebih dahulu."
                );
                return;
            }

            var token = actionContext.Request.Headers
                .GetValues("Authorization")
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(token))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    "Token kosong."
                );
                return;
            }

            token = token.Trim();

            if (token.StartsWith("Bearer "))
            {
                token = token.Replace("Bearer ", "").Trim();
            }

            using (var db = new LibraryDbContext())
            {
                var user = db.Users.FirstOrDefault(x =>
                    x.Token == token &&
                    x.Status == "Active"
                );

                if (user == null)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        "Token tidak valid atau akun tidak aktif."
                    );
                    return;
                }
            }

            base.OnAuthorization(actionContext);
        }
    }
}