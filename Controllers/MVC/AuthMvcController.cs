using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class AuthMvcController : Controller
    {
        private readonly IAuthService authService;

        public AuthMvcController()
        {
            authService = new AuthService();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginRequest request)
        {
            try
            {
                var response = authService.Login(request);

                Session["UserId"] = response.Id;
                Session["FullName"] = response.FullName;
                Session["Username"] = response.Username;
                Session["Role"] = response.Role;
                Session["Status"] = response.Status;
                Session["Token"] = response.Token;

                if (response.Role == "Librarian")
                {
                    return RedirectToAction("Librarian", "DashboardMvc");
                }

                if (response.Role == "Member")
                {
                    return RedirectToAction("Member", "DashboardMvc");
                }

                TempData["Error"] = "Role tidak dikenali.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Data register belum valid. Periksa kembali input yang wajib diisi.";
                    return View(request);
                }

                authService.Register(request);

                TempData["Success"] = "Registrasi berhasil. Akun menunggu persetujuan pustakawan.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}