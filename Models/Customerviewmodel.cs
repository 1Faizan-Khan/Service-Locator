using ServiceLocator.Models;

namespace ServiceLocator.Models
{
    public class CustomerPageViewModel
    {
        public List<Customersignup> CustomerList { get; set; }
        public List<Providersignup> Providers { get; set; }
        public Customersignup Customer { get; set; }
    }
}