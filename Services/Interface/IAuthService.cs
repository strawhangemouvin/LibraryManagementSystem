using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LibraryManagementSystem.Models.ViewModel;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IAuthService
    {
        LoginResponse Login(LoginRequest request);
        object Register(RegisterRequest request);
        object ResetPasswordToHash(string username, string newPassword);
    }
}