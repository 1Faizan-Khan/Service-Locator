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
                ViewData["BodyClass"] = "homepage-background";
                if (_context.Customer.Any(x => x.Email == loginModel.Email)) // checks to see if customer's email is in the system
                {
                    var theCustomer = _context.Customer // thecustomer equals the row where the email matches the email logged in with
                        .FirstOrDefault(c => c.Email == loginModel.Email);

                    var matchingProviders = _context.Provider // finds all providers that match the customer's requested service
                        .Where(p => p.professionName.ToLower() == theCustomer.Whatservice.ToLower())
                        .ToList();

                    var vm = new CustomerPageViewModel // creates viewmodel to pass to customerpage
                    {
                        Providers = matchingProviders
                    };

                    ViewData["service"] = theCustomer.Whatservice;
                    ViewData["zip"] = theCustomer.Zipcode;
                    HttpContext.Session.SetInt32("CustomerId", theCustomer.Id);
                    ViewBag.NotificationCount = GetUnreadNotificationCount();
                    return View("Customerpage", vm);
                }
            }
            else if (userType == "Provider")
            {
                ViewData["BodyClass"] = "homepage-background";

                var theProvider = _context.Provider // row where provider matches the email logged in with
                    .FirstOrDefault(c => c.Email == loginModel.Email);

                if (theProvider == null)
                {
                    ViewData["user"] = userType;
                    ViewData["Error"] = "Email is not in the system";
                    ViewData["BodyClass"] = "homepage-background";
                    return View(); // back to login view with error
                }

                var matchingCustomers = _context.Customer // finds all customers that match the provider's profession
                    .Where(c => c.Whatservice == theProvider.professionName)
                    .ToList();

                var vm = new CustomerPageViewModel // creates viewmodel to pass to providerpage
                {
                    CustomerList = matchingCustomers // <- fixed reference
                };

                ViewData["zip"] = theProvider.Zipcode;
                HttpContext.Session.SetInt32("ProviderId", theProvider.Id);
                ViewBag.NotificationCount = GetUnreadNotificationCount();
                return View("Providerpage", vm);
            }

            ViewData["user"] = userType;
            ViewData["BodyClass"] = "homepage-background";
            ViewData["Error"] = "Email is not in the system";
            return View();
        }

        //Request Service action
        [HttpPost]
        public IActionResult RequestService([FromForm] int providerId)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return Unauthorized();

            Console.WriteLine("==== REQUEST SERVICE DEBUG ====");
            Console.WriteLine($"CustomerId: {customerId}, ProviderId: {providerId}");
            Console.WriteLine("===============================");

            // ✅ Check for duplicate request
            var alreadyExists = _context.Notifications.Any(n =>
                n.InitiatorType == "Customer" &&
                n.InitiatorId == customerId.Value &&
                n.TargetType == "Provider" &&
                n.TargetId == providerId
            );

            if (alreadyExists)
            {
                return BadRequest("You already requested this provider.");
            }

            var notification = new Notification
            {
                InitiatorType = "Customer",
                InitiatorId = customerId.Value,
                TargetType = "Provider",
                TargetId = providerId,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return Ok();
        }


        [HttpPost]
        public IActionResult ProvideService([FromForm] int customerId)
        {
            int? providerId = HttpContext.Session.GetInt32("ProviderId");
            if (providerId == null)
                return Unauthorized();

            Console.WriteLine("==== PROVIDE SERVICE DEBUG ====");
            Console.WriteLine($"ProviderId: {providerId}, customerId: {customerId}");
            Console.WriteLine("===============================");

            // ✅ DEFINE alreadyExists HERE
            var alreadyExists = _context.Notifications.Any(n =>
                n.InitiatorType == "Provider" &&
                n.InitiatorId == providerId.Value &&
                n.TargetType == "Customer" &&
                n.TargetId == customerId
            );

            if (alreadyExists)
            {
                return BadRequest("You already offered your service.");
            }

            var notification = new Notification
            {
                InitiatorType = "Provider",
                InitiatorId = providerId.Value,
                TargetType = "Customer",
                TargetId = customerId,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return Ok();
        }

            public IActionResult Notifications()
{
    int? customerId = HttpContext.Session.GetInt32("CustomerId");
    int? providerId = HttpContext.Session.GetInt32("ProviderId");

    IQueryable<Notification> query = _context.Notifications;

    if (customerId != null)
    {
        query = query.Where(n =>
            n.TargetType == "Customer" &&
            n.TargetId == customerId.Value);
    }
    else if (providerId != null)
    {
        query = query.Where(n =>
            n.TargetType == "Provider" &&
            n.TargetId == providerId.Value);
    }
    else
    {
        return Unauthorized();
    }

    var notifications = query
        .OrderByDescending(n => n.CreatedAt)
        .ToList();

    // Mark all as read
    notifications.ForEach(n => n.IsRead = true);
    _context.SaveChanges();

    bool isCustomer = customerId != null;

    var model = notifications.Select(n =>
    {
        string contactName = "";
        string contactPhone = "";
        string contactEmail = "";

        if (n.InitiatorType == "Customer")
        {
            var c = _context.Customer.FirstOrDefault(cust => cust.Id == n.InitiatorId);
            if (c != null)
            {
                contactName = c.Name;
                contactPhone = c.Phone; // assuming your Customer entity has Phone
                contactEmail = c.Email;
            }
        }
        else if (n.InitiatorType == "Provider")
        {
            var p = _context.Provider.FirstOrDefault(prov => prov.Id == n.InitiatorId);
            if (p != null)
            {
                contactName = p.Name;
                contactPhone = p.Phone; // assuming your Provider entity has Phone
                contactEmail = p.Email;
            }
        }

        return new NotificationViewModel
        {
            NotificationId = n.Id,
            InitiatorName = contactName,
            Service = isCustomer
                ? _context.Provider.FirstOrDefault(p => p.Id == n.InitiatorId)?.professionName
                : _context.Customer.FirstOrDefault(c => c.Id == n.InitiatorId)?.Whatservice,
            viewerIsCustomer = isCustomer,
            IsAccepted = n.IsAccepted, // make sure you added this bool to your Notification entity
            Phone = contactPhone,
            Email = contactEmail
        };
    }).ToList();

    return View(model);
}



        public int GetUnreadNotificationCount()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            int? providerId = HttpContext.Session.GetInt32("ProviderId");

            if (customerId != null)
            {
                return _context.Notifications.Count(n =>
                    n.TargetType == "Customer" &&
                    n.TargetId == customerId.Value &&
                    !n.IsRead);
            }

            if (providerId != null)
            {
                return _context.Notifications.Count(n =>
                    n.TargetType == "Provider" &&
                    n.TargetId == providerId.Value &&
                    !n.IsRead);
            }

            return 0;
        }

        [HttpPost]
        public IActionResult AcceptNotification(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null) return NotFound();

            notification.IsAccepted = true;

            // Create a new notification for the initiator
            _context.Notifications.Add(new Notification
            {
                InitiatorType = notification.TargetType,   // the current user who accepted
                InitiatorId = notification.TargetId,
                TargetType = notification.InitiatorType,  // the original initiator
                TargetId = notification.InitiatorId,
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();

            return Ok();
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
