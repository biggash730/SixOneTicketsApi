using System;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using SixOneTikitsApi.AxHelpers;
using SixOneTikitsApi.DataAccess.Filters;
using SixOneTikitsApi.Extensions;
using SixOneTikitsApi.Models;

namespace SixOneTikitsApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/cancelledticketsales")]
    public class CancelledTicketSalesController : BaseApi<CancelledTicketSale>
    {
        public override ResultObj Post(CancelledTicketSale record)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;
                
                results = WebHelpers.BuildResponse(null, "Not Implemented", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        public override ResultObj Put(CancelledTicketSale record)
        {
            ResultObj results;
            try
            {
                //var user = User.Identity.AsAppUser().Result;

                results = WebHelpers.BuildResponse(null, "Not Implemented", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        public override ResultObj Delete(long id)
        {
            ResultObj results;
            try
            {
                results = WebHelpers.BuildResponse(null, "Not Implemented", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        public override ResultObj Get(long id)
        {
            ResultObj results;
            try
            {
                var data = Repository.Get(new CancelledTicketSaleFilter { Id = id }).ToList().Select(x => new
                {
                    x.Id,
                    x.Date,
                    x.Notes,
                    Sale = JsonConvert.DeserializeObject<TicketSale>(x.TicketSale),
                    x.CreatedBy,
                    x.ModifiedBy,
                    x.CreatedAt,
                    x.ModifiedAt

                }).First();
                results = WebHelpers.BuildResponse(data, "Record Loaded Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        [HttpGet]
        [Route("reverse")]
        public ResultObj Reverse(long id)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var ent = db.CancelledTicketSales.First(x => x.Id == id);
                    var sale = JsonConvert.DeserializeObject<TicketSale>(ent.TicketSale);

                    var ns = new TicketSale
                    {
                        Date = sale.Date,
                        Discount = sale.Discount,
                        TicketId = sale.TicketId,
                        RefNumber = sale.RefNumber,
                        Quantity = sale.Quantity,
                        TotalPrice = sale.TotalPrice,
                        CustomerNumber = sale.CustomerNumber,
                        CreatedAt = sale.CreatedAt,
                        ModifiedAt = sale.ModifiedAt,
                        CreatedBy = sale.CreatedBy,
                        ModifiedBy = sale.ModifiedBy
                    };
                    db.TicketSales.Add(ns);
                    db.CancelledTicketSales.Remove(ent);
                    db.SaveChanges();
                    results = WebHelpers.BuildResponse(id, "Successful", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("query")]
        public ResultObj Query(CancelledTicketSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var data = Repository.Get(filter).ToList().OrderByDescending(x=> x.Date).Select(x => new
                {
                    x.Id,
                    x.Date,
                    x.Notes,
                    Sale = JsonConvert.DeserializeObject<TicketSale>(x.TicketSale),
                    x.CreatedBy,
                    x.ModifiedBy,
                    x.CreatedAt,
                    x.ModifiedAt
                }).ToList();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        
    }
}