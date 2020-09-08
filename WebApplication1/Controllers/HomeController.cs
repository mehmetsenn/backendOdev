using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using WebApplication1.Models;
using WebApplication1.Models.DB;

namespace WebApplication1.Controllers
{

    public class HomeController : BaseController
    {
        private static Random random = new Random();


        [Route("/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [Route("/Dashboard")]
        public IActionResult Dashboard()
        {
            List<OnlineUsers> onlineUsers = context.OnlineUsers.ToList();
            foreach (var offlineuser in onlineUsers)
            {
                TimeSpan time = ((TimeSpan)(DateTime.Now - offlineuser.LoginTime));
                if (time.Days > 1)
                {
                    offlineuser.FkUserId = null;
                    context.OnlineUsers.Update(offlineuser);
                }
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                var error = e.Message;
            }

            List<User> users = new List<User>();
            onlineUsers = context.OnlineUsers.ToList();
            foreach (var onlineuser in onlineUsers)
            {
                if (onlineuser.FkUser != null)
                {
                    User user = context.User.Where(x => x.UserId.Equals(onlineuser.FkUser.UserId)).SingleOrDefault();
                    users.Add(user);
                }
            }
            ViewBag.OnlineUsers = users;

            users = new List<User>();
            List<User> registiratedUsers = context.User.Where(x => x.IsVerified.Equals(true)).ToList();
            foreach (var newRegistiratedUser in registiratedUsers)
            {
                TimeSpan time = (TimeSpan)(DateTime.Now - newRegistiratedUser.RegisterDate);
                if (time.Days < 1)
                {
                    users.Add(newRegistiratedUser);
                }
            }
            ViewBag.NewRegistiratedUsers = users;

            users = new List<User>();
            List<User> notActiveUsers = context.User.Where(x => x.IsVerified.Equals(false)).ToList();
            foreach (var notActiveUser in users)
            {
                TimeSpan time = (TimeSpan)(DateTime.Now - notActiveUser.RegisterDate);
                if (time.Days >= 1)
                {
                    users.Add(notActiveUser);
                }
            }
            ViewBag.NonVerifiedUsers = users;


            List<int> loginDurations = new List<int>();
            List<OnlineUsers> allLogins = context.OnlineUsers.ToList();
            foreach (var OnlineUser in allLogins)
            {
                TimeSpan time = (TimeSpan)(DateTime.Now - OnlineUser.LoginTime);
                if (time.Days < 1)
                {
                    TimeSpan loginTimeDuration = (TimeSpan)(OnlineUser.LoginTime - OnlineUser.LoginAttemp);
                    loginDurations.Add(loginTimeDuration.Seconds);
                }
            }
            if (loginDurations.Count != 0)
            {
                ViewBag.LoginDuration = loginDurations.Average();

            }


            return View();
        }

        [Route("/Activation")]
        public IActionResult Activation()
        {
            return View();
        }


        [Route("HomeController/Register")]
        public IActionResult Register(User user)
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            user.ActivationCode = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());


            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("Mehmet", "mehmet.sen0097@gmail.com"));
            mailMessage.To.Add(new MailboxAddress(user.FirstName + " " + user.LastName, user.Email));
            mailMessage.Subject = "Verification";
            mailMessage.Body = new TextPart("plain")
            {
                Text = "Your activation code is: " + user.ActivationCode
            };

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("mehmet.sen0097@gmail.com", "Password");
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }

            user.IsVerified = false;
            string HashCode = ComputeSha256Hash(user.Password).Substring(0, 20);
            user.Password = HashCode;
            user.RegisterDate = DateTime.Now;
            try
            {
                context.User.Add(user);
                context.SaveChanges();
                ViewBag.Message = null;
            }
            catch (Exception e)
            {
                var error = e.Message;
                ViewBag.Message = "Please fill all fields";
                return View();
            }

            return View();

        }

        [Route("/HomeController/Activate")]
        public IActionResult Activate(string ActivationCode)
        {
            string email = User.Identity.Name;
            User user = context.User.Where(x => x.Email.Equals(email)).SingleOrDefault();

            if (user.IsVerified == false)
            {
                if (user.ActivationCode == ActivationCode)
                {
                    user.IsVerified = true;
                    try
                    {
                        context.User.Update(user);
                        context.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        var error = e.Message;
                        return Redirect("/Activation");
                    }
                    return Redirect("/Dashboard");

                }

                else
                {
                    return Redirect("/Activation");
                }
            }
            else
            {
                return Redirect("/Dashboard");
            }

        }


        [Route("/HomeController/Logout")]
        public async Task<IActionResult> Logout()
            {

                await HttpContext.SignOutAsync();

                return Redirect("/");
            }


        }



}
