using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace MagicVilla_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public AuthController(IMapper mapper, IAuthService authService)
        {
            this._mapper = mapper;
            this._authService = authService;
        }

        public async Task<IActionResult> Login()
        {
            LoginRequestDTO obj = new();
            return await Task.Run(() => View(obj));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO model)
        {
            var response = await _authService.LoginAsync<APIResponse>(model);
            if (response != null && response.IsSuccess)
            {
                LoginResponseDTO loginResponseDTO = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(response.Result));

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, loginResponseDTO.User.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, loginResponseDTO.User.Role));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal); //lector also passes the scheme as first param, see if any diferences

                HttpContext.Session.SetString(StaticDetails.SessionToken, loginResponseDTO.Token);

                TempData["success"] = "Login successful";
                return await Task.Run(() => RedirectToAction("Index", "Home"));
            }

            ModelState.AddModelError("CustomError", response.ErrorMessages.FirstOrDefault());
            TempData["error"] = "Error encountered";
            return await Task.Run(() => View(model));
        }

        public async Task<IActionResult> Register()
        {
            RegistrationRequestDTO obj = new();
            return await Task.Run(() => View(obj));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO model)
        {
            APIResponse response = await _authService.RegisterAsync<APIResponse>(model);
            if (response != null && response.IsSuccess)
            {
                return await Task.Run(() => RedirectToAction("Login"));
            }

            ModelState.AddModelError("CustomError", response.ErrorMessages.FirstOrDefault());
            TempData["error"] = "Error encountered";
            return await Task.Run(() => View(model));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            //HttpContext.Session.SetString(StaticDetails.SessionToken, string.Empty); //used by author
            return await Task.Run(() => RedirectToAction("Index","Home"));
        }

        public async Task<IActionResult> AccessDenied()
        {
            return await Task.Run(() => View());
        }
    }
}
