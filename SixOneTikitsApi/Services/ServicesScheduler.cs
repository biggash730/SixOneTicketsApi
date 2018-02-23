using System;
using Quartz;
using Quartz.Impl;

namespace PianoBarApi.Services
{
    public class ServicesScheduler
    {
        public static void Start()
        {
            var scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            var messageService = JobBuilder.Create<MessageProcessService>().Build();
            var msgTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(2)
                        .RepeatForever())
                    .Build();
            //var today = DateTime.Now;
            //var midNight = new DateTime(today.Year, today.Month, today.Day,23,59,00);

            scheduler.ScheduleJob(messageService, msgTrigger);
        }
    }
}