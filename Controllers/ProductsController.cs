using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteSoft.Models;
using WhiteSoft.Services;

namespace WhiteSoft.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public ProductsController(ApplicationDbContext context, ILogService logService) {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) {
            Console.WriteLine(id);
            var model = _context.Products.Find(id);
            if (model == null) {
                return NotFound();
            }

            try
            {
                _context.Products.Remove(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Produkt byl úspěšně smazán.";
                await _logService.LogAsync("Success", $"Smazání produktu (ID {model.Id} proběhlo úspěšně.", User.Identity?.Name);
                return Redirect(Request.Headers["Referer"].ToString());
            }
            catch {
                await _logService.LogAsync("Warning", $"Produkt (ID {model.Id}) nebyl smazán.", User.Identity?.Name);

                return StatusCode(500, "Chyba serveru. Záznam nebyl smazán.");
            }

        }
    }
    [ApiController]
    [Route("api/products")]
    [Authorize(Roles = "superadmin")]
    public class ProductsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Adds and edits products
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromForm] Product? model, IFormFile? productImage)
        {
            Product? entity;
            Console.WriteLine("Formulář: " + model);
            if (model!.Id == 0)
            {
                Console.WriteLine("Kapacita 0: " + model.MaxCapacity);
                // new product
                entity = new Product
                {
                    Name = model.Name,
                    Type = model.Type,
                    MaxCapacity = model.MaxCapacity,
                    Price = model.Price,
                    IsPinned = model.IsPinned
                };

                _context.Products.Add(entity);
                await _context.SaveChangesAsync();
            }
            else {
                // update of existed product
                entity = await _context.Products.FindAsync(model.Id)
                    ?? throw new InvalidOperationException($"Produkt s ID {model.Id} neexistuje.");

                if (entity == null)
                    return NotFound();

                Console.WriteLine("Kapacita: " + model.MaxCapacity);
                if (model.MaxCapacity != null)
                {
                    entity.MaxCapacity = model.MaxCapacity;
                }

                entity.Name = model.Name;
                entity.Type = model.Type;
                
                entity.Price = model.Price;
                entity.IsPinned = model.IsPinned;
            }

            // Uploaded image
            if ((productImage != null) && productImage.Length > 0)
            {
                var extension = Path.GetExtension(productImage.FileName);
                var folderPath = Path.Combine("wwwroot/img/products");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{entity.Id}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productImage.CopyToAsync(stream);
                }
                entity.ImageUrl = "/img/products/" + fileName;
            }
            _context.Products.Update(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
