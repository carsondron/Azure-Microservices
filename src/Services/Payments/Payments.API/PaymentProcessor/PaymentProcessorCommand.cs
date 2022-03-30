using MediatR;

namespace Payments.API.PaymentProcessor
{
    public class PaymentProcessorCommand : IRequest<bool>
    {
        public string Username { get; set; }

        public decimal TotalPrice { get; set; }

        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        public int PaymentMethod { get; set; }
    }
}