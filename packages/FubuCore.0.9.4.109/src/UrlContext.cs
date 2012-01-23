using System;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace FubuCore
{
    public static class UrlContext
    {
        static UrlContext()
        {
            Reset();
        }

        private static Func<string, string, string> _combine { get; set; }
        private static Func<string, string> _toAbsolute { get; set; }
        private static Func<string, bool> _isAbsolute { get; set; }
        private static Func<string, string> _mapPath { get; set; }

        public static void Reset()
        {
            if (HttpRuntime.AppDomainAppVirtualPath != null)
            {
                Live();
                return;
            }

            Stub("");
        }

        public static void Stub()
        {
            Stub("");
        }

        public static void Stub(string usingFakeUrl)
        {
            _combine = (basePath, subPath) =>
            {
                var root = basePath.TrimEnd('/');
                if (root.Length > 0) root += '/';
                return root + subPath.TrimStart('/');
            };
            _isAbsolute = path => path.StartsWith("/");
            _toAbsolute = path => _isAbsolute(path) ? path : _combine(usingFakeUrl, path.Replace("~", ""));
            _mapPath = virtPath => _toAbsolute(virtPath).Replace("~", "").Replace("//", "/").Replace("/", "\\");
        }

        public static void Live()
        {
            _combine = VirtualPathUtility.Combine;
            _toAbsolute = VirtualPathUtility.ToAbsolute;
            _isAbsolute = VirtualPathUtility.IsAbsolute;
            _mapPath = HostingEnvironment.MapPath;
        }

        public static string ToAbsoluteUrl(this string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) return url;
            if (url.IsEmpty()) url = "~/";
            // When running on WebDev.WebServer (and possibly IIS 6) VirtualPathUtility chokes when the url contains a querystring
            var urlParts = url.Split(new[] { '?' }, 2);
            var baseUrl = urlParts[0];
            if (!_isAbsolute(baseUrl))
            {
                baseUrl = _combine("~", baseUrl);
            }

            var absoluteUrl = _toAbsolute(baseUrl);
            if (urlParts.Length > 1) absoluteUrl += ("?" + urlParts[1]);
            return absoluteUrl;
        }



        public static string ToPhysicalPath(this string webRelativePath)
        {
            if (!_isAbsolute(webRelativePath))
            {
                webRelativePath = _combine("~", webRelativePath);
            }
            return _mapPath(webRelativePath);
        }

        public static string WithQueryStringValues(this string querystring, params object[] values)
        {
            return querystring.ToFormat(values.Select(value => value.ToString().UrlEncoded()).ToArray());
        }

    }

    public static class UrlStringExtensions
    {
        public static string ToAbsoluteUrl(this string url, string applicationUrl)
        {
            if (!Uri.IsWellFormedUriString(applicationUrl, UriKind.Absolute))
            {
                throw new ArgumentOutOfRangeException("applicationUrl", "applicationUrl must be an absolute url");
            }
			
			url = url ?? string.Empty;
            url = url.TrimStart('~', '/').TrimStart('/');
			
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) return url;			
			
            return (applicationUrl.TrimEnd('/') + "/" + url).TrimEnd('/');
        }

        public static string ToServerQualifiedUrl(this string relativeUrl, string serverBasedUrl)
        {
            var baseUri = new Uri(serverBasedUrl);
            return new Uri(baseUri, relativeUrl.ToAbsoluteUrl(serverBasedUrl)).ToString();
        }

        public static string UrlEncoded(this object target)
        {
            //properly encoding URI: http://blogs.msdn.com/yangxind/default.aspx
            return target != null ? Uri.EscapeDataString(target.ToString()) : string.Empty;
        }

        public static string WithQueryStringValues(this string querystring, params object[] values)
        {
            return querystring.ToFormat(values.Select(value => value.ToString().UrlEncoded()).ToArray());
        }

        public static string WithoutQueryString(this string querystring)
        {
            var questionMarkIndex = querystring.IndexOf('?');

            if (questionMarkIndex == -1) return querystring;

            return querystring.Substring(0, questionMarkIndex);
        }

    
    }

}