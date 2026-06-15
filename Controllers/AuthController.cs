using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    public class AuthController : ApiController
    {
        private readonly IAuthService authService;

        public AuthController()
        {
            authService = new AuthService();
        }

        [HttpPost]
        [Route("api/auth/login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = authService.Login(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/auth/register")]
        public IHttpActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = authService.Register(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //[HttpPost]
        //[Route("api/auth/reset-password-hash")]
        //public IHttpActionResult ResetPasswordHash(string username, string newPassword)
        //{
        //    try
        //    {
        //        var response = authService.ResetPasswordToHash(username, newPassword);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}