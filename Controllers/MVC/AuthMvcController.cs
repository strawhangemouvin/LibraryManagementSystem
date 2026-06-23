using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Helpers;
using System.Linq;
using System;
using System.Web.Mvc;
using System.Web;

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

                TempData["Error"] = "Unrecognized role.";
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
                    TempData["Error"] = "Registration data is invalid. Please check the required inputs.";
                    return View(request);
                }

                authService.Register(request);

                TempData["Success"] = "Registration successful. Your account is pending librarian approval.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(request);
            }
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["Error"] = "Email is required.";
                    return View();
                }

                email = email.Trim().ToLower();

                // Validate email domain restriction first
                if (!email.EndsWith("@gmail.com") &&
                    !email.EndsWith("@yahoo.com") &&
                    !email.EndsWith("@email.com"))
                {
                    TempData["Error"] = "Email can only use @gmail.com, @yahoo.com, or @email.com domains.";
                    return View();
                }

                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email);
                    if (user == null)
                    {
                        TempData["Error"] = "Email address not found.";
                        return View();
                    }

                    // Generate a 6-digit random code
                    var random = new Random();
                    var code = random.Next(100000, 999999).ToString();

                    // Store code in Token column
                    user.Token = code;
                    user.UpdatedAt = DateTime.Now;
                    db.SaveChanges();

                    // Send Email
                    EmailHelper.SendEmail(
                        email,
                        "Moon Books Password Reset Verification Code",
                        $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 12px; background-color: #fff;'>
                            <div style='text-align: center; margin-bottom: 20px;'>
                                <h2 style='color: #4A2E35; font-family: Georgia, serif;'>Moon Books</h2>
                            </div>
                            <h3 style='color: #4A2E35;'>Password Reset Request</h3>
                            <p>Hello <b>{user.FullName}</b>,</p>
                            <p>We received a request to reset the password for your Moon Books account.</p>
                            <p>Your password recovery verification code is:</p>
                            <h2 style='color: #C97A7E; font-family: monospace; letter-spacing: 4px; text-align: center; padding: 15px; background: #FFF0F2; border-radius: 12px; margin: 20px 0; border: 1px solid rgba(201, 122, 126, 0.2);'>{code}</h2>
                            <p>Please enter this code on the password reset page to set a new password. This code will expire if a new reset request is made or upon login.</p>
                            <br/>
                            <p style='color: #7E6569; font-size: 13px;'>If you did not request this, you can safely ignore this email.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #aaa; text-align: center;'>Moon Books Library System</p>
                        </div>
                        "
                    );

                    TempData["Success"] = "Verification code has been sent to your email.";
                    return RedirectToAction("ResetPassword", new { email = email });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to send code: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string email, string code, string password, string confirmPassword)
        {
            ViewBag.Email = email;

            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || 
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    TempData["Error"] = "All fields are required.";
                    return View();
                }

                email = email.Trim().ToLower();
                code = code.Trim();
                password = password.Trim();
                confirmPassword = confirmPassword.Trim();

                if (password != confirmPassword)
                {
                    TempData["Error"] = "Password and confirm password do not match.";
                    return View();
                }

                // Validate password strength: 6-15 characters, lowercase letters and numbers only
                if (password.Length < 6 || password.Length > 15)
                {
                    TempData["Error"] = "Password must be between 6 and 15 characters.";
                    return View();
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-z0-9]+$"))
                {
                    TempData["Error"] = "Password can only contain lowercase letters and numbers.";
                    return View();
                }

                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email);
                    if (user == null)
                    {
                        TempData["Error"] = "Email address not found.";
                        return View();
                    }

                    if (user.Token != code || string.IsNullOrEmpty(user.Token))
                    {
                        TempData["Error"] = "Invalid verification code.";
                        return View();
                    }

                    // Reset password
                    user.Password = PasswordHelper.HashPassword(password);
                    user.Token = null; // Clear the code after successful reset
                    user.UpdatedAt = DateTime.Now;
                    db.SaveChanges();

                    TempData["Success"] = "Password reset successful! You can now log in with your new password.";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public new ActionResult Profile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var db = new LibraryDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Login");
                }

                ViewBag.User = user;

                if (user.Role == "Member")
                {
                    var member = db.Members.FirstOrDefault(m => m.UserId == userId);
                    ViewBag.Member = member;
                }

                return View();
            }
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var db = new LibraryDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Login");
                }

                ViewBag.User = user;

                if (user.Role == "Member")
                {
                    var member = db.Members.FirstOrDefault(m => m.UserId == userId);
                    ViewBag.Member = member;
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult UpdateProfile(string fullName, string email, string address, string className, HttpPostedFileBase avatarFile)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    TempData["Error"] = "Full Name is required.";
                    return RedirectToAction("EditProfile");
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["Error"] = "Email is required.";
                    return RedirectToAction("EditProfile");
                }

                fullName = fullName.Trim();
                email = email.Trim().ToLower();

                if (fullName.Length < 3 || fullName.Length > 100)
                {
                    TempData["Error"] = "Full Name must be between 3 and 100 characters.";
                    return RedirectToAction("EditProfile");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@(gmail\.com|yahoo\.com|email\.com)$"))
                {
                    TempData["Error"] = "Email format is invalid or has unsupported domain. Use only @gmail.com, @yahoo.com, or @email.com.";
                    return RedirectToAction("EditProfile");
                }

                using (var db = new LibraryDbContext())
                {
                    var emailExists = db.Users.Any(u => u.Email == email && u.Id != userId);
                    if (emailExists)
                    {
                        TempData["Error"] = "Email is already in use by another account.";
                        return RedirectToAction("EditProfile");
                    }

                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        TempData["Error"] = "User not found.";
                        return RedirectToAction("Login");
                    }

                    // Handle Avatar File Upload
                    if (avatarFile != null && avatarFile.ContentLength > 0)
                    {
                        var ext = System.IO.Path.GetExtension(avatarFile.FileName).ToLower();
                        if (ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".gif")
                        {
                            var relativeDir = "~/Uploads/Avatars/";
                            var serverDir = Server.MapPath(relativeDir);
                            if (!System.IO.Directory.Exists(serverDir))
                            {
                                System.IO.Directory.CreateDirectory(serverDir);
                            }

                            // Delete existing avatar files for this user
                            string[] extensions = { ".jpg", ".png", ".jpeg", ".gif" };
                            foreach (var existingExt in extensions)
                            {
                                var existingFile = System.IO.Path.Combine(serverDir, user.Username + existingExt);
                                if (System.IO.File.Exists(existingFile))
                                {
                                    System.IO.File.Delete(existingFile);
                                }
                            }

                            // Save new file
                            var savePath = System.IO.Path.Combine(serverDir, user.Username + ext);
                            avatarFile.SaveAs(savePath);
                        }
                        else
                        {
                            TempData["Error"] = "Avatar must be an image (.jpg, .jpeg, .png, or .gif).";
                            return RedirectToAction("EditProfile");
                        }
                    }

                    user.FullName = fullName;
                    user.Email = email;
                    user.UpdatedAt = DateTime.Now;

                    if (user.Role == "Member")
                    {
                        var member = db.Members.FirstOrDefault(m => m.UserId == userId);
                        if (member != null)
                        {
                            member.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
                            member.ClassName = string.IsNullOrWhiteSpace(className) ? null : className.Trim();
                        }
                    }

                    db.SaveChanges();

                    Session["FullName"] = user.FullName;
                    Session["Email"] = user.Email;

                    TempData["Success"] = "Profile updated successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return RedirectToAction("EditProfile");
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            using (var db = new LibraryDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Login");
                }
                ViewBag.User = user;
                return View();
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmNewPassword))
                {
                    TempData["Error"] = "All password fields are required.";
                    return RedirectToAction("ChangePassword");
                }

                oldPassword = oldPassword.Trim();
                newPassword = newPassword.Trim();
                confirmNewPassword = confirmNewPassword.Trim();

                if (newPassword != confirmNewPassword)
                {
                    TempData["Error"] = "New password and confirmation do not match.";
                    return RedirectToAction("ChangePassword");
                }

                if (newPassword.Length < 6 || newPassword.Length > 15)
                {
                    TempData["Error"] = "New password must be between 6 and 15 characters.";
                    return RedirectToAction("ChangePassword");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^[a-z0-9]+$"))
                {
                    TempData["Error"] = "New password can only contain lowercase letters and numbers.";
                    return RedirectToAction("ChangePassword");
                }

                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        TempData["Error"] = "User not found.";
                        return RedirectToAction("Login");
                    }

                    if (!PasswordHelper.VerifyPassword(oldPassword, user.Password))
                    {
                        TempData["Error"] = "Current password is incorrect.";
                        return RedirectToAction("ChangePassword");
                    }

                    user.Password = PasswordHelper.HashPassword(newPassword);
                    user.UpdatedAt = DateTime.Now;
                    db.SaveChanges();

                    TempData["Success"] = "Password changed successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return RedirectToAction("ChangePassword");
            }

            return RedirectToAction("Profile");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}