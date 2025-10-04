using ServiceLocator.Models;

namespace ServiceLocator.Models
{
    public class CustomerPageViewModel
    {
        public List<Customersignup> Customers { get; set; }
        public List<Providersignup> Providers { get; set; }
        public Customersignup Customer { get; set; }
    }
}