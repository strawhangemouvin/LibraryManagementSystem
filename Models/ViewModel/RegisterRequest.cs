using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap minimal 3 karakter dan maksimal 100 karakter")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username wajib diisi")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Username minimal 4 karakter dan maksimal 15 karakter")]
        [RegularExpression(@"^[a-z0-9_.]+$", ErrorMessage = "Username hanya boleh huruf kecil, angka, underscore (_), dan titik (.)")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [RegularExpression(@"^[^@\s]+@(gmail\.com|yahoo\.com|email\.com)$", ErrorMessage = "Email hanya boleh menggunakan domain @gmail.com, @yahoo.com, atau @email.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password minimal 6 karakter dan maksimal 15 karakter")]
        [RegularExpression(@"^[a-z0-9]+$", ErrorMessage = "Password hanya boleh huruf kecil dan angka")]
        public string Password { get; set; }

        [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "Kelas maksimal 50 karakter")]
        public string ClassName { get; set; }
    }
}