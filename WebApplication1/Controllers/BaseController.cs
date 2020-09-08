using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication1.Models;
using WebApplication1.Models.DB;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static string ComputeSha256Hash(string rawData)
        {

            using (SHA256 sha256Hash = SHA256.Create())
            {

                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public WebAppContext context = new WebAppContext();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Path != "/" && 
                filterContext.HttpContext.Request.Path != "/Register" && 
                filterContext.HttpContext.Request.Path != "/HomeController/Register" &&
                filterContext.HttpContext.Request.Path != "/ForgotPassword" &&
                !filterContext.HttpContext.Request.Path.StartsWithSegments("/LoginPageController"))
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    string email = User.Identity.Name;
                    User user = context.User.Where(x => x.Email.Equals(email)).SingleOrDefault();
                    ViewBag.User = user;
                    

                    //if (user.IsVerified == true)
                    //{

                    //}
                    //else
                    //{
                    //    //var url = filterContext.HttpContext.Request.Path;
                    //    //filterContext.Result = new RedirectResult("/Activation");
                    //    //return;
                    //}
                }
                else
                {
                    var url = filterContext.HttpContext.Request.Path;
                    filterContext.Result = new RedirectResult("/");
                    return;
                }

            }
            else
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var url = filterContext.HttpContext.Request.Path;
                    filterContext.Result = new RedirectResult("/Dashboard");
                    return;
                }
            }
        }
    }
}
