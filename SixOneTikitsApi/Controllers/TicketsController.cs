using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using SixOneTikitsApi.AxHelpers;
using SixOneTikitsApi.DataAccess.Filters;
using SixOneTikitsApi.DataAccess.Repositories;
using SixOneTikitsApi.Extensions;
using SixOneTikitsApi.Models;

namespace SixOneTikitsApi.Controllers
{
    [Authorize]
    public class TicketsController : BaseApi<Ticket>
    {
        public override ResultObj Post(Ticket record)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                //upload to imgur and return link
                //record.ImageLink = record.Image == null ? record.ImageLink : WebHelpers.UploadImageToImgUr(record.Image);
                record.CreatedAt = DateTime.Now;
                record.ModifiedAt = DateTime.Now;
                record.Status = TicketStatus.Pending;
                Repository.Insert(SetAudit(record, true));
                results = WebHelpers.BuildResponse(record.Id, "Saved Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        public override ResultObj Put(Ticket record)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                //record.ImageLink = record.Image == null ? record.ImageLink : WebHelpers.UploadImageToImgUr(record.Image);
                record.ModifiedAt = DateTime.Now;
                record.ModifiedBy = user.UserName;
                Repository.Update(SetAudit(record));
                results = WebHelpers.BuildResponse(record.Id, "Updated Successfully.", true, 1);
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
                var data = Repository.Get(new TicketFilter { Id = id }).ToList().Select(x => new
                {
                    x.Id,
                    x.Title,
                    Type = x.Type.ToString(),
                    x.Description,
                    x.Price,
                    Status = x.Status.ToString(),
                    //x.ImageLink,
                    x.RefPrefix,
                    x.CreatedBy,
                    x.ModifiedBy,
                    x.CreatedAt,
                    x.ModifiedAt,
                    x.Admission

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
        [Route("api/tickets/activate")]
        public ResultObj ActivateTicket(long id)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var tick = db.Tickets.First(x => x.Id == id);
                    tick.ModifiedAt = DateTime.Now;
                    tick.ModifiedBy = user.UserName;
                    tick.Status = TicketStatus.Active;
                    db.SaveChanges();
                    results = WebHelpers.BuildResponse(id, "Ticket  Activated Successfully.", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("api/tickets/cancel")]
        public ResultObj CancelTicket(CancelModel obj)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var tick = db.Tickets.First(x => x.Id == obj.Id);
                    tick.ModifiedAt = DateTime.Now;
                    tick.ModifiedBy = user.UserName;
                    tick.Status = TicketStatus.Cancelled;
                    tick.Reason = obj.Note;
                    db.SaveChanges();
                    results = WebHelpers.BuildResponse(obj.Id, "Ticket has been cancelled.", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("api/tickets/query")]
        public ResultObj Query(TicketFilter filter)
        {
            ResultObj results;
            try
            {
                var data = Repository.Get(filter).OrderBy(x => x.Title).ToList().Select(x => new
                {
                    x.Id,
                    x.Title,
                    Type = x.Type.ToString(),
                    x.Description,
                    x.Price,
                    Status = x.Status.ToString(),
                    //x.ImageLink,
                    x.RefPrefix,
                    x.CreatedBy,
                    x.ModifiedBy,
                    x.CreatedAt,
                    x.ModifiedAt,
                    x.Admission
                }).ToList();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpGet]
        [Route("api/tickets/getactiveticketsbytype")]
        public ResultObj GetActiveTicketsByType(string type)
        {
            ResultObj results;
            try
            {
                var data = Repository.Get().Where(x=> x.Status == TicketStatus.Active && x.Type.ToString().ToLower() == type.ToLower()).OrderBy(x=> x.Type).ThenBy(x => x.Title).ToList().Select(x => new
                {
                    x.Id,
                    x.Title,
                    Type = x.Type.ToString(),
                    x.Description,
                    x.Price,
                    Status = x.Status.ToString(),
                    //x.ImageLink,
                    x.RefPrefix,
                    x.Admission
                }).ToList();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("api/tickets/sellticket")]
        public ResultObj SellTicket(TicketSale record)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var tick = db.Tickets.First(x => x.Id == record.TicketId);
                    record.CreatedAt = DateTime.Now;
                    record.ModifiedAt = DateTime.Now;
                    record.CreatedBy = user.UserName;
                    record.ModifiedBy = user.UserName;
                    record.RefNumber = tick.RefPrefix + MessageHelpers.GenerateRandomNumber(5);
                    db.TicketSales.Add(record);
                    db.SaveChanges();

                    //generate reciept
                    //var res = new TicketSaleRepository().GenerateReceipt(record.Id);


                    results = WebHelpers.BuildResponse(record, "Successful.", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        [HttpPost]
        [Route("api/tickets/getticketsales")]
        public ResultObj GetTicketSales(TicketSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var query = db.TicketSales.Where(x => x.Id > 0).Include(x => x.Ticket);
                    var recs = filter.BuildQuery(query).OrderByDescending(x => x.Date).ToList().Select(x => new
                    {
                        x.Id,
                        x.Ticket.Title,
                        x.RefNumber,
                        x.TotalPrice,
                        x.Quantity,
                        x.Discount,
                        x.Date,
                        x.CustomerNumber,
                        x.Ticket.Price,
                        Type = x.Ticket.Type.ToString(),
                        x.Admission
                    }).ToList();
                    results = WebHelpers.BuildResponse(recs, "Records Loaded", true, recs.Count());
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("api/tickets/getpersonalticketsales")]
        public ResultObj GetPersonalTicketSales(TicketSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var query = db.TicketSales.Where(x => x.CreatedBy == user.UserName).Include(x => x.Ticket);
                    var recs = filter.BuildQuery(query).OrderByDescending(x => x.Date).ToList().Select(x => new
                    {
                        x.Id,
                        x.Ticket.Title,
                        x.RefNumber,
                        x.TotalPrice,
                        x.Quantity,
                        x.Discount,
                        x.Date,
                        x.CustomerNumber,
                        x.Ticket.Price,
                        Type = x.Ticket.Type.ToString(),
                        x.Admission
                    }).ToList();
                    results = WebHelpers.BuildResponse(recs, "Records Loaded", true, recs.Count());
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        

        [HttpGet]
        [Route("api/tickets/getticketsale")]
        public ResultObj GetTicketSale(long id)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var data = db.TicketSales.Where(x => x.Id == id).Include(x => x.Ticket).ToList().Select(x => new
                    {
                        x.Id,
                        Ticket = new
                        {
                            x.Ticket.Id,
                            x.Ticket.Title,
                            Type = x.Ticket.Type.ToString(),
                            x.Ticket.Description,
                            x.Ticket.Price,
                            x.Ticket.RefPrefix,
                            x.Ticket.Admission
                        },
                        x.RefNumber,
                        x.TotalPrice,
                        x.Quantity,
                        x.Discount,
                        x.Date,
                        x.CustomerNumber,
                        x.Admission
                    }).First();
                    results = WebHelpers.BuildResponse(data, "Record Loaded", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("api/tickets/cancelticketsale")]
        public ResultObj CancelTicketSale(CancelledTicketSale obj)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var ts = db.TicketSales.Where(x=> x.Id > 0).Include(x=> x.Ticket).FirstOrDefault(x => x.Id == obj.SaleId);
                    if(ts == null) throw new Exception("Please check the ticket number");
                    db.TicketSales.Remove(ts);

                    //add it to cancelled tickets
                    db.CancelledTicketSales.Add(new CancelledTicketSale
                    {
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        CreatedBy = user.UserName,
                        ModifiedBy = user.UserName,
                        Date = DateTime.Now,
                        TicketSale = JsonConvert.SerializeObject(ts),
                        Notes = obj.Notes
                    });
                    db.SaveChanges();
                    results = WebHelpers.BuildResponse(obj.SaleId, "Sale cancelled", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpGet]
        [Route("api/tickets/getticketsalebyreference")]
        public ResultObj GetTicketSaleByReference(string refNumber)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var data = db.TicketSales.Where(x => x.RefNumber == refNumber).Include(x => x.Ticket).ToList().Select(x => new
                    {
                        x.Id,
                        Ticket = new
                        {
                            x.Ticket.Id,
                            x.Ticket.Title,
                            Type = x.Ticket.Type.ToString(),
                            x.Ticket.Description,
                            x.Ticket.Price,
                            x.Ticket.RefPrefix,
                            x.Ticket.Admission
                        },
                        x.RefNumber,
                        x.TotalPrice,
                        x.Quantity,
                        x.Discount,
                        x.Date,
                        x.CustomerNumber,
                        x.CreatedBy,
                        x.CreatedAt,
                        x.Admission
                    }).First();
                    results = WebHelpers.BuildResponse(data, "Record Loaded", true, 1);
                }
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpGet]
        [Route("api/tickets/getactivetickets")]
        public ResultObj GetActiveTickets()
        {
            ResultObj results;
            try
            {
                var data = Repository.Get().Where(x => x.Status == TicketStatus.Active).OrderBy(x => x.Type).ThenBy(x=> x.Title).ToList().Select(x => new
                {
                    x.Id,
                    x.Title,
                    Type = x.Type.ToString(),
                    x.Description,
                    x.Price,
                    //x.ImageLink,
                    x.RefPrefix,
                    x.Admission
                }).ToList();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpGet]
        [Route("api/tickets/printsale")]
        public ResultObj SellTicket(long id)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                //generate reciept
                var res = new TicketSaleRepository().GenerateReceipt(id);
                results = WebHelpers.BuildResponse(res, "Successful.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }
    }
}