using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly LibraryDbContext db = new LibraryDbContext();

        public LoginResponse Login(LoginRequest request)
        {
            if (request == null)
            {
                throw new Exception("Login data cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new Exception("Username is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Password is required");
            }

            var username = request.Username.Trim();
            var password = request.Password.Trim();

            if (!Regex.IsMatch(username, @"^[a-z0-9_.]+$"))
            {
                throw new Exception("Username can only contain lowercase letters, numbers, underscores (_), and periods (.)");
            }

            if (!Regex.IsMatch(password, @"^[a-z0-9]+$"))
            {
                throw new Exception("Password can only contain lowercase letters and numbers");
            }

            var user = db.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception("Invalid username or password");
            }

            var passwordValid = PasswordHelper.VerifyPassword(password, user.Password);

            if (!passwordValid)
            {
                throw new Exception("Invalid username or password");
            }

            if (user.Status == "Pending")
            {
                throw new Exception("Account is not active yet. Pending librarian approval.");
            }

            if (user.Status == "Rejected")
            {
                throw new Exception("Account rejected. Please contact the librarian.");
            }

            if (user.Status == "Inactive")
            {
                throw new Exception("Account is inactive.");
            }

            var token = TokenHelper.GenerateToken();

            user.Token = token;
            user.UpdatedAt = DateTime.Now;

            db.SaveChanges();

            return new LoginResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role,
                Status = user.Status,
                Token = token,
                Message = "Login successful"
            };
        }

        public object Register(RegisterRequest request)
        {
            if (request == null)
            {
                throw new Exception("Registration data cannot be empty");
            }

            ValidateRegisterInput(request);

            var fullName = request.FullName.Trim();
            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLower();

            var address = string.IsNullOrWhiteSpace(request.Address)
                ? null
                : request.Address.Trim();

            var className = string.IsNullOrWhiteSpace(request.ClassName)
                ? null
                : request.ClassName.Trim();

            var usernameExists = db.Users.Any(x => x.Username == username);

            if (usernameExists)
            {
                throw new Exception("Username is already taken");
            }

            var emailExists = db.Users.Any(x => x.Email == email);

            if (emailExists)
            {
                throw new Exception("Email is already taken");
            }

            var user = new User
            {
                FullName = fullName,
                Username = username,
                Email = email,
                Password = PasswordHelper.HashPassword(request.Password.Trim()),
                Role = "Member",
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            db.Users.Add(user);
            db.SaveChanges();

            var member = new Member
            {
                UserId = user.Id,
                MemberCode = GenerateMemberCode(user.Id),
                Address = address,
                ClassName = className,
                ApprovedBy = null,
                ApprovedAt = null
            };

            db.Members.Add(member);
            db.SaveChanges();

            SendRegisterEmail(user);

            return new
            {
                message = "Registration successful. Your account is pending librarian approval.",
                userId = user.Id,
                memberId = member.Id,
                memberCode = member.MemberCode,
                fullName = user.FullName,
                email = user.Email,
                status = user.Status
            };
        }

        public object ResetPasswordToHash(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is required");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("New password is required");
            }

            var cleanUsername = username.Trim();
            var cleanPassword = newPassword.Trim();

            if (!Regex.IsMatch(cleanPassword, @"^[a-z0-9]+$"))
            {
                throw new Exception("Password can only contain lowercase letters and numbers");
            }

            if (cleanPassword.Length < 6 || cleanPassword.Length > 15)
            {
                throw new Exception("Password must be between 6 and 15 characters");
            }

            var user = db.Users.FirstOrDefault(x => x.Username == cleanUsername);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.Password = PasswordHelper.HashPassword(cleanPassword);
            user.UpdatedAt = DateTime.Now;

            db.SaveChanges();

            return new
            {
                message = "Password successfully hashed",
                username = user.Username
            };
        }

        private void ValidateRegisterInput(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new Exception("Full name is required");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new Exception("Username is required");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new Exception("Email is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Password is required");
            }

            var fullName = request.FullName.Trim();
            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLower();
            var password = request.Password.Trim();

            if (fullName.Length < 3 || fullName.Length > 100)
            {
                throw new Exception("Full name must be between 3 and 100 characters");
            }

            if (username.Length < 4 || username.Length > 15)
            {
                throw new Exception("Username must be between 4 and 15 characters");
            }

            if (username.Contains(" "))
            {
                throw new Exception("Username cannot contain spaces");
            }

            if (!Regex.IsMatch(username, @"^[a-z0-9_.]+$"))
            {
                throw new Exception("Username can only contain lowercase letters, numbers, underscores (_), and periods (.)");
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new Exception("Invalid email format");
            }

            if (!email.EndsWith("@gmail.com") &&
                !email.EndsWith("@yahoo.com") &&
                !email.EndsWith("@email.com"))
            {
                throw new Exception("Email can only use @gmail.com, @yahoo.com, or @email.com domains");
            }

            if (password.Length < 6 || password.Length > 15)
            {
                throw new Exception("Password must be between 6 and 15 characters");
            }

            if (!Regex.IsMatch(password, @"^[a-z0-9]+$"))
            {
                throw new Exception("Password can only contain lowercase letters and numbers");
            }

            if (!string.IsNullOrWhiteSpace(request.Address) &&
                request.Address.Trim().Length > 255)
            {
                throw new Exception("Address must be at most 255 characters");
            }

            if (!string.IsNullOrWhiteSpace(request.ClassName) &&
                request.ClassName.Trim().Length > 50)
            {
                throw new Exception("Class name must be at most 50 characters");
            }
        }

        private string GenerateMemberCode(int userId)
        {
            return "MBR" + userId.ToString("D3");
        }

        private void SendRegisterEmail(User user)
        {
            try
            {
                if (user == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return;
                }

                EmailHelper.SendEmail(
                    user.Email,
                    "Moon Books Account Registration",
                    $@"
            <h3>Registration Successful</h3>
            <p>Hello <b>{user.FullName}</b>,</p>

            <p>Thank you for registering at <b>Moon Books</b>.</p>

            <p>Your current account status: <b>Pending</b>.</p>

            <p>
                Please wait for approval from a librarian so your account can be
                used to log in and request book borrowing.
            </p>

            <br/>
            <p>Warm regards,</p>
            <p><b>Moon Books</b></p>
            "
                );
            }
            catch
            {
                // Email gagal tidak membatalkan proses register.
            }
        }
    }
}
