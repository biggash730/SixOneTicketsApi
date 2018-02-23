using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PianoBarApi.AxHelpers;
using PianoBarApi.DataAccess.Filters;
using PianoBarApi.Extensions;
using PianoBarApi.Models;

namespace PianoBarApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        [HttpPost]
        [Route("getpersonalticketstats")]
        public ResultObj GetPersonalTicketStats(TicketSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var stats = new TicketStats();
                    var query = db.TicketSales.Where(x => x.CreatedBy == user.UserName).Include(x => x.Ticket);
                    query = filter.BuildQuery(query);
                    var query2 = db.CancelledTicketSales.Where(x => x.TicketSale.Contains(user.UserName));
                    var filter2 = new CancelledTicketSaleFilter
                    {
                        DateFrom = filter.DateFrom,
                        DateTo = filter.DateTo
                    };
                    var res = filter2.BuildQuery(query2);
                    if (query.Any())
                    {
                        stats = new TicketStats
                        {
                            TicketsSold = query.Count(),
                            RegularTickets = query.Count(x => x.Ticket.Type == TicketType.Regular),
                            MemberTickets = query.Count(x => x.Ticket.Type == TicketType.Member),
                            CashReceived = query.Sum(x => x.TotalPrice),
                            CancelledTicketSales = res.Count()
                        };
                    }
                    results = WebHelpers.BuildResponse(stats, "Stats Loaded", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        

        [HttpGet]
        [Route("getsummaries")]
        public ResultObj GetDashboardSummaries()
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var date = DateTime.Now;

                    var ticks =
                        db.TicketSales.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month &&
                                                  x.Date.Day == date.Day).ToList();
                    //var saleItems = db.MenuOrders.Where(x => x.Status == OrderStatus.Supplied && x.Cart.Status == CartStatus.Closed );
                    //var cartIds = sales.Select(x => x.CartId).ToList();
                    //var items = db.
                    var ttsales = ticks.Sum(x => x.TotalPrice);
                    
                    var res  = new {
                        TotalSales = ttsales,
                        TotalTicketSales = ticks.Sum(x => x.TotalPrice),
                        TotalTicketsSold = ticks.Count()
                    };
                    results = WebHelpers.BuildResponse(res, "Successful", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
    }
}
