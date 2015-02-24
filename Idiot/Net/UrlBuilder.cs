using System;
using Microsoft.SPOT;

namespace Idiot.Net
{
    internal static class UrlBuilder
    {
        internal static string Join(params string[] args)
        {
            string url = "";
            foreach (string argument in args)
            {
                url += "/" + argument;
            }
            return url;
        }

        internal static string[] Extract(string url)
        {
            string[] args = url.Split('/');
            return args;
        }
    }
}
