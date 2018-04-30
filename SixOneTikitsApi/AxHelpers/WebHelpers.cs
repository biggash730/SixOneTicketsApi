using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using RestSharp;

namespace SixOneTikitsApi.AxHelpers
{
    public class WebHelpers
    {
        /// <summary>
        /// Builds the results object.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        public static ResultObj BuildResponse(object data, string msg, bool success, int total)
        {
            if (string.IsNullOrEmpty(msg)) msg = $"{total} record(s) found.";
            var results = new ResultObj
            {
                Data = data,
                Message = msg,
                Success = success,
                Total = total
            };

            return results;
        }

        /// <summary>
        /// Builds the erroe message
        /// </summary>
        /// <param name="exception">The ex.</param>
        /// <returns></returns>
        private static string ErrorMsg(Exception exception)
        {
            var validationException = exception as DbEntityValidationException;
            if (validationException != null)
            {
                var lines = validationException.EntityValidationErrors.Select(
                    x => new
                    {
                        name = x.Entry.Entity.GetType().Name.Split('_')[0],
                        errors = x.ValidationErrors.Select(y => y.PropertyName + ":" + y.ErrorMessage)
                    })
                                               .Select(x => $"{x.name} => {string.Join(",", x.errors)}");
                return string.Join("\r\n", lines);
            }

            var updateException = exception as DbUpdateException;
            if (updateException != null)
            {
                Exception innerException = updateException;
                while (innerException.InnerException != null) innerException = innerException.InnerException;
                if (innerException != updateException)
                {
                    if (innerException is SqlException)
                    {
                        var result = ProcessSqlExceptionMessage(innerException.Message);
                        if (!string.IsNullOrEmpty(result)) return result;
                    }
                }
                var entities = updateException.Entries.Select(x => x.Entity.GetType().Name.Split('_')[0])
                                              .Distinct()
                                              .Aggregate((a, b) => a + ", " + b);
                return ($"{innerException.Message} => {entities}");
            }

            var msg = exception.Message;
            if (exception.InnerException == null) return msg;
            msg = exception.InnerException.Message;

            if (exception.InnerException.InnerException == null) return msg;
            msg = exception.InnerException.InnerException.Message;

            if (exception.InnerException.InnerException.InnerException != null)
            {
                msg = exception.InnerException.InnerException.InnerException.Message;
            }

            return msg;
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="exception">The ex.</param>
        /// <returns></returns>
        public static ResultObj ProcessException(Exception exception)
        {
            var msg = ErrorMsg(exception);
            return BuildResponse(null, msg, false, 0);
        }

        /// <summary>
        /// Processes the SQL exception message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string ProcessSqlExceptionMessage(string message)
        {

            if (message.Contains("unique index"))
                return "Operation failed. Data is constrained to be unique.";
            return message.Contains("The DELETE statement conflicted with the REFERENCE constraint") ?
                "This record is referenced by other records hence can not be deleted."
                : message;
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="values">The ASP.NET MVC model state values.</param>
        /// <returns></returns>
        public static ResultObj ProcessException(ICollection<ModelState> values)
        {
            var msg = values.SelectMany(modelState => modelState.Errors)
                .Aggregate("", (current, error) => current + error.ErrorMessage + "\n");
            return BuildResponse(null, msg, false, 0);
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="identityResult">The identity result.</param>
        /// <returns></returns>
        public static ResultObj ProcessException(IdentityResult identityResult)
        {
            var msg = identityResult.Errors.Aggregate("", (current, error) => current + error + "\n");
            return BuildResponse(null, msg, false, 0);
        }


        public static Domain GetSubdomain()
        {
            var url = HttpContext.Current.Request.Url.Host;
            var subdomain = url.Contains(".") ? url.Split('.').FirstOrDefault() : "demo";
            string[] blacklist = { "www", "axoncubes"};
            if (blacklist.Contains(subdomain)) subdomain = "demo"; 

            return new Domain { Subdomain = subdomain, Url = url };
        }

        private const string ClientId = "23f92166c02a450";

        public static string UploadImageToImgUr(string base64Data)
        {
            base64Data = base64Data.Replace("data:image/png;base64,", "");
            base64Data = base64Data.Replace("data:image/jpeg;base64,", "");
            base64Data = base64Data.Replace("data:image/jpg;base64,", "");
            base64Data = base64Data.Replace("data:image/gif;base64,", "");
            var auth = "Client-ID " + ClientId;

            var client = new RestClient("https://api.imgur.com/3/image");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", auth);
            request.AddHeader("Accept", "application/json");

            request.AddParameter("image", base64Data);
            request.AddParameter("type", "base64");

            var response = client.Execute(request);
            if (response.ResponseStatus != ResponseStatus.Completed) throw new Exception("Could not upload image to imgur. Try again!!");

            var res = JsonConvert.DeserializeObject<ImgUrResultObj>
                (response.Content);
            if (!res.success) throw new Exception("Could not upload image to imgur. Try again!!");

            return res.data.link;
        }

    }

    public class Domain
    {
        public string Url { get; set; }
        public string Subdomain { get; set; }
    }

    public class ResultObj
    {
        public long Total { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class ImgUrResultObj
    {
        public ImgUrResultData data { get; set; }
        public string status { get; set; }
        public bool success { get; set; }
    }

    public class ImgUrResultData
    {
        public string id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string link { get; set; }
    }
}