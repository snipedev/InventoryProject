using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrdersService.Models
{
    public enum OrderStatus
    {
        Delivered,
        Pending,
        Failed,
        Cancelled,
        Placed
    }
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public ICollection<Item> Items { get; set; }
        public OrderStatus Status { get; set; }
        public Address Address { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime PlacedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PaymentMethod { get; set; }
    }

    public record Item
    {
        [Key]
        public int ItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }

    public record Address
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    public class Entities
    {
    }
}
