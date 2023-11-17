using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
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

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(loginResponseDTO.Token);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(x => x.Type == "unique_name").Value)); // claim type is "name" for <= .NET 6 and "unique_name" for >= .NET 7 
                identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(x=>x.Type=="role").Value)); // JWT specific claim type magic string
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal); 

                HttpContext.Session.SetString(StaticDetails.SessionTokenName, loginResponseDTO.Token);

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
            //HttpContext.Session.SetString(StaticDetails.SessionToken, string.Empty); //used by author, otherwise it is null
            return await Task.Run(() => RedirectToAction("Index","Home"));
        }

        public async Task<IActionResult> AccessDenied()
        {
            return await Task.Run(() => View());
        }
    }
}
