using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Windows.Forms;
using Newtonsoft.Json;
using PdfSharp;
using PianoBarApi.AxHelpers;
using PianoBarApi.DataAccess.Filters;
using PianoBarApi.DataAccess.Repositories;
using PianoBarApi.Extensions;
using PianoBarApi.Models;

namespace PianoBarApi.Controllers
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
                        Customer = new CustomerRepository().GetByNumber(x.CustomerNumber)?.Name,
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
                            $"<td>{item.Customer}</td>" +
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

        [Route("saleslist")]
        public ResultObj SalesList(MenuSaleFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<MenuSale>();
                var tickets =
                    repo.Query(filter).OrderBy(x => x.Date).ToList().Select(x => new
                    {
                        x.Id,
                        x.Reference,
                        x.CartId,
                        x.Discount,
                        x.CreatedBy,
                        x.ModifiedBy,
                        x.CreatedAt,
                        x.ModifiedAt,
                        x.Balance,
                        x.AmountPayable,
                        x.AmountPaid,
                        Total = x.AmountPaid - x.Balance,
                        x.Date,
                        x.ReceiptName,
                        Customer = new CustomerRepository().GetByNumber(x.Cart?.CustomerNumber)?.Name,
                    }).ToList();


                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/Sales.html"));

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
                            $"<td>{item.Reference}</td>" +
                            $"<td>{item.Customer}</td>" +
                            $"<td class='text-right'>{item.AmountPayable:f2}</td>" +
                            $"<td class='text-right'>{item.Discount:f2}</td>" +
                            $"<td class='text-right'>{item.Total:f2}</td> </tr>";

                    total = total + item.Total;
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", $"{total:f2}");

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4);
                results = WebHelpers.BuildResponse(pdfOutput, "Sales Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("cancelledorders")]
        public ResultObj CancelledOrders(MenuOrderFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<MenuOrder>();
                var tickets =
                    repo.Query(filter).Where(x=> x.Status == OrderStatus.Cancelled).OrderBy(x => x.CreatedAt).ToList().Select(x => new
                    {
                        x.Id,
                        x.Quantity,
                        x.Reference,
                        x.MenuId,
                        x.CartId,
                        x.CreatedBy,
                        x.ModifiedBy,
                        x.CreatedAt,
                        ModifiedAt = x.ModifiedAt.ToShortDateString(),
                        x.Reason,
                        x.ReceivedBy,
                        Status = x.Status.ToString(),
                        Category = x.Menu.Category.Name,
                        Type = x.Menu.Category.Type.ToString(),
                        Menu = x.Menu.Title,
                        x.Menu.SellingPriceWithVat,
                        x.Menu.SellingPriceWithoutVat,
                        Total = x.Menu.SellingPriceWithVat * x.Quantity,
                        Customer = new CustomerRepository().GetByNumber(x.Cart?.CustomerNumber)?.Name,
                    }).ToList();
                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/CancelledOrders.html"));

                //Write company details


                var list = string.Empty;
                var num = 0;
                var total = 0.0;

                foreach (var item in tickets)
                {
                    num++;
                    list += "<tr style='border:none;font-size: 14px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.ModifiedAt}</td>" +
                            $"<td>{item.Reference}</td>" +
                            $"<td>{item.Customer}</td>" +
                            $"<td>{item.ModifiedBy}</td>" +
                            $"<td>{item.Reason}</td>" +
                            $"<td class='text-right'>{item.SellingPriceWithVat:f2}</td>" +
                            $"<td class='text-right'>{item.Quantity}</td> "+
                            $"<td class='text-right'>{item.Total:f2}</td> </tr>";

                    total = total + item.Total;
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", $"{total:f2}");

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Cancelled Orders Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("rejectedorders")]
        public ResultObj RejectedOrders(MenuOrderFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<MenuOrder>();
                var tickets =
                    repo.Query(filter).Where(x => x.Status == OrderStatus.Rejected).OrderBy(x => x.CreatedAt).ToList().Select(x => new
                    {
                        x.Id,
                        x.Quantity,
                        x.Reference,
                        x.MenuId,
                        x.CartId,
                        x.CreatedBy,
                        x.ModifiedBy,
                        x.CreatedAt,
                        x.ModifiedAt,
                        x.Reason,
                        x.ReceivedBy,
                        Status = x.Status.ToString(),
                        Category = x.Menu.Category.Name,
                        Type = x.Menu.Category.Type.ToString(),
                        Menu = x.Menu.Title,
                        x.Menu.SellingPriceWithVat,
                        Total = x.Menu.SellingPriceWithVat * x.Quantity,
                        Customer = new CustomerRepository().GetByNumber(x.Cart?.CustomerNumber)?.Name,
                    }).ToList();
                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/RejectedOrders.html"));

                var list = string.Empty;
                var num = 0;
                var total = 0.0;

                foreach (var item in tickets)
                {
                    num++;
                    list += "<tr style='border:none;font-size: 14px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.ModifiedAt}</td>" +
                            $"<td>{item.Reference}</td>" +
                            $"<td>{item.Customer}</td>" +
                            $"<td>{item.ModifiedBy}</td>" +
                            $"<td>{item.Reason}</td>" +
                            $"<td class='text-right'>{item.SellingPriceWithVat:f2}</td>" +
                            $"<td class='text-right'>{item.Quantity}</td> " +
                            $"<td class='text-right'>{item.Total}</td> </tr>";

                    total = total + item.Total;
                }

                template = template.Replace("[LIST]", list);
                template = template.Replace("[TOTAL]", $"{total:f2}");

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Rejected Orders Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("customers")]
        public ResultObj Customers(CustomerFilter filter)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var repo = new BaseRepository<Customer>();
                var custs =
                    repo.Query(filter).OrderBy(x => x.CreatedAt).ToList().Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.PhoneNumber,
                        x.Email,
                        Status = x.Status.ToString(),
                        x.RefNumber,
                        x.ResidentialAddress,
                        x.DateOfBirth,
                        IdType = x.IdType?.Name,
                        x.IdTypeId,
                        x.IdNumber,
                        x.IdExpiryDate,
                        x.City,
                        x.PackageId,
                        Package = x.Package?.Name,
                        x.Fee,
                        PackageStartDate = x.PackageStartDate?.ToShortDateString(),
                        PackageEndDate = x.PackageEndDate?.ToShortDateString(),
                        x.CreatedBy,
                        x.ModifiedBy,
                        CreatedAt = x.CreatedAt.ToShortDateString(),
                        ModifiedAt = x.ModifiedAt.ToShortDateString(),
                    }).ToList();
                var template = File.ReadAllText(HttpContext.Current
                    .Server.MapPath(@"~/ReportTemplates/CustomersList.html"));

                var list = string.Empty;
                var num = 0;

                foreach (var item in custs)
                {
                    num++;
                    list += "<tr style='border:none;font-size: 14px; text-transform: uppercase !important;'>" +
                            $"<td>{num}</td>" +
                            $"<td>{item.CreatedAt}</td>" +
                            $"<td>{item.Name}</td>" +
                            $"<td>{item.PhoneNumber}</td>" +
                            $"<td>{item.RefNumber}</td>" +
                            $"<td>{item.Package}</td>" +
                            $"<td>{item.PackageStartDate}</td>" +
                            $"<td>{item.PackageStartDate}</td>" +
                            $"<td>{item.PackageEndDate}</td> </tr>";
                }

                template = template.Replace("[LIST]", list);

                var pdfOutput = Reporter.GeneratePdf(template, PageSize.A4, true);
                results = WebHelpers.BuildResponse(pdfOutput, "Customers List Report", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

    }
}
