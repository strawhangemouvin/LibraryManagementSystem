using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class AuthService : IAuthService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public LoginResponse Login(LoginRequest request)
        {
            if (request == null)
            {
                throw new Exception("Data login tidak boleh kosong");
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Username dan password wajib diisi");
            }

            var username = request.Username.Trim();
            var password = request.Password.Trim();

            var user = db.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception("Username atau password salah");
            }

            if (!PasswordHelper.VerifyPassword(password, user.Password))
            {
                throw new Exception("Username atau password salah");
            }

            if (user.Status == "Pending")
            {
                throw new Exception("Akun belum aktif. Menunggu persetujuan pustakawan.");
            }

            if (user.Status == "Rejected")
            {
                throw new Exception("Akun ditolak. Silakan hubungi pustakawan.");
            }

            if (user.Status == "Inactive")
            {
                throw new Exception("Akun tidak aktif.");
            }

            var token = TokenHelper.GenerateToken();

            user.Token = token;
            user.UpdatedAt = DateTime.Now;

            db.SaveChanges();

            var response = new LoginResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role,
                Status = user.Status,
                Token = token,
                Message = "Login berhasil"
            };

            return response;
        }

        public object Register(RegisterRequest request)
        {
            if (request == null)
            {
                throw new Exception("Data register tidak boleh kosong");
            }

            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("FullName, Username, Email, dan Password wajib diisi");
            }

            var username = request.Username.Trim();
            var email = request.Email.Trim();

            var usernameExists = db.Users.Any(x => x.Username == username);

            if (usernameExists)
            {
                throw new Exception("Username sudah digunakan");
            }

            var emailExists = db.Users.Any(x => x.Email == email);

            if (emailExists)
            {
                throw new Exception("Email sudah digunakan");
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Username = username,
                Email = email,
                Password = PasswordHelper.HashPassword(request.Password),
                Role = "Member",
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            db.Users.Add(user);
            db.SaveChanges();

            var memberCode = "MBR" + user.Id.ToString("D3");

            var member = new Member
            {
                UserId = user.Id,
                MemberCode = memberCode,
                Phone = request.Phone,
                Address = request.Address,
                ClassName = request.ClassName,
                ApprovedBy = null,
                ApprovedAt = null
            };

            db.Members.Add(member);
            db.SaveChanges();

            return new
            {
                message = "Register berhasil. Akun menunggu persetujuan pustakawan.",
                userId = user.Id,
                memberCode = member.MemberCode,
                status = user.Status
            };
        }

        public object ResetPasswordToHash(string username, string newPassword)
        {
            throw new NotImplementedException();
        }
        //public object ResetPasswordToHash(string username, string newPassword)
        //{
        //    if (string.IsNullOrWhiteSpace(username))
        //    {
        //        throw new Exception("Username wajib diisi");
        //    }

        //    if (string.IsNullOrWhiteSpace(newPassword))
        //    {
        //        throw new Exception("Password baru wajib diisi");
        //    }

        //    var user = db.Users.FirstOrDefault(x => x.Username == username);

        //    if (user == null)
        //    {
        //        throw new Exception("User tidak ditemukan");
        //    }

        //    user.Password = PasswordHelper.HashPassword(newPassword);
        //    user.UpdatedAt = DateTime.Now;

        //    db.SaveChanges();

        //    return new
        //    {
        //        message = "Password berhasil di-hash",
        //        username = user.Username
        //    };
        //}
    }
}