using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PianoBarApi.AxHelpers;

namespace PianoBarApi.Models
{
    public interface IHasId
    {
        [Key]
        long Id { get; set; }
    }

    public interface ISecured
    {
        bool Locked { get; set; }
        bool Hidden { get; set; }
    }

    public interface IAuditable : IHasId
    {
        [Required]
        string CreatedBy { get; set; }
        [Required]
        string ModifiedBy { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }

    public class HasId : IHasId
    {
        public long Id { get; set; }
    }

    public class AuditFields : HasId, IAuditable, ISecured
    {
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
    }

    public class LookUp : AuditFields
    {
        [MaxLength(512), Required, Index(IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
    }

    public class User : IdentityUser
    {
        [MaxLength(128), Required]
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public virtual Profile Profile { get; set; }
        public long ProfileId { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IdExpiryDate { get; set; }
        public string ResidentialAddress { get; set; }
        public string City { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public string Token { get; set; }
        
        public bool TokenVerified { get; set; } = false;
        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public string ConfirmPassword { get; set; }
        [NotMapped]
        public long RoleId { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            return userIdentity;
        }
    }

    public class Profile : HasId
    {
        [Required, MaxLength(512), Index(IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
        [MaxLength(500000)]
        public string Privileges { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public List<User> Users { get; set; }
    }

    public class AppSetting : AuditFields
    {
        [MaxLength(512), Required, Index(IsUnique = true)]
        public string Name { get; set; }
        public string Value { get; set; }
    }
    
    public enum TicketType
    {
        Regular,
        Member
    }
    public enum TicketStatus
    {
        Pending,
        Active,
        Cancelled
    }

    public class Ticket : AuditFields
    {
        [Required, MaxLength(256), Index(IsUnique = true)]
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public TicketType Type { get; set; }
        public TicketStatus Status { get; set; }
        public string RefPrefix { get; set; }
        public string Reason { get; set; }
        public int Admission { get; set; }
    }

    public class TicketSale : AuditFields
    {
        public long TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string RefNumber { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public double TotalPrice { get; set; }
        public string CustomerNumber { get; set; }
        public string Reason { get; set; }
        public int Admission { get; set; }
    }

    public class TicketStats
    {
        public long TicketsSold { get; set; } = 0;
        public long RegularTickets { get; set; } = 0;
        public long MemberTickets { get; set; } = 0;
        public double CashReceived { get; set; } = 0.0;
        public long CancelledTicketSales { get; set; } = 0;
    }

    public class CancelledTicketSale : AuditFields
    {
        public string TicketSale { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Notes { get; set; }
        [NotMapped]
        public long SaleId { get; set; }
    }

    public class CancelModel
    {
        public long Id { get; set; }
        public string Note { get; set; }
    }

    public class ResetRequest : HasId
    {
        public string Ip { get; set; } = "127.0.0.1";
        public DateTime Date { get; set; } = DateTime.Now;
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public bool IsActive { get; set; } = false;
    }

    public class ResetModel
    {
        public string Token { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class VerifyModel
    {
        public string Code { get; set; }
        public string PhoneNumber { get; set; }
    }


    public class Message : HasId
    {
        [MaxLength(256), Required]
        public string Recipient { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string Subject { get; set; }
        [Required]
        public string Text { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Pending;
        public MessageType Type { get; set; } = MessageType.SMS;
        [MaxLength(5000)]
        public string Response { get; set; }
        public DateTime TimeStamp { get; set; }
        [NotMapped]
        public string Attachment { get; set; }
    }

    public enum MessageType
    {
        SMS,
        Email
    }

    public enum MessageStatus
    {
        Pending,
        Sent,
        Received,
        Failed
    }

    


}