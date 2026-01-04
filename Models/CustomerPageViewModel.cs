namespace ServiceLocator.Models
{
    public class CustomerPageViewModel
    {
        // Logged-in user (customer OR provider)
        public Customersignup Customer { get; set; }
        public Providersignup Provider { get; set; }

        // Lists for search results / panels
        public List<Customersignup> CustomerList { get; set; } = new();
        public List<Providersignup> Providers { get; set; } = new();

        // Search inputs (shared)
        public string Service { get; set; }
        public int Radius { get; set; }
    }
}
