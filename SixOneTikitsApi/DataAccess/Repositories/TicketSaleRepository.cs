using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using PianoBarApi.AxHelpers;
using PianoBarApi.Models;

namespace PianoBarApi.DataAccess.Repositories
{
    public class TicketSaleRepository : BaseRepository<TicketSale>
    {
        public byte[] GenerateReceipt(long saleId)
        {
            var sale = DbContext.TicketSales.Where(x => x.Id == saleId).Include(x => x.Ticket).First();
            
            var psnl = DbContext.Users.First(x => x.UserName == sale.CreatedBy);
            var contents = File.ReadAllText(System.Web.HttpContext.Current
                .Server.MapPath(@"~/ReportTemplates/TicketReceipt.html"));
            contents = contents.Replace("[REFNUM]", sale.RefNumber);
            contents = contents.Replace("[DATE]", sale.Date.ToShortDateString());
            contents = contents.Replace("[TITLE]", sale.Ticket.Title.ToUpper());
            contents = contents.Replace("[QUANTITY]", sale.Quantity.ToString());
            contents = contents.Replace("[PERSONNEL]", psnl.Name.ToUpper());
            contents = contents.Replace("[ADMISSION]", sale.Admission.ToString());
            contents = contents.Replace("[UNITPRICE]", sale.Ticket.Price.ToString("##,###.00"));
            contents = contents.Replace("[DISCOUNT]", sale.Discount.ToString("##,###.00"));
            contents = contents.Replace("[TOTALPRICE]", sale.TotalPrice.ToString("##,###.00"));

            return Reporter.GeneratePdf(contents, 250, 300, 8);
        }
    }
}