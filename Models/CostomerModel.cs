namespace CryptoBot
{
    public class CostomerModel
    {
        public int Id { get; set; }
        public decimal HowMuchGives { get; set;}
        public decimal Course { get; set; }
        public decimal HowMuchGet { get; set;}
        public string CurrencyCell { get; set; }
        public string CurrencyGet { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }   
        public string LastName { get; set; }   
        public string Username { get; set; } 
        public string CardNumber { get; set; }
        public string Service { get; set; } 
        public bool Order { get; set; }
        public bool IfEnd { get; set; }
    }
}