using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LibraryManagementSystem.Helpers
{
    public static class AvatarHelper
    {
        public static string GetGravatarUrl(string email, int size = 150)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "default@example.com";
            }

            email = email.Trim().ToLowerInvariant();

            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(email);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                string hash = sb.ToString();
                return $"https://www.gravatar.com/avatar/{hash}?d=mp&s={size}";
            }
        }

        public static string GetUserAvatarUrl(string username, string email, int size = 150)
        {
            if (HttpContext.Current != null && !string.IsNullOrEmpty(username))
            {
                var relativeDir = "~/Uploads/Avatars/";
                var serverDir = HttpContext.Current.Server.MapPath(relativeDir);

                if (System.IO.Directory.Exists(serverDir))
                {
                    string[] extensions = { ".jpg", ".png", ".jpeg", ".gif" };
                    foreach (var ext in extensions)
                    {
                        var filePath = System.IO.Path.Combine(serverDir, username + ext);
                        if (System.IO.File.Exists(filePath))
                        {
                            var fileInfo = new System.IO.FileInfo(filePath);
                            var lastWrite = fileInfo.LastWriteTime.Ticks;
                            return VirtualPathUtility.ToAbsolute(relativeDir + username + ext) + "?v=" + lastWrite;
                        }
                    }
                }
            }

            return GetGravatarUrl(email, size);
        }
    }
}
