﻿using Orders.Domain.Common;

namespace Orders.Domain.Entities
{
    public class OrderItem : EntityBase
    {
        public string Username { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine { get; set; }
        public string Country { get; set; }

        public decimal TotalPrice { get; set; }

        public string? CardName { get; set; }
        public string? CardNumber { get; set; }
        public string? Expiration { get; set; } 
        public string? CVV { get; set; }
        public int PaymentMethod { get; set; }
    }
}