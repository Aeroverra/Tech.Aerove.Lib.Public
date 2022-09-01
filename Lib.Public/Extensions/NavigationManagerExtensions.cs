using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPublic.Extensions
{
    public static class NavigationManagerExtensions
    {
        public static string GetRoute(this NavigationManager nav)
        {
            return $"{nav.Uri}".Replace(nav.BaseUri, "");
        }
    }
}
