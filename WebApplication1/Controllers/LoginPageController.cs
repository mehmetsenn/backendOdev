using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using WebApplication1.Models.DB;

namespace WebApplication1.Controllers
{
    public class LoginPageController : BaseController
    {

        [Route("/")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("/LoginPageController/Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if(email!=null && password != null)
            {
                DateTime loginAttemp = DateTime.Now;

                User user = context.User.Where(x => x.Email.Equals(email)).SingleOrDefault();
                var inputpassword = ComputeSha256Hash(password).Substring(0, 20);
                if (user != null)
                {
                    if (inputpassword == user.Password)
                    {
                        var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Name, email),

                };

                        var userIdentity = new ClaimsIdentity(claims, "login");
                        var userdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                        var name = User.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                        await HttpContext.SignInAsync(principal, new AuthenticationProperties { IsPersistent = false });

                        var Id = user.UserId;
                        OnlineUsers onlineuser = new OnlineUsers
                        {
                            FkUserId = Id,
                            LoginAttemp = loginAttemp,
                            LoginTime = DateTime.Now
                        };
                        OnlineUsers oldLogin = context.OnlineUsers.Where(x => x.FkUserId.Equals(Id)).SingleOrDefault();
                        if (oldLogin != null)
                        {
                            oldLogin.FkUserId = null;
                        }

                        try
                        {
                            if (oldLogin != null)
                            {
                                oldLogin.FkUserId = null;
                                context.OnlineUsers.Update(oldLogin);
                            }
                            context.OnlineUsers.Add(onlineuser);
                            context.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            var error = e.Message;
                        }

                        if (user.IsVerified == true)
                        {
                            return Redirect("/Dashboard");
                        }
                        else
                        {
                            return Redirect("/Activation");
                        }

                    }
                    else
                    {
                        ViewBag.Message = "Wrong password or email";
                        return View();
                    }
                }
                
            }
            ViewBag.Message = "Wrong password or email";
            return View();

        }

        [Route("/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [Route("/LoginPageController/ForgotPassword")]
        public IActionResult ForgotPassword(string email)
        {
            if (email != null)
            {
                User user = context.User.Where(x => x.Email.Equals(email)).SingleOrDefault();
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("Mehmet", "mehmet.sen0097@gmail.com"));
                mailMessage.To.Add(new MailboxAddress(user.FirstName + " " + user.LastName, user.Email));
                mailMessage.Subject = "Verification";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Your password reset link: " + "https://localhost:44345/LoginPageController/ChangePassword/" + user.ActivationCode
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Authenticate("mehmet.sen0097@gmail.com", "Password");
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                return ChangePassword(user.ActivationCode);
            }

            return View();
        }

        [Route("/LoginPageController/ChangePassword/{ActivationCode}")]
        public IActionResult ChangePassword(string ActivationCode)
        {
            return View();
        }

        [Route("/LoginPageController/ChangePassword")]
        public IActionResult ChangePassword(string email, string password, string confirmPassword)
        {
            if (password == confirmPassword)
            {
                User user = context.User.Where(x => x.Email.Equals(email)).SingleOrDefault();
                string HashCode = ComputeSha256Hash(password).Substring(0, 20);
                user.Password = HashCode;
                try
                {
                    context.User.Update(user);
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    var error = e.Message;
                }

            }
            else
            {
                ViewBag.Message = "Passwords didn't match";
            }
            return View();
        }
    }
}
