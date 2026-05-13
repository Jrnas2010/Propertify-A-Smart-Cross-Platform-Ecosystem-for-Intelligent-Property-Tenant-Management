using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Propertify.Web.Controllers
{
    public class LanguageController : Controller
    {
        // تم تغيير الاسم هنا ليتطابق مع طلب المتصفح في الصورة
        [HttpGet] // لاحظ أن الرابط في الصورة يستخدم GET (لأنه يحتوي على علامة استفهام)
        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home");

            return LocalRedirect(returnUrl);
        }
    }
}