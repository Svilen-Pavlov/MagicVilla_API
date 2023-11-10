using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MagicVilla_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IVillaService _villaService;

        public HomeController(IMapper mapper, IVillaService villaService)
        {
            this._mapper = mapper;
            this._villaService = villaService;
        }
        public async Task<IActionResult> Index()
        {
            List<VillaDTO> list = new List<VillaDTO>();

            var response = await _villaService.GetAllAsync<APIResponse>(HttpContext.Session.GetString(StaticDetails.SessionTokenName));
            if (response != null && response.IsSuccess )
            {
                list = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result));
            }

            return View(list);
        }

       
    }
}