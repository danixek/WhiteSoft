using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using WhiteSoft.Models;
using WhiteSoft.Services;
using X.PagedList;

namespace WhiteSoft.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersApiController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        // Index page of eshop
        [HttpGet("eshop")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
        // pinned products on main page
        [HttpGet("pinned")]
        public async Task<IActionResult> GetPinnedProducts()
        {
            var pinned = await _context.Products
                .Where(p => p.IsPinned)
                .ToListAsync();

            return Ok(pinned);
        }

        [HttpPost("AddToCart")]
        public IActionResult AddToCartApi( /* [FromBody] CookieCart? selectedProductByJavascript , */ [FromForm] CookieCart? selectedProductByForm)
        {
            // Here I tested javascript form with fallback to static <form>
            var selectedProduct = /* selectedProductByJavascript ?? */ selectedProductByForm;
            if (selectedProduct == null) return BadRequest("Chybí položka.");

            List<CookieCart> cookieCart;
            if (Request.Cookies.TryGetValue("Cart", out var cartJson))
                cookieCart = JsonSerializer.Deserialize<List<CookieCart>>(cartJson) ?? new List<CookieCart>();
            else
                cookieCart = new List<CookieCart>();

            var product = cookieCart.FirstOrDefault(c => c.ProductId == selectedProduct.ProductId);
            if (product != null)
            {
                Console.WriteLine("quantity: " + product.Quantity);
                Console.WriteLine("selected quantity: " + selectedProduct.Quantity);
                product.Quantity += selectedProduct.Quantity;
            }
            else
            {
                Console.WriteLine("Selected product: " + selectedProduct);
                cookieCart.Add(selectedProduct);
            }

            var cookieCartJson = JsonSerializer.Serialize(cookieCart);
            Console.WriteLine("Cookies! " + cookieCartJson);
            Response.Cookies.Append("Cart", cookieCartJson, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = false,  // for Postman and JS
                IsEssential = true,
                Path = "/"
            });

            // resolution depending on where the request came from
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
                return Ok(cookieCart);
            else
                return RedirectToAction("Cart", "Orders");
        }

        // Fallback for cart view
        // Is it actually useful here?
        [HttpGet("cart")]
        public async Task<IActionResult> GetCartItems()
        {
            if (!Request.Cookies.TryGetValue("Cart", out var cartJson))
            {
                return Ok(new List<CookieCart>());
            }
            var result = await _cartService.LoadItemsFromCookieCart(cartJson);

            return Ok(result);
        }

        // It will process order from cart
        // POST: /api/orders/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] OrderCheckoutModel orderInput)
        {
            var cookieCart = orderInput.Cart;
            bool simulation = false;

            // Superadmin can start the order simulation in order management
            if (Request.Headers.ContainsKey("X-Simulation") && User.Identity!.IsAuthenticated)
            {
                if (cookieCart == null || !cookieCart.Any())
                    return BadRequest("Košík je prázdný.");
                
                // simulation of order - get cart from form
                simulation = true;
                cookieCart = orderInput.Cart;
            }
            else
            {
                simulation = false;
                if (User.Identity!.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null) return BadRequest("Uživatel není přihlášen.");
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null) return BadRequest("Uživatel nenalezen.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(orderInput.CustomerName) || string.IsNullOrWhiteSpace(orderInput.CustomerEmail))
                        return BadRequest("Jméno a email jsou povinné.");

                    // elementary validation of email
                    if (!orderInput.CustomerEmail.Contains("@"))
                        return BadRequest("Neplatný email.");

                }
            // Normal user - get cart from cookie
            if (!Request.Cookies.TryGetValue("Cart", out var cartJson))
                return BadRequest("Košík je prázdný.");

            cookieCart = JsonSerializer.Deserialize<List<CookieCart>>(cartJson);
            if (cookieCart == null || !cookieCart.Any())
                return BadRequest("Košík je prázdný.");
            }
            return await ProcessOrder(orderInput.CustomerName, orderInput.CustomerEmail, cookieCart!, simulation);

        }
        // Checkout method continues here
        private async Task<IActionResult> ProcessOrder(string customerName, string customerEmail, List<CookieCart> cart, bool simulation)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            var order = new Order
            {
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone),
                Total = 0
            };

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return NotFound($"Produkt s ID {item.ProductId} nenalezen.");

                if (product.MaxCapacity.HasValue)
                {
                    var currentOrders = await _context.OrderItems
                        .Where(oi => oi.ProductId == product.Id)
                        .SumAsync(oi => oi.Quantity);

                    if (currentOrders + item.Quantity > product.MaxCapacity.Value)
                        return BadRequest($"Produkt {product.Name} není aktuálně k dispozici.");
                }

                var orderItem = new OrderItem
                {
                    ProductName = product.Name,
                    ProductType = product.Type,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    Order = order
                };

                order.Total += orderItem.Price * orderItem.Quantity;
                _context.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            if (!simulation)
                Response.Cookies.Delete("Cart");
            return Ok();
        }

        // GET /api/orders
        [HttpGet]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> GetOrders(int? page)
        {
            int pageSize = 30;
            int pageNumber = page ?? 1;

            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await _context.Orders.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return Ok(new { orders, totalPages, totalItems });
        }
        // GET /api/orders/orderItems
        [HttpGet("orderItems")]
        [Authorize(Roles = "admin,superadmin")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {

            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            return Ok(orderItems);
        }
    }
}
// MVC controller pro view
public class OrdersController : Controller
{
    private readonly CartService _cartService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
    {
        _context = context;
        _userManager = userManager;
        _cartService = cartService;
    }

    [HttpGet]
    [ActionName("Index")]
    [Authorize(Roles = "admin,superadmin")]
    public IActionResult Orders()
    {
        return View();
    }
    public IActionResult Eshop()
    {
        return View();
    }

    public async Task<IActionResult> Cart()
    {
        if (!Request.Cookies.TryGetValue("Cart", out var cartJson))
        {
            return View(new CartUserViewModel());
        }
        var Cart = await _cartService.LoadItemsFromCookieCart(cartJson);

        // If is user logged, it prefilled name and email in view
        var model = new CartUserViewModel
        {
            Cart = Cart
        };

        if (User.Identity!.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = _userManager.FindByIdAsync(userId).Result;
                if (user != null)
                {
                    model.CustomerName = user.FirstName + " " + user.LastName;
                    model.CustomerEmail = user.Email!;
                    model.Cart = Cart;
                }
            }
        }
        return View(model);
    }

}
