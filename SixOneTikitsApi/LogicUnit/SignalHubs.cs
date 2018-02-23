using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PianoBarApi.AxHelpers;

namespace PianoBarApi.LogicUnit
{
    [HubName("processhub")]
    public class ProcessHub : Hub { }

    public class SignalEvents
    {
        public static readonly string NewOrder = "NewOrder";
        public static readonly string ProcessingOrder = "ProcessingOrder";
        public static readonly string FulfilledOrder = "FulfilledOrder";
    }

    public class SignalNotice
    {
        public string SignalEvent { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
    }

    public class SignalHelpers
    {
        public static void OnSignalEvent(SignalEvent evnt, string msg, object data)
        {
            var processHub = GlobalHost.ConnectionManager.GetHubContext<ProcessHub>();
            processHub.Clients.All.notify(new SignalNotice
            {
                SignalEvent = evnt,
                Success = true,
                Message = msg,
                Data = data
            });
        }
    }
}