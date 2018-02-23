using System;
using System.Linq;
using System.Net;
using System.Text;
using PianoBarApi.Models;
using RestSharp;

namespace PianoBarApi.AxHelpers
{
    public class MessageHelpers
    {
        private static readonly MessageService MsgServiceSettings = SetupConfig.Setting.MessageService;
        public static void SendMessage(long msgId)
        {
            if (!MsgServiceSettings.IsActive) return;
            using (var db = new AppDbContext())
            {
                var eoe = db.Messages.First(x => x.Id == msgId && (x.Status== MessageStatus.Pending || x.Status == MessageStatus.Failed));

                var client = new RestClient(MsgServiceSettings.BaseUrl);
                var request = new RestRequest(MsgServiceSettings.SendMessageUrl, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader(HttpRequestHeader.Authorization.ToString(),
                    $"Bearer {MsgServiceSettings.ApiKey}");
                request.AddParameter("Type", MessageType.SMS.ToString());
                request.AddParameter("SenderId", MsgServiceSettings.SenderId);
                request.AddParameter("Subject", eoe.Subject);
                request.AddParameter("Message", eoe.Text);
                request.AddParameter("Recipients", eoe.Recipient);
                var res = client.Execute(request);
                eoe.Response = res.ResponseStatus + " @" + DateTime.Now;
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    eoe.Status = MessageStatus.Failed;
                    return;
                }
                eoe.Status = MessageStatus.Sent;
                db.SaveChanges();
            }
        }

        public static string GenerateRandomString(int length)
        {
            var stringBuilder = new StringBuilder(length);
            var chArray = "abcdefghijklmnopqrstuvwxyz0123456789_-".ToCharArray();
            var random = new Random((int)DateTime.Now.Ticks);
            for (var index = 0; index < length; ++index)
                stringBuilder.Append(chArray[random.Next(chArray.Length)]);
            return stringBuilder.ToString().ToLower();
        }
        public static string GenerateRandomNumber(int length)
        {
            var stringBuilder = new StringBuilder(length);
            var chArray = "0123456789".ToCharArray();
            var random = new Random((int)DateTime.Now.Ticks);
            for (var index = 0; index < length; ++index)
                stringBuilder.Append(chArray[random.Next(chArray.Length)]);
            return stringBuilder.ToString();
        }
    }
}