using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace LibraryManagementSystem.Helpers
{
    public class TokenHelper
    {
        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}