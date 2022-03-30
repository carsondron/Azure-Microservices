namespace EventBusMessages.Events
{
    public class PaymentExecutionEvent : IntegrationBaseEvent
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

