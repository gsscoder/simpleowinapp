using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppFunc = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
                System.Threading.Tasks.Task>; // Done

namespace SimpleOwinApp
{
    // This is a sample Owin (http://owin.org/) App derived from https://github.com/markrendle/Fix/tree/master/Print.
    // Since Owin is a specification that rely on .NET BCL delegates (and standard types), this assembly has no
    // particular dependency.
    // Any Owin compliant-host should (or better MUST) be able to host this application.
    public class WebApp
    {
        // The Export attribute helps Flux (https://github.com/markrendle/Flux) hook the Owin App
        // using Fix (https://github.com/markrendle/Fix).
        [Export("Owin.Application")]
        public Task PrintRequest(IDictionary<string, object> env)
        {
            try
            {
                var scriptName = env.GetPath().ToLower();
                if (!(scriptName.Contains("/info") || scriptName.Contains(".")))
                {
                    return HandlePrintRequest(env);
                }
            }
            catch (Exception ex)
            {
                return TaskHelper.Error(ex);
            }
            return TaskHelper.NotFound();
        }

        private static Task HandlePrintRequest(IDictionary<string, object> env)
        {
            env["owin.ResponseStatusCode"] = 200;
            env["owin.ResponseHeaders"] =
                new Dictionary<string, string[]> { { "Content-Type", new[] { "text/html" } } };
            return ((Stream)env["owin.ResponseBody"]).WriteAsync(BuildHtml(env));
        }

        private static string BuildHtml(IDictionary<string, object> env)
        {
            var builder = new StringBuilder("<html><body>");
            builder.AppendFormat("<p>{0}</p>", ConstructUri(env));
            builder.AppendFormat("<p>{0}</p>", env["owin.RequestMethod"]);
            foreach (var header in env)
            {
                builder.AppendFormat("<p><strong>{0}</strong>: {1}</p>", header.Key, header.Value);
            }
            builder.Append("</body></html>");
            return builder.ToString();
        }

        private static string ConstructUri(IDictionary<string, object> env)
        {
            object serverName;
            if (!env.TryGetValue("host.ServerName", out serverName))
            {
                serverName = "*";
            }
            var builder = new StringBuilder(env["owin.RequestScheme"] + "://" + serverName);
            if (env.ContainsKey("host.ServerPort") && env["host.ServerPort"].ToString() != "80")
            {
                builder.AppendFormat(":{0}", env["host.ServerPort"]);
            }
            if (!string.IsNullOrEmpty(env["owin.RequestPath"].ToString()))
            {
                builder.AppendFormat("{0}", env["owin.RequestPath"]);
            }
            return builder.ToString();
        }
    }
}