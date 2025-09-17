using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WhiteSoft.Models;

namespace WhiteSoft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        [HttpPost("AddToCart")] // pevná URL pro form i JS
        public IActionResult AddToCartApi([FromBody] CartItem? item, [FromForm] CartItem? formItem)
        {
            var cartItem = item ?? formItem;
            if (cartItem == null) return BadRequest("Chybí položka.");

            List<CartItem> cart;
            if (Request.Cookies.TryGetValue("Cart", out var cartJson))
                cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            else
                cart = new List<CartItem>();

            var existing = cart.FirstOrDefault(c => c.ProductId == cartItem.ProductId);
            if (existing != null)
                existing.Quantity += cartItem.Quantity;
            else
                cart.Add(cartItem);

            var newJson = JsonSerializer.Serialize(cart);
            Response.Cookies.Append("Cart", newJson, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true,
                IsEssential = true
            });

            return Ok(cart);
        }

        // Košík - Načte položky z košíku (z cookies) do API
        [HttpGet("cart")]
        public async Task<IActionResult> GetCartItems()
        {
            if (!Request.Cookies.TryGetValue("Cart", out var cartJson))
                return Ok(new List<object>()); // prázdný košík

            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            if (cartItems == null || !cartItems.Any())
                return Ok(new List<object>());

            var result = new List<object>();

            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                result.Add(new
                {
                    product.Id,
                    product.Name,
                    product.Price,
                    product.ImageUrl,
                    item.Quantity
                });
            }

            return Ok(result);
        }


        // Zpracuje objednávku z košíku
        [HttpPost]
        public async Task<IActionResult> Checkout([FromForm] string? customerName, [FromForm] string? customerEmail)
        {
            string finalName;
            string finalEmail;

            if (User.Identity!.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return BadRequest("Uživatel není přihlášen.");
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return BadRequest("Uživatel nenalezen.");

                finalName = user.UserName!;
                finalEmail = user.Email!;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerEmail))
                    return BadRequest("Jméno a email jsou povinné.");

                // základní validace emailu
                if (!customerEmail.Contains("@"))
                    return BadRequest("Neplatný email.");

                finalName = customerName;
                finalEmail = customerEmail;
            }

            if (!Request.Cookies.TryGetValue("Cart", out var cartJson))
                return BadRequest("Košík je prázdný.");

            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            if (cartItems == null || !cartItems.Any())
                return BadRequest("Košík je prázdný.");

            var order = new Order
            {
                CustomerName = finalName,
                CustomerEmail = finalEmail,
                CreatedAt = DateTime.UtcNow,
                Total = 0
            };

            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return NotFound($"Produkt s ID {item.ProductId} nenalezen.");

                // kontrola dostupnosti / kapacity
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

            Response.Cookies.Delete("Cart");

            return Ok(order);
        }

    }
}
// MVC controller pro view
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    [Route("Index")]
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }
    public IActionResult Eshop()
    {
        return View(); // vrátí Views/Orders/Eshop.cshtml
    }
    // Vloží položku do košíku (do cookies)
    [HttpPost]
    public IActionResult AddToCart(CartItem item)
    {
        // Zkusí načíst existující košík z cookies
        List<CartItem> cart;
        if (Request.Cookies.TryGetValue("Cart", out var cartJson))
        {
            cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }
        else
        {
            cart = new List<CartItem>();
        }

        // Najde, jestli už produkt v košíku je a přičte množství
        var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }

        // Uloží zpět do cookies
        var newJson = JsonSerializer.Serialize(cart);
        Response.Cookies.Append("Cart", newJson, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            HttpOnly = true,
            IsEssential = true
        });

        return Ok(cart); // nebo RedirectToAction("Index"), podle potřeby
    }

    public IActionResult CartView()
    {
        // Pokud je uživatel přihlášený, můžeme předvyplnit jméno a email ve view
        string? userName = null;
        string? userEmail = null;

        if (User.Identity!.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = _userManager.FindByIdAsync(userId).Result;
                if (user != null)
                {
                    userName = user.UserName;
                    userEmail = user.Email;
                }
            }
        }

        ViewData["UserName"] = userName;
        ViewData["UserEmail"] = userEmail;

        return View();
    }

}
