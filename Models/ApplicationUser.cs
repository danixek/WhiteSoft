using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using WhiteSoft.Handlers;

namespace WhiteSoft.Models
{
    /// <summary>
    /// user model for authentication and authorization
    /// </summary>
    public class ApplicationUser : IdentityUser, ISecurityModel
    {
        [Required, AntiXss]
        public required string FirstName { get; set; }
        [Required, AntiXss]
        public required string LastName { get; set; }
    }
}
