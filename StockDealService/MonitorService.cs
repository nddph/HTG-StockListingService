using LogBusinessSharing.LogBusiness;
using LogBusinessSharing.LogCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService
{
    public class MonitorServiceOptions
    {
        public string Path { get; set; } = "/token"; //line 1

        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(12); //line 2

        public SigningCredentials SigningCredentials { get; set; }//line 3

    }

    public class LogContent
    {
        public string DeviceId { get; set; }

        public object Body { get; set; }

        public object Param { get; set; }
    }

    public class MonitorService
    {

        private readonly RequestDelegate _next;

        private readonly MonitorServiceOptions _options;

        public MonitorService(
            RequestDelegate next,
            IOptions<MonitorServiceOptions> options)
        {
            _next = next;
            _options = options.Value;


        }

        public async Task Invoke(HttpContext context)
        {
            var userId = GetUserId(context);

            await LogRequest(context.Request, userId);

            await _next.Invoke(context);// If log response then  remove this code.

            //await LogResponseAndInvokeNext(context);
        }

        private async Task LogRequest(HttpRequest request, Guid? userId)
        {
            var content = new LogContent()
            {
                Param = request.QueryString,
            };


            request.Headers.TryGetValue("deviceId", out StringValues header);

            if (header.Count == 0)
            {
                request.Headers.TryGetValue("DeviceId", out header);
            }

            content.DeviceId = header.Count != 0 ? header : "";

            //kiểm tra xem API này có cần log body hay không
            if (request.ContentType == "application/json")
            {
                using var bodyReader = new StreamReader(request.Body);

                var body = await bodyReader.ReadToEndAsync();
                request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
                //kiểm tra xem API có cần remove password
                content.Body = ClearPasswordRequest(body);
            }
            await LogManagerBusiness.LogDBAsync(LogActionType.Info,
                                                request.Path,
                                                request.Path,
                                                content,
                                                userId);
        }

        private string ClearPasswordRequest(string json)
        {
            JObject jObject = JObject.Parse(json);

            if (jObject.ContainsKey("Password"))
            {
                jObject["Password"] = "";
            }

            if (jObject.ContainsKey("password"))
            {
                jObject["password"] = "";
            }

            if (jObject.ContainsKey("OldPassword"))
            {
                jObject["OldPassword"] = "";
            }

            if (jObject.ContainsKey("oldPassword"))
            {
                jObject["oldPassword"] = "";
            }

            if (jObject.ContainsKey("ConfirmPassword"))
            {
                jObject["ConfirmPassword"] = "";
            }

            if (jObject.ContainsKey("confirmPassword"))
            {
                jObject["confirmPassword"] = "";
            }

            return Convert.ToString(jObject);
        }

        Guid? GetUserId(HttpContext context)
        {
            try
            {
                if (!context.Request.Headers.ContainsKey("Authorization")) return null;

                context.Request.Headers.TryGetValue("Authorization", out StringValues token);
                var handler = new JwtSecurityTokenHandler();
                var strToken = token.ToString().Replace("Bearer ", "");
                var tokenInfo = handler.ReadJwtToken(strToken);
                if (tokenInfo != null)
                {
                    var UserId = tokenInfo.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                    if (string.IsNullOrEmpty(UserId)) return null;
                    return Guid.Parse(UserId);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task LogResponseAndInvokeNext(HttpContext context)
        {
            using (var buffer = new MemoryStream())
            {
                //replace the context response with our buffer
                var stream = context.Response.Body;
                context.Response.Body = buffer;

                //invoke the rest of the pipeline
                await _next.Invoke(context);

                //reset the buffer and read out the contents
                buffer.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(buffer);
                using (var bufferReader = new StreamReader(buffer))
                {
                    string body = await bufferReader.ReadToEndAsync();

                    //reset to start of stream
                    buffer.Seek(0, SeekOrigin.Begin);

                    //copy our content to the original stream and put it back
                    await buffer.CopyToAsync(stream);
                    context.Response.Body = stream;

                    System.Diagnostics.Debug.Print($"Response: {body}");

                }
            }
        }

    }

}
