using GDR.Lib;
using GDR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GDR.Repositories;
using Microsoft.AspNetCore.Http;

namespace GDR.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthentication authentication;
        private readonly ILogRepository log;

        private readonly string controllerName = "Authentication";

        public AuthenticationController(IAuthentication authentication, ILogRepository log)
        {
            this.authentication = authentication;
            this.log = log;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel login, string returnUrl)
        {
            List<Claim> claims = new List<Claim>();

            if (ModelState.IsValid)
            {

                //Verifica se está sendo usado o usuário administrador, caso não esteja verifica no AD
                if (!authentication.VerifyAdminUser(login))
                {
                    try
                    {

                        claims = await authentication.VerifyAccess(login);
                        if (claims == null || claims.Count == 0)
                        {
                            ModelState.AddModelError("", "Usuário ou senha inválida");
                            return this.View(login);
                        }
                    }
                    catch(Exception error) {
                        ModelState.AddModelError("Username", "Erro ao acessar ao acessar o servidor LDAP");
                        await log.SaveLogApplicationError(controllerName, "Erro ao logar: " + error.ToString());
                        return View();
                    }

                }
                else {
                    claims = authentication.GetClaimType(Models.AccessTypeEnum.Administration);
                }

                // Create the identity from the user info
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, login.Username));
                identity.AddClaim(new Claim(ClaimTypes.Name, login.Username));
                identity.AddClaims(claims);

                // Authenticate using the identity
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = login.RememberMe });
                await log.SaveLogApplicationMessage(controllerName, "Login realizado com sucesso");

                if (returnUrl != null) {
                    return View(returnUrl);
                }

                return this.RedirectToAction("Index", "Home");
            }

            return this.View(login);

        }

        [Authorize(Policy = "Read")]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            //log.SaveLogApplicationMessage(controllerName, User.Identity.Name, "LogOff realizado com sucesso");
            return this.RedirectToAction("Login", "Authentication");
        }
    }
}