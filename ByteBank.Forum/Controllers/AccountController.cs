using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace ByteBank.Forum.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<UserApplication> _userManager;
        public UserManager<UserApplication> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _userManager = contextOwin.GetUserManager<UserManager<UserApplication>>();
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }

        private SignInManager<UserApplication, string> _signInManager;
        public SignInManager<UserApplication, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _signInManager = contextOwin.GetUserManager<SignInManager<UserApplication, string>>();
                }
                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        public IAuthenticationManager _authenticationManager
        {
            get
            {
                var contextoOwin = Request.GetOwinContext();
                return contextoOwin.Authentication;
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel modelo)
        {
            // Validação da classe de modelo, se tudo está ok
            if (ModelState.IsValid)
            {
                // Instanciação de uma nova classe UserApplication com os parâmetros pegos da RegisterViewModel
                var newUser = new UserApplication
                {
                    Email = modelo.Email,
                    UserName = modelo.UserName,
                    FullName = modelo.FullName
                };

                // Buscar usuário por email cadastrado no banco de dados
                var user = await UserManager.FindByEmailAsync(modelo.Email);

                // Inclusão de um novo usuário no banco de dados
                var result = await UserManager.CreateAsync(newUser, modelo.Password);

                // Retorno de um booleano de sucesso ou não da operação
                if (result.Succeeded)
                {
                    // Enviar o email de confirmação
                    await SendMailConfirmationAsync(newUser);

                    // Retornar para uma página para confirmar
                    return View("WaitingConfirmation");
                }
                else
                    AddErrors(result);
            }

            // Caso de erro devolver uma mensagem de erro e o objeto preenchido
            return View(modelo);
        }

        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return View("Error");

            var resultado = await UserManager.ConfirmEmailAsync(userId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");

            return View("Error");
        }

        [HttpGet]
        public ActionResult Login() => View();

        [HttpPost]
        public async Task<ActionResult> Login(AccountLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Realizar login pelo Identity
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return PasswordOrUserInvalid();
                }
                var signInResult = await SignInManager.PasswordSignInAsync
                (
                    user.UserName,
                    model.Password,
                    isPersistent: model.KeepLoggedIn,
                    shouldLockout: true
                );

                switch (signInResult)
                {
                    case SignInStatus.Success:
                        if (!user.EmailConfirmed)
                        {
                            // Método para deslogar o usuário
                            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                            return View("WaitingConfirmation");
                        }
                        return RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        // Retorna um booleano contendo a informação se a senha está correta ou não
                        var passwordCorrect = await UserManager.CheckPasswordAsync(user, model.Password);

                        if (!passwordCorrect)
                            return PasswordOrUserInvalid();
                        return UserLockedOut();

                    default:
                        return PasswordOrUserInvalid();

                }
            }

            // Algo de errado não está certo
            return View(model);
        }
        [HttpGet]
        public ActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(AccountForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await UserManager.FindByEmailAsync(model.Email);

                if(user != null)
                {
                    // Gerar token de reset de senha
                    var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                    // Gerar a url para o usuário

                    var linkOfCallback = Url.Action(
                                                    "ConfirmResetPassword",
                                                    "Account",
                                                    new {userId = user.Id, token = token },
                                                    Request.Url.Scheme
                                                    );

                    // Enviar o email

                    await UserManager.SendEmailAsync(
                        user.Id,
                        "Byte Bank - Alteração de senha",
                        $"Clique aqui {linkOfCallback} para alterar sua senha");

                    return View("ConfirmEmailPassword");
                }

            }

            return View("ConfirmEmailPassword");

        }

        [HttpGet]
        public ActionResult ConfirmResetPassword(string userId, string token)
        {
            var model = new AccountConfirmResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmResetPasswordAsync(AccountConfirmResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                // Verifica token recebido
                if (model.UserId == null || model.Token == null)
                    return View("Error");



                // Trocar a senha
                var resultReset = await UserManager.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);

                if (resultReset.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                AddErrors(resultReset);

            }
            return View();
        }
        private ActionResult PasswordOrUserInvalid()
        {
            ModelState.AddModelError("", "User or password incorrect");
            return View("Login");
        }
        private ActionResult UserLockedOut()
        {
            ModelState.AddModelError("", "Account blocked, try again later");
            return View("Login");
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        public async Task SendMailConfirmationAsync(UserApplication userapplication)
        {
            // Gera um token para confirmação do email
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(userapplication.Id);

            // Gera um link de confirmação para o usuário que acabou de se cadastrar
            var linkOfCallback = Url.Action("ConfirmEmail", "Account", new { userId = userapplication.Id, token = token },
                Request.Url.Scheme);


            await UserManager.SendEmailAsync(
                userapplication.Id,
                "Byte Bank - Confirmacao email",
                $"Bem vindo ao Byte Bank, clique aqui {linkOfCallback} para confirmar seu email");
        }

        private void AddErrors(IdentityResult result)
        {
            // Retorna todos os erros através da model state
            foreach (var erro in result.Errors)
                ModelState.AddModelError("", erro);
        }
    }
}
