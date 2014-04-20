using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Azyotter.Models
{
    public static class StatusProcessor
    {
        public static string GetOriginalProfileImage(Uri uri)
        {
            return Regex.Replace(uri.ToString(), @"_normal(\.[a-zA-Z0-9]+)?$", "$1");
        }
    }
}
