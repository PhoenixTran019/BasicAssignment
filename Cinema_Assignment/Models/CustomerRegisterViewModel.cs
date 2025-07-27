namespace Cinema_Assignment.Models
{
    public class CustomerRegisterViewModel
    {
        public int CustomerID { get; set; }
        public int AccountID { get; set; }
        public int LoginID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
