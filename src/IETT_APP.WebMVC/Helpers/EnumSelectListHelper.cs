using Microsoft.AspNetCore.Mvc.Rendering;

namespace IETT_APP.WebMVC.Extensions
{
    public static class EnumSelectListHelper
    {
        public static SelectList ToSelectList<TEnum>() where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new
                {
                    Value = Convert.ToInt32(x),
                    Text = x.ToDisplayName() // senin EnumExtensions içindeki ToDisplayName() metodu
                });

            return new SelectList(values, "Value", "Text");
        }
    }
}