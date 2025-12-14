using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ServiceLocator.Models;
using ServiceLocator.Services; // <-- Add this for GeoService

namespace ServiceLocator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GeoServices _geoService; // <-- Add this
        private readonly Dbcontext _context;

        public HomeController(ILogger<HomeController> logger, GeoServices geoService, Dbcontext context) // <-- Inject here
        {
            _logger = logger;
            _geoService = geoService;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["BodyClass"] = "homepage-background";
            return View();
        }

        [HttpPost]
        public IActionResult Customer(Customersignup model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["BodyClass"] = "homepage-background";
                return View(model);
            }

            if (_context.Customer.Any(x => x.Email == model.Email))
            {
                ViewData["Error"] = "Email is already in the system";
                ViewData["BodyClass"] = "homepage-background";
                return View("Customer");
            }
            else
            {
                ViewData["service"] = model.Whatservice;
                ViewData["zip"] = model.Zipcode;
                _context.Customer.Add(model);
                _context.SaveChanges();

                if (_context.Provider.Any(x => x.professionName == model.Whatservice))
                {
                    var results = _context.Provider
                        .Where(x => x.professionName == model.Whatservice)
                        .ToList();

                    var realresult = new CustomerPageViewModel
                    {
                        Providers = results
                    };

                    ViewData["BodyClass"] = "homepage-background";
                    return View("Customerpage", realresult);
                }
            }

            ViewData["cantfind"] = "Can't find any user that provides that service";
            ViewData["BodyClass"] = "homepage-background";
            return View("Customerpage", new CustomerPageViewModel { Providers = new List<Providersignup>() });
        }

        [HttpPost]
        public IActionResult Customerpage(CustomerPageViewModel model)
        {
            bool serviceExists = _context.Provider
                .Any(p => p.professionName.Contains(model.Customer.Whatservice));

            if (serviceExists)
            {
                var results = _context.Provider
                    .Where(p => p.professionName.Contains(model.Customer.Whatservice))
                    .ToList();

                var real = new CustomerPageViewModel
                {
                    Providers = results
                };

                ViewData["service"] = model.Customer.Whatservice;
                ViewData["zip"] = model.Customer.Zipcode;
                ViewData["radius"] = model.Customer.Radius;
                ViewData["BodyClass"] = "homepage-background";
                return View(real);
            }

            ViewData["cantfind"] = "Can't find a provider that provides that service";
            ViewData["service"] = model.Customer.Whatservice;
            ViewData["zip"] = model.Customer.Zipcode;
            ViewData["radius"] = model.Customer.Radius;
            ViewData["BodyClass"] = "homepage-background";
            return View(new CustomerPageViewModel { Providers = new List<Providersignup>() });
        }

        public IActionResult Customer()
        {
            ViewData["BodyClass"] = "homepage-background";
            return View();
        }

        public IActionResult Provider()
        {
            ViewData["BodyClass"] = "homepage-background";
            return View();
        }

        [HttpPost]
        public IActionResult Provider(Providersignup model)
        {
            if (_context.Provider.Any(x => x.Email == model.Email))
            {
                ViewData["BodyClass"] = "homepage-background";
                ViewData["Error"] = "Email is already in the system";
                return View("Provider");
            }
            else
            {
                _context.Provider.Add(model);
                _context.SaveChanges();
                return View("Providerpage");
            }
        }

        public IActionResult Login(string userType)
        {
            ViewData["BodyClass"] = "homepage-background";
            ViewData["UserType"] = userType; // Pass to view
            return View();
        }

        [HttpPost]
        public IActionResult Login(Login loginModel, string userType)
        {
            if (userType == "Customer")
            {
                if (_context.Customer.Any(x => x.Email == loginModel.Email))
                {
                    var theCustomer = _context.Customer
                        .FirstOrDefault(c => c.Email == loginModel.Email);

                    var matchingProviders = _context.Provider
                        .Where(p => p.professionName.ToLower() == theCustomer.Whatservice.ToLower())
                        .ToList();

                    var vm = new CustomerPageViewModel
                    {
                        Providers = matchingProviders
                    };

                    ViewData["service"] = theCustomer.Whatservice;
                    ViewData["zip"] = theCustomer.Zipcode;
                    ViewData["BodyClass"] = "homepage-background";
                    return View("Customerpage", vm);
                }
            }
            else if (userType == "Provider")
            {
                ViewData["BodyClass"] = "homepage-background";
                var theProvider = _context.Provider
                    .FirstOrDefault(c => c.Email == loginModel.Email);

                if (theProvider == null)
                {
                    ViewData["user"] = userType;
                    ViewData["Error"] = "Email is not in the system";
                    ViewData["BodyClass"] = "homepage-background";
                    return View(); // back to login view with error
                }

                var matchingCustomers = _context.Customer
                    .Where(c => c.Whatservice == theProvider.professionName)
                    .ToList();

                var vm = new CustomerPageViewModel
                {
                    CustomerList = matchingCustomers // <- fixed reference
                };

                ViewData["zip"] = theProvider.Zipcode;
                return View("Providerpage", vm);
            }

            ViewData["user"] = userType;
            ViewData["BodyClass"] = "homepage-background";
            ViewData["Error"] = "Email is not in the system";
            return View();
        }

        // New test action
        public async Task<IActionResult> Location(string zip)
        {
            if (string.IsNullOrWhiteSpace(zip))
                return BadRequest("ZIP code is required.");

            var location = await _geoService.GetCoordinatesAsync(zip);

            if (location == null)
                return NotFound("Could not retrieve location.");

            return Ok(location); // You can also return a View if you prefer
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
