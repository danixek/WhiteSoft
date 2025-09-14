using WhiteSoft.Handlers;

namespace WhiteSoft.Models
{
    public class AdminUserViewModel : ISecurityModel
    {
        public required string Id { get; set; }
        [AntiXss]
        public required string FirstName { get; set; }
        [AntiXss]
        public required string LastName { get; set; }
        [AntiXss]
        public required string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}