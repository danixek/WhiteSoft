using System.Web;

namespace WhiteSoft.Handlers
{
    public static class Security
    {
        public static string XssProtection(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input
                .Replace("&", "&amp;")   // musí být první
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AntiXssAttribute : Attribute
    {
        // zatím prázdný, jen jako značka
    }
}
