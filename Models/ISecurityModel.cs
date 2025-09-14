using WhiteSoft.Handlers;

namespace WhiteSoft.Models
{
    public interface ISecurityModel { }
    public static class SecurityModelExtensions
    {
        public static void ApplyXssProtection(this ISecurityModel model)
        {
            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(string) &&
                    Attribute.IsDefined(prop, typeof(AntiXssAttribute)))
                {
                    string value = (string?)prop.GetValue(model) ?? "";
                    prop.SetValue(model, Security.XssProtection(value));
                }
            }
        }
    }
}
