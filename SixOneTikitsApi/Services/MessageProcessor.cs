using System;
using System.Linq;
using Quartz;
using SixOneTikitsApi.AxHelpers;
using SixOneTikitsApi.Models;

namespace SixOneTikitsApi.Services
{
    public class MessageProcessor
    {
        public void Send()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var unsentMsgs = db.Messages.Where(x => (x.Status ==MessageStatus.Pending || x.Status == MessageStatus.Failed) ).ToList();

                    foreach (var msg in unsentMsgs)
                    {
                        MessageHelpers.SendMessage(msg.Id);
                    }
                }
            }
            catch (Exception) { }
        }


    }
    [DisallowConcurrentExecution]
    public class MessageProcessService : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            new MessageProcessor().Send();
        }
    }
}