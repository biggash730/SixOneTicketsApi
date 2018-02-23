using System;
using System.Data.Entity;
using System.Linq;
using PianoBarApi.Models;

namespace PianoBarApi.DataAccess.Filters
{
    public class UserFilter : Filter<User>
    {
        public long ProfileId;

        public override IQueryable<User> BuildQuery(IQueryable<User> query)
        {
            if (ProfileId > 0) query = query.Where(q => q.Profile.Id == ProfileId);

            query = query.Where(q => !q.Hidden);
            return query;
        }
    }

    public class TicketFilter : Filter<Ticket>
    {
        public long Id;
        public string Title;
        public string Description;
        public TicketType? Type;
        public string Status;
        public double PriceFrom;
        public double PriceTo;

        public override IQueryable<Ticket> BuildQuery(IQueryable<Ticket> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(Title)) query = query.Where(q => q.Title.ToLower().Contains(Title.ToLower()));
            if (!string.IsNullOrEmpty(Description)) query = query.Where(q => q.Description.ToLower().Contains(Description.ToLower()));
            if (Type.HasValue) query = query.Where(q => q.Type == Type);
            if (!string.IsNullOrEmpty(Status)) query = query.Where(q => Status.ToLower().Contains(q.Status.ToString()));
            if (PriceFrom > 0) query = query.Where(q => q.Price >= PriceFrom);
            if (PriceTo > 0) query = query.Where(q => q.Price <= PriceTo);

            query = query.Where(q => !q.Hidden);
            return query;
        }
    }

    public class TicketSaleFilter : Filter<TicketSale>
    {
        public long Id;
        public string Title;
        public string Description;
        public string RefNumber;
        public TicketType? Type;
        public TicketStatus? Status;
        public double PriceFrom;
        public double PriceTo;
        public DateTime? DateFrom;
        public DateTime? DateTo;
        public DateTime? Date;
        public DateTime? EndDate;
        public DateTime? StartDate;

        public override IQueryable<TicketSale> BuildQuery(IQueryable<TicketSale> query)
        {
            query = query.Include(x => x.Ticket);
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(Title)) query = query.Where(q => q.Ticket.Title.ToLower().Contains(Title.ToLower()));
            if (!string.IsNullOrEmpty(Description)) query = query.Where(q => q.Ticket.Description.ToLower().Contains(Description.ToLower()));
            if (!string.IsNullOrEmpty(RefNumber)) query = query.Where(q => q.RefNumber.ToLower().Contains(RefNumber.ToLower()));
            if (Type.HasValue) query = query.Where(q => q.Ticket.Type == Type);
            if (Status.HasValue) query = query.Where(q => q.Ticket.Status == Status);
            if (PriceFrom > 0) query = query.Where(q => q.Ticket.Price >= PriceFrom);
            if (PriceTo > 0) query = query.Where(q => q.Ticket.Price <= PriceTo);
            if (DateFrom.HasValue)
            {
                DateFrom = new DateTime(DateFrom.Value.Year,DateFrom.Value.Month, DateFrom.Value.Day,0,0,0);
                query = query.Where(q => q.Date >= DateFrom);
            }
            if (DateTo.HasValue)
            {
                DateTo = new DateTime(DateTo.Value.Year, DateTo.Value.Month, DateTo.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Date <= DateTo);
            }
            if (StartDate.HasValue)
            {
                StartDate = new DateTime(StartDate.Value.Year, StartDate.Value.Month, StartDate.Value.Day, 0, 0, 0);
                query = query.Where(q => q.Date >= StartDate);
            }
            if (EndDate.HasValue)
            {
                EndDate = new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Date <= EndDate);
            }
            if (Date.HasValue)
            {
                query = query.Where(x => x.Date.Year == Date.Value.Year && x.Date.Month == Date.Value.Month && x.Date.Day== Date.Value.Day);
            }

            query = query.Where(q => !q.Hidden);
            return query;
        }
    }

    public class CancelledTicketSaleFilter : Filter<CancelledTicketSale>
    {
        public long Id;
        public DateTime? DateFrom;
        public DateTime? DateTo;
        public DateTime? EndDate;
        public DateTime? StartDate;

        public override IQueryable<CancelledTicketSale> BuildQuery(IQueryable<CancelledTicketSale> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            
            if (DateFrom.HasValue)
            {
                DateFrom = new DateTime(DateFrom.Value.Year, DateFrom.Value.Month, DateFrom.Value.Day, 0, 0, 0);
                query = query.Where(q => q.Date >= DateFrom);
            }
            if (DateTo.HasValue)
            {
                DateTo = new DateTime(DateTo.Value.Year, DateTo.Value.Month, DateTo.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Date <= DateTo);
            }
            if (StartDate.HasValue)
            {
                StartDate = new DateTime(StartDate.Value.Year, StartDate.Value.Month, StartDate.Value.Day, 0, 0, 0);
                query = query.Where(q => q.Date >= StartDate);
            }
            if (EndDate.HasValue)
            {
                EndDate = new DateTime(EndDate.Value.Year, EndDate.Value.Month, EndDate.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Date <= EndDate);
            }
            query = query.Where(q => !q.Hidden);
            return query;
        }
    }
}