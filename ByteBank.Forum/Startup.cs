using ByteBank.Forum.App_Start.Identity;
using ByteBank.Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(ByteBank.Forum.Startup))]

namespace ByteBank.Forum
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.CreatePerOwinContext<DbContext>(() =>
                new IdentityDbContext<UserApplication>("DefaultConnection"));

            builder.CreatePerOwinContext<IUserStore<UserApplication>>(
                (opcoes, contextoOwin) =>
                {
                    var dbContext = contextoOwin.Get<DbContext>();
                    return new UserStore<UserApplication>(dbContext);
                });

            builder.CreatePerOwinContext<UserManager<UserApplication>>(
                (opcoes, contextoOwin) =>
                {
                    var userStore = contextoOwin.Get<IUserStore<UserApplication>>();
                    var userManager = new UserManager<UserApplication>(userStore);

                    var userValidator = new UserValidator<UserApplication>(userManager)
                    {
                        RequireUniqueEmail = true
                    };

                    userManager.UserValidator = userValidator;

                    // Utilização da classe pronta de PasswordValidator que implementa IIdentityValidator
                    userManager.PasswordValidator = new PasswordValidator()
                    {
                        RequiredLength = 8,
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireUppercase = true,
                        RequireNonLetterOrDigit = true
                    };

                    userManager.EmailService = new MailService();

                    // Criação de um dataprotectionprovider para ser o do token??
                    var dataProtectionProvider = opcoes.DataProtectionProvider;
                    var dataProtectionProviderCreated = dataProtectionProvider.Create("ByteBank");

                    userManager.UserTokenProvider = new DataProtectorTokenProvider<UserApplication>(dataProtectionProviderCreated);

                    userManager.MaxFailedAccessAttemptsBeforeLockout = 3;
                    userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    userManager.UserLockoutEnabledByDefault = true;


                    return userManager;
                });

            builder.CreatePerOwinContext<SignInManager<UserApplication, string>>(
                (opcoes, contextoOwin) =>
                {
                    var userManager = contextoOwin.Get<UserManager<UserApplication>>();

                    var SignInManager = new SignInManager<UserApplication, string>
                    (
                        userManager,
                        contextoOwin.Authentication
                    );

                    return SignInManager;
                });

            builder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });

        }
    }
}