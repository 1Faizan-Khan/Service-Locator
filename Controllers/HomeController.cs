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
                return View(model);
            }

            // Save customer
            _context.Customer.Add(model);
            _context.SaveChanges();

            // Session
            HttpContext.Session.SetInt32("CustomerId", model.Id);
            HttpContext.Session.Remove("ProviderId");


            // ViewData
            ViewData["service"] = model.Whatservice;
            ViewData["zip"] = model.Zipcode;
            ViewData["radius"] = model.Radius;
            ViewData["city"] = model.City;
            ViewData["state"] = model.State;
            ViewData["BodyClass"] = "homepage-background";
            ViewBag.NotificationCount = GetUnreadNotificationCount();

            var providers = _context.Provider
                .Where(x => x.professionName.ToLower().Trim() == model.Whatservice.ToLower().Trim())
                .ToList();

            if (!providers.Any())
            {
                ViewData["cantfind"] = "Can't find any user that provides that service";
            }

            return View("Customerpage", new CustomerPageViewModel
            {
                Providers = providers
            });
        }





       [HttpGet]
        public IActionResult Customer()
        {
            ViewData["BodyClass"] = "homepage-background";

            return View("Customer");
        }





        [HttpGet]
        public IActionResult Provider(string search)
        {
            ViewData["BodyClass"] = "homepage-background";

            if (!string.IsNullOrEmpty(search) && search.ToLower() == "true")
            {
                var vm = new CustomerPageViewModel
                {
                    CustomerList = _context.Customer
                        .Where(c => c.Whatservice == "Plumber")
                        .ToList()
                };

                return View("Providerpage", vm);
            }

            return View("Provider");
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

                HttpContext.Session.SetInt32("ProviderId", model.Id);
                HttpContext.Session.Remove("CustomerId");

                var matchingCustomers = _context.Customer
                    .Where(c => c.Whatservice.ToLower().Trim() == model.professionName.ToLower().Trim())
                    .ToList();

                var vm = new CustomerPageViewModel
                {
                    CustomerList = matchingCustomers
                };

                ViewData["service"] = model.professionName;
                ViewData["zip"] = model.Zipcode;
                ViewData["radius"] = model.Radius;
                ViewData["city"] = model.City;
                ViewData["state"] = model.State;

                // 🔹 Guidance messages for provider
                var guidanceSteps = new List<string>
                {
                    "Welcome to ServiceLocator! Let's walk you through how to provide services.",
                    "You can view customers requesting your services on your homepage.",
                    "Click on a customer to view their request details and message them.",
                    "Once you accept a request, you can start communicating through the messaging system.",
                    "Remember to keep your profile updated with your services and availability."
                };

                TempData["GuidanceSteps"] = System.Text.Json.JsonSerializer.Serialize(guidanceSteps);

                ViewData["BodyClass"] = "homepage-background";
                ViewBag.NotificationCount = GetUnreadNotificationCount();
                return View("Providerpage", vm);
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
                        .FirstOrDefault(c => c.Email == loginModel.Email
                                        && c.Password == loginModel.Password);

                    if (theCustomer == null)
                    {
                        ViewData["UserType"] = userType;
                        ViewData["Error"] = "Invalid email or password";
                        ViewData["BodyClass"] = "homepage-background";
                        return View();
                    }

                    var matchingProviders = _context.Provider // finds all providers that match the customer's requested service
                        .Where(p => p.professionName.ToLower() == theCustomer.Whatservice.ToLower())
                        .ToList();

                    var vm = new CustomerPageViewModel // creates viewmodel to pass to customerpage
                    {
                        Providers = matchingProviders
                    };

                    ViewData["service"] = theCustomer.Whatservice;
                    ViewData["zip"] = theCustomer.Zipcode;
                    ViewData["city"] = theCustomer.City;
                    ViewData["State"] = theCustomer.State;
                    HttpContext.Session.Remove("ProviderId");
                    HttpContext.Session.SetInt32("CustomerId", theCustomer.Id);
                    ViewBag.NotificationCount = GetUnreadNotificationCount();
                    return View("Customerpage", vm);
                }
            }
            else if (userType == "Provider")
            {
                ViewData["BodyClass"] = "homepage-background";

                var theProvider = _context.Provider // row where provider matches the email logged in with
                    .FirstOrDefault(c => c.Email == loginModel.Email
                                    && c.Password == loginModel.Password);

                if (theProvider == null)
                {
                    ViewData["UserType"] = userType;
                    ViewData["Error"] = "Invalid Email or Password";
                    ViewData["BodyClass"] = "homepage-background";
                    return View(); // back to login view with error
                }

                var matchingCustomers = _context.Customer
                .Where(c =>
                    c.Whatservice.ToLower().Trim() ==
                    theProvider.professionName.ToLower().Trim()
                )
                .ToList();

                var vm = new CustomerPageViewModel // creates viewmodel to pass to providerpage
                {
                    CustomerList = matchingCustomers // <- fixed reference
                };

                ViewData["service"] = theProvider.professionName;
                ViewData["zip"] = theProvider.Zipcode;
                ViewData["city"] = theProvider.City;
                ViewData["State"] = theProvider.State;
                HttpContext.Session.Remove("CustomerId");
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
        public IActionResult RequestService(int providerId)
        {

            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return Unauthorized();



            bool alreadyRequested = _context.Notifications.Any(n =>
                n.InitiatorType == "Customer" &&
                n.InitiatorId == customerId.Value &&
                n.TargetType == "Provider" &&
                n.TargetId == providerId &&
                n.IsAccepted == false 
              
            );

            if (alreadyRequested)
                return BadRequest("You already requested this service.");

            // ✅ Ensure conversation exists
            var convoExists = _context.Conversations.Any(c =>
                c.CustomerId == customerId.Value &&
                c.ProviderId == providerId
            );

            if (!convoExists)
            {
                _context.Conversations.Add(new Conversation
                {
                    CustomerId = customerId.Value,
                    ProviderId = providerId
                });
            }

            // ✅ Create the notification correctly
            _context.Notifications.Add(new Notification
            {
                InitiatorType = "Customer",
                InitiatorId = customerId.Value,
                TargetType = "Provider",
                TargetId = providerId,
                IsRead = false,
                IsAccepted = false,
                CreatedAt = DateTime.UtcNow,
            });

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


            var existingNotification = _context.Notifications
            .FirstOrDefault(n =>
                n.InitiatorType == "Provider" &&
                n.InitiatorId == providerId.Value &&
                n.TargetType == "Customer" &&
                n.TargetId == customerId
            );

            // 🚫 If already accepted → DO NOTHING
            if (existingNotification != null && existingNotification.IsAccepted)
            {
                return Ok(); // silently ignore, conversation already exists
            }

            // 🚫 If already pending → DO NOTHING
            if (existingNotification != null && !existingNotification.IsAccepted)
            {
                return BadRequest("You already offered your service.");
            }


            var notification = new Notification
            {
                InitiatorType = "Provider",
                InitiatorId = providerId.Value,
                TargetType = "Customer",
                TargetId = customerId,
                IsRead = false,
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
                    return View(new List<NotificationViewModel>());
                }

                

                var notifications = query
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

                // 🔹 Mark notifications as read
                notifications.ForEach(n => n.IsRead = true);

                // 🔹 ALSO mark messages as read (THIS IS THE FIX)
                if (customerId != null)
                {
                    var unreadMessages = _context.Messages
                        .Where(m => m.RecipientType == "Customer" &&
                                    m.RecipientId == customerId.Value &&
                                    !m.IsRead)
                        .ToList();

                    unreadMessages.ForEach(m => m.IsRead = true);
                }
                else if (providerId != null)
                {
                    var unreadMessages = _context.Messages
                        .Where(m => m.RecipientType == "Provider" &&
                                    m.RecipientId == providerId.Value &&
                                    !m.IsRead)
                        .ToList();

                    unreadMessages.ForEach(m => m.IsRead = true);
                }

                _context.SaveChanges();


                bool isCustomer = customerId != null;

                var model = notifications.Select(n =>
                {
                    string contactName = "";
                    string contactPhone = "";
                    string contactEmail = "";

                    if (n.InitiatorType == "Customer")
                    {
                        var c = _context.Customer.FirstOrDefault(x => x.Id == n.InitiatorId);
                        if (c != null)
                        {
                            contactName = c.Name;
                            contactPhone = c.Phone;
                            contactEmail = c.Email;
                        }
                    }
                    else
                    {
                        var p = _context.Provider.FirstOrDefault(x => x.Id == n.InitiatorId);
                        if (p != null)
                        {
                            contactName = p.Name;
                            contactPhone = p.Phone;
                            contactEmail = p.Email;
                        }
                    }

                    // 🔹 FETCH MESSAGES ONLY IF ACCEPTED
                    var messages = new List<MessageViewModel>();

                    if (n.IsAccepted)
                    {
                        if (isCustomer)
                        {
                            messages = _context.Messages
                                .Where(m =>
                                    (m.SenderType == "Customer" && m.SenderId == customerId.Value &&
                                    m.RecipientType == "Provider" && m.RecipientId == n.InitiatorId)
                                || (m.SenderType == "Provider" && m.SenderId == n.InitiatorId &&
                                    m.RecipientType == "Customer" && m.RecipientId == customerId.Value)
                                )
                                .OrderBy(m => m.CreatedAt)
                                .Select(m => new MessageViewModel
                                {
                                    SenderName = m.SenderType == "Customer"
                                        ? _context.Customer.First(x => x.Id == m.SenderId).Name
                                        : _context.Provider.First(x => x.Id == m.SenderId).Name,
                                    Text = m.Text,
                                    CreatedAt = m.CreatedAt
                                })
                                .ToList();
                        }
                        else
                        {
                            messages = _context.Messages
                                .Where(m =>
                                    (m.SenderType == "Provider" && m.SenderId == providerId.Value &&
                                    m.RecipientType == "Customer" && m.RecipientId == n.InitiatorId)
                                || (m.SenderType == "Customer" && m.SenderId == n.InitiatorId &&
                                    m.RecipientType == "Provider" && m.RecipientId == providerId.Value)
                                )
                                .OrderBy(m => m.CreatedAt)
                                .Select(m => new MessageViewModel
                                {
                                    SenderName = m.SenderType == "Customer"
                                        ? _context.Customer.First(x => x.Id == m.SenderId).Name
                                        : _context.Provider.First(x => x.Id == m.SenderId).Name,
                                    Text = m.Text,
                                    CreatedAt = m.CreatedAt
                                })
                                .ToList();
                        }
                    }

                    return new NotificationViewModel
                    {
                        NotificationId = n.Id,
                        InitiatorId = n.InitiatorId,
                        InitiatorName = contactName,
                        Service = isCustomer
                            ? _context.Provider.FirstOrDefault(p => p.Id == n.InitiatorId)?.professionName
                            : _context.Customer.FirstOrDefault(c => c.Id == n.InitiatorId)?.Whatservice,
                        viewerIsCustomer = isCustomer,
                        IsAccepted = n.IsAccepted,
                        Phone = contactPhone,
                        Email = contactEmail,
                        Messages = messages
                    };
                }).ToList();

                return View(model);
            }





        public int GetUnreadNotificationCount()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            int? providerId = HttpContext.Session.GetInt32("ProviderId");

            int count = 0;

            if (customerId != null)
            {
                // 🔹 Unread service notifications
                int notificationCount = _context.Notifications.Count(n =>
                    n.TargetType == "Customer" &&
                    n.TargetId == customerId.Value &&
                    !n.IsRead
                );

                // 🔹 Unread messages
                int messageCount = _context.Messages.Count(m =>
                    m.RecipientType == "Customer" &&
                    m.RecipientId == customerId.Value &&
                    !m.IsRead
                );

                count = notificationCount + messageCount;
            }
            else if (providerId != null)
            {
                // 🔹 Unread service notifications
                int notificationCount = _context.Notifications.Count(n =>
                    n.TargetType == "Provider" &&
                    n.TargetId == providerId.Value &&
                    !n.IsRead
                );

                // 🔹 Unread messages
                int messageCount = _context.Messages.Count(m =>
                    m.RecipientType == "Provider" &&
                    m.RecipientId == providerId.Value &&
                    !m.IsRead
                );

                count = notificationCount + messageCount;
            }

            return count;
        }




        [HttpPost]
        public IActionResult AcceptNotification(int notificationId)
        {
            var notification = _context.Notifications
                .FirstOrDefault(n => n.Id == notificationId);

            if (notification == null)
                return NotFound();

            notification.IsAccepted = true;

            _context.Notifications.Add(new Notification
            {
                InitiatorType = notification.TargetType,
                InitiatorId = notification.TargetId,
                TargetType = notification.InitiatorType,
                TargetId = notification.InitiatorId,
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();

            return RedirectToAction("Notifications");
        }


        [HttpGet]
        public IActionResult CustomerSearch(
            string service,
            string city,
            string state,
            string zip,
            int? radius)
        {
            var query = _context.Provider.AsQueryable();

            // STEP 1: filter by service if specified
            if (!string.IsNullOrWhiteSpace(service))
                query = query.Where(p => p.professionName.ToLower().Contains(service.ToLower()));

            // STEP 2: radius filtering only if radius AND at least one location is specified
            if (radius.HasValue)
            {
                int r = radius.Value;

                if (!string.IsNullOrWhiteSpace(zip) && r <= 5)
                {
                    query = query.Where(p => p.Zipcode == zip);
                }
                else if (!string.IsNullOrWhiteSpace(city) && r <= 15)
                {
                    query = query.Where(p => p.City.ToLower() == city.ToLower());
                }
                else if (!string.IsNullOrWhiteSpace(state) && r <= 25)
                {
                    query = query.Where(p => p.State.ToLower() == state.ToLower());
                }
                // If radius is set but no location info is given, ignore radius
            }

            var providers = query.ToList();

            if (!providers.Any())
            {
                ViewData["cantfind"] = "Can't find any providers";
            }

            var vm = new CustomerPageViewModel
            {
                Providers = providers
            };

            ViewBag.NotificationCount = GetUnreadNotificationCount();

            ViewData["service"] = service;
            ViewData["city"] = city;
            ViewData["state"] = state;
            ViewData["zip"] = zip;
            ViewData["radius"] = radius;

            ViewData["BodyClass"] = "homepage-background";
            return View("Customerpage", vm);
        }




        [HttpGet]
        public IActionResult ProviderSearch(
            string service,
            string city,
            string state,
            string zip,
            int? radius)
        {
            var query = _context.Customer.AsQueryable();

            // STEP 1: filter by service if specified
            if (!string.IsNullOrWhiteSpace(service))
                query = query.Where(c => c.Whatservice.ToLower().Contains(service.ToLower()));

            // STEP 2: radius filtering only if radius AND at least one location is specified
            if (radius.HasValue)
            {
                int r = radius.Value;

                if (!string.IsNullOrWhiteSpace(zip) && r <= 5)
                {
                    query = query.Where(c => c.Zipcode == zip);
                }
                else if (!string.IsNullOrWhiteSpace(city) && r <= 15)
                {
                    query = query.Where(c => c.City.ToLower() == city.ToLower());
                }
                else if (!string.IsNullOrWhiteSpace(state) && r <= 25)
                {
                    query = query.Where(c => c.State.ToLower() == state.ToLower());
                }
                // If radius is set but no location info is given, ignore radius
            }

            var customers = query.ToList();

            if (!customers.Any())
            {
                ViewData["cantfind"] = "Can't find any customers";
            }

            var vm = new CustomerPageViewModel
            {
                CustomerList = customers
            };

            ViewBag.NotificationCount = GetUnreadNotificationCount();

            ViewData["service"] = service;
            ViewData["city"] = city;
            ViewData["state"] = state;
            ViewData["zip"] = zip;
            ViewData["radius"] = radius;

            ViewData["BodyClass"] = "homepage-background";
            return View("Providerpage", vm);
        }

        [HttpPost]
        public IActionResult SendMessage(int recipientId, string recipientType, string messageText)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            int? providerId = HttpContext.Session.GetInt32("ProviderId");

            if ((customerId == null && providerId == null) || string.IsNullOrWhiteSpace(messageText))
            {
                return BadRequest("Invalid message or user not logged in.");
            }

            // Determine sender
            string senderType;
            int senderId;

            if (customerId != null)
            {
                senderType = "Customer";
                senderId = customerId.Value;
            }
            else
            {
                senderType = "Provider";
                senderId = providerId.Value;
            }

            // Create message
            var msg = new Message
            {
                SenderId = senderId,
                SenderType = senderType,
                RecipientId = recipientId,
                RecipientType = recipientType,
                Text = messageText,
                CreatedAt = DateTime.UtcNow,
                IsRead = false // ✅ IMPORTANT: mark as unread
            };

            _context.Messages.Add(msg);

            /*
            * IMPORTANT:
            * Do NOT create a service notification here.
            * Service notifications are ONLY for requests & acceptance.
            * Messaging after acceptance should never re-create them.
            */

            _context.SaveChanges();

            return Ok(new { success = true });
        }






    }
}