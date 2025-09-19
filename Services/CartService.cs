using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PojistakNET.Data;
using WhiteSoft.Models;

namespace WhiteSoft.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<CartProductsViewModel>> LoadItemsFromCookieCart(string cartJson)
        {
            var cookieCart = JsonSerializer.Deserialize<List<CartProductsViewModel>>(cartJson);
            if (cookieCart == null || !cookieCart.Any())
                return new List<CartProductsViewModel>();

            var result = new List<CartProductsViewModel>();

            foreach (var item in cookieCart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                result.Add(new CartProductsViewModel
                {
                    ProductId = product.Id,
                    Details = new ProductDetails
                    {
                        Name = product.Name,
                        Quantity = item.Quantity,
                        Price = product.Price,
                        Total = product.Price * item.Quantity,
                        ImageUrl = product.ImageUrl

                    }
                });
            }
            return result;
        }
    }
}
