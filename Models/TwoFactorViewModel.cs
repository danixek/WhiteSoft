using System.ComponentModel.DataAnnotations;

namespace WhiteSoft.Models
{
   public class TwoFactorViewModel
    {
        public string? SharedKey { get; set; }
        public string? AuthenticatorUri { get; set; }
        public string? QrCodeImageUrl { get; set; }

        // Blastnost pro ověření kódu z Authenticator aplikace
        [Required(ErrorMessage = "Zadejte kód z aplikace.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kód musí mít 6 číslic.")]
        [Display(Name = "Ověřovací kód")]
        public string? Code { get; set; }

        public bool IsSetupMode { get; set; } = false; // true = nastavení, false = jen login

        public bool TOTP { get; set; } = false;
    }
}