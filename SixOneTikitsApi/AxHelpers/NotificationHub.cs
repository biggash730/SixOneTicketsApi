using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PianoBarApi.LogicUnit;

namespace PianoBarApi.AxHelpers
{
    [HubName("notificationhub")]
    public class NotificationHub : Hub { }

    public class SignalNotice
    {
        public string SignalEvent { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
    }

    public enum SignalEvent
    {
        FoodOrderIn,
        FoodOrderProcessing,
        FoodOrderReady,
        FoodOrderRejected,
        FoodOrderCancelled,
        FoodOrderSupplied,
        DrinkOrderIn,
        DrinkOrderProcessing,
        DrinkOrderReady,
        DrinkOrderRejected,
        DrinkOrderCancelled,
        DrinkOrderSupplied,
        General
    }

    public class NotificationHelpers
    {
        public static void OnSignalEvent(SignalEvent evnt, string msg, object data)
        {
            var processHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            processHub.Clients.All.notify(new SignalNotice
            {
                SignalEvent = evnt.ToString(),
                Success = true,
                Message = msg,
                Data = data
            });
        }
    }
}