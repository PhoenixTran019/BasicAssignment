namespace Cinema_Assignment.Models
{
    public class ConfirmBookingRequest
    {
        public int ShowtimeId { get; set; }
        public string SelectedSeats { get; set; } // "A1,B2,C3"
        public string PaymentMethod { get; set; } // VD: "Cash", "CreditCard"
    }
}
