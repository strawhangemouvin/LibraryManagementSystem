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
                throw new Exception("Data login tidak boleh kosong");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new Exception("Username wajib diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Password wajib diisi");
            }

            var username = request.Username.Trim();
            var password = request.Password.Trim();

            if (!Regex.IsMatch(username, @"^[a-z0-9_.]+$"))
            {
                throw new Exception("Username hanya boleh huruf kecil, angka, underscore (_), dan titik (.)");
            }

            if (!Regex.IsMatch(password, @"^[a-z0-9]+$"))
            {
                throw new Exception("Password hanya boleh huruf kecil dan angka");
            }

            var user = db.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception("Username atau password salah");
            }

            var passwordValid = PasswordHelper.VerifyPassword(password, user.Password);

            if (!passwordValid)
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

            return new LoginResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role,
                Status = user.Status,
                Token = token,
                Message = "Login berhasil"
            };
        }

        public object Register(RegisterRequest request)
        {
            if (request == null)
            {
                throw new Exception("Data register tidak boleh kosong");
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
                throw new Exception("Username sudah digunakan");
            }

            var emailExists = db.Users.Any(x => x.Email == email);

            if (emailExists)
            {
                throw new Exception("Email sudah digunakan");
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
                message = "Register berhasil. Akun menunggu persetujuan pustakawan.",
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
                throw new Exception("Username wajib diisi");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("Password baru wajib diisi");
            }

            var cleanUsername = username.Trim();
            var cleanPassword = newPassword.Trim();

            if (!Regex.IsMatch(cleanPassword, @"^[a-z0-9]+$"))
            {
                throw new Exception("Password hanya boleh huruf kecil dan angka");
            }

            if (cleanPassword.Length < 6 || cleanPassword.Length > 15)
            {
                throw new Exception("Password minimal 6 karakter dan maksimal 15 karakter");
            }

            var user = db.Users.FirstOrDefault(x => x.Username == cleanUsername);

            if (user == null)
            {
                throw new Exception("User tidak ditemukan");
            }

            user.Password = PasswordHelper.HashPassword(cleanPassword);
            user.UpdatedAt = DateTime.Now;

            db.SaveChanges();

            return new
            {
                message = "Password berhasil di-hash",
                username = user.Username
            };
        }

        private void ValidateRegisterInput(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new Exception("Nama lengkap wajib diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new Exception("Username wajib diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new Exception("Email wajib diisi");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new Exception("Password wajib diisi");
            }

            var fullName = request.FullName.Trim();
            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLower();
            var password = request.Password.Trim();

            if (fullName.Length < 3 || fullName.Length > 100)
            {
                throw new Exception("Nama lengkap minimal 3 karakter dan maksimal 100 karakter");
            }

            if (username.Length < 4 || username.Length > 15)
            {
                throw new Exception("Username minimal 4 karakter dan maksimal 15 karakter");
            }

            if (username.Contains(" "))
            {
                throw new Exception("Username tidak boleh mengandung spasi");
            }

            if (!Regex.IsMatch(username, @"^[a-z0-9_.]+$"))
            {
                throw new Exception("Username hanya boleh huruf kecil, angka, underscore (_), dan titik (.)");
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new Exception("Format email tidak valid");
            }

            if (!email.EndsWith("@gmail.com") &&
                !email.EndsWith("@yahoo.com") &&
                !email.EndsWith("@email.com"))
            {
                throw new Exception("Email hanya boleh menggunakan domain @gmail.com, @yahoo.com, atau @email.com");
            }

            if (password.Length < 6 || password.Length > 15)
            {
                throw new Exception("Password minimal 6 karakter dan maksimal 15 karakter");
            }

            if (!Regex.IsMatch(password, @"^[a-z0-9]+$"))
            {
                throw new Exception("Password hanya boleh huruf kecil dan angka");
            }

            if (!string.IsNullOrWhiteSpace(request.Address) &&
                request.Address.Trim().Length > 255)
            {
                throw new Exception("Alamat maksimal 255 karakter");
            }

            if (!string.IsNullOrWhiteSpace(request.ClassName) &&
                request.ClassName.Trim().Length > 50)
            {
                throw new Exception("Kelas maksimal 50 karakter");
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
                    "Registrasi Akun Moon Books",
                    $@"
            <h3>Registrasi Berhasil</h3>
            <p>Halo <b>{user.FullName}</b>,</p>

            <p>Terima kasih telah melakukan registrasi pada <b>Moon Books</b>.</p>

            <p>Status akun Anda saat ini: <b>Pending</b>.</p>

            <p>
                Silakan menunggu persetujuan dari pustakawan agar akun Anda dapat
                digunakan untuk login dan mengajukan peminjaman buku.
            </p>

            <br/>
            <p>Salam hangat,</p>
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
