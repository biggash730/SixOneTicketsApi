using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using PdfSharp;
using SixOneTikitsApi.AxHelpers;
using SixOneTikitsApi.DataAccess.Filters;
using SixOneTikitsApi.DataAccess.Repositories;
using SixOneTikitsApi.Extensions;
using SixOneTikitsApi.Models;

namespace SixOneTikitsApi.Controllers
{
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        [Route("ticketslist")]
        public ResultObj TicketsList(TicketFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<Ticket>();
                var tickets =
                    repo.Query(filter).Select(x => new
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


                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/TicketsList.html"));

                //Write company details
                

                var list = string.Empty;
                var num = 0;

                foreach (var item in tickets)
                {
                    num++;
                    list += "<tr style='border:none;font-size: 14px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            //$"<td>{item.Status}</td>" +
                            $"<td>{item.Title}</td>" +
                            $"<td  class='text-right'>{item.Price:f2}</td>" +
                            $"<td>{item.Description}</td>" +
                            $"<td>{item.Type}</td>" +
                            $"<td>{item.Status}</td> </tr>";
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", num.ToString());

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4);
                results = WebHelpers.BuildResponse(pdfOutput, "Tickets List Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("ticketsales")]
        public ResultObj TicketsSales(TicketSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<TicketSale>();
                var tickets =
                    repo.Query(filter).OrderBy(x=> x.Date).ToList().Select(x => new
                    {
                        x.Id,
                        x.Ticket.Title,
                        x.RefNumber,
                        x.TotalPrice,
                        x.Quantity,
                        x.Discount,
                        Date = x.Date.ToShortDateString(),
                        //Customer = new CustomerRepository().GetByNumber(x.CustomerNumber)?.Name,
                        x.Ticket.Price,
                        Type = x.Ticket.Type.ToString()
                    }).ToList();


                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/TicketSales.html"));

                //Write company details


                var list = string.Empty;
                var num = 0;
                var total = 0.0;

                foreach (var item in tickets)
                {
                    num++;
                    list += "<tr style='border:none;font-size: 14px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Date}</td>" +
                            $"<td>{item.Id}</td>" +
                            $"<td>{item.RefNumber}</td>" +
                            $"<td>{item.Title}</td>" +
                            $"<td>{item.Type}</td>" +
                            $"<td class='text-right'>{item.Price:f2}</td>" +
                            $"<td>{item.Quantity}</td>" +
                            $"<td class='text-right'>{item.Discount:f2}</td>" +
                            $"<td class='text-right'>{item.TotalPrice:f2}</td> </tr>";

                    total = total + item.TotalPrice;
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", $"{total:f2}");

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Ticket Sales Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("cancelledticketsales")]
        public ResultObj CancelledTicketsSales(CancelledTicketSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<CancelledTicketSale>();
                var tickets =
                    repo.Query(filter).OrderBy(x => x.Date).ToList().Select(x => new
                    {
                        x.Id,
                        x.Notes,
                        x.CreatedBy,
                        Date = x.CreatedAt.ToShortDateString(),
                        x.TicketSale
                    }).ToList();


                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/CancelledTicketSales.html"));

                var list = string.Empty;
                var num = 0;
                var total = 0.0;

                foreach (var item in tickets)
                {
                    num++;
                    var tick = JsonConvert.DeserializeObject<TicketSale>(item.TicketSale);
                    list += "<tr style='border:none;font-size: 14px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.Date}</td>" +
                            $"<td>{item.CreatedBy}</td>" +
                            $"<td>{tick.RefNumber}</td>" +
                            $"<td>{tick.Ticket.Title}</td>" +
                            $"<td>{tick.Quantity}</td>" +
                            $"<td class='text-right'>{tick.TotalPrice:f2}</td>" +
                            $"<td>{item.Notes}</td> </tr>";

                    total = total + tick.TotalPrice;
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", $"{total:f2}");

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Cancelled Ticket Sales Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

    }
}
