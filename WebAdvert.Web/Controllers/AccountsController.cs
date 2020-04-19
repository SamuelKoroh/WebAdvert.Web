using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserManager<CognitoUser> _userManager;
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly CognitoUserPool  _pool;

        public AccountsController(UserManager<CognitoUser> userManager, 
            SignInManager<CognitoUser> signInManager,
            CognitoUserPool pool)
        {
           _userManager = userManager;
           _signInManager = signInManager;
            _pool = pool;
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignUpModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);

                if(user.Status == null)
                {
                    user.Attributes.Add(CognitoAttribute.Name.ToString(), model.Email) ;

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                        return RedirectToAction("Confirm");

                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
                else
                    ModelState.AddModelError("", "User already exists!");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Confirm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if(user != null)
                {
                    var result = await (_userManager as CognitoUserManager<CognitoUser>)
                        .ConfirmSignUpAsync(user, model.Code, false);

                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");

                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }

                ModelState.AddModelError("", "The user does not exists!");
            }


            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LogInModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await(_signInManager as CognitoSignInManager<CognitoUser>)
                        .PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "The user does not exists!");
            }


            return View(model);
        }
    }
}