using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace MagicVilla_Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IVillaService _villaService;

        public VillaController(IMapper mapper, IVillaService villaService)
        {
            this._mapper = mapper;
            this._villaService = villaService;
        }
        public async Task<IActionResult> IndexVilla(int pageSize = 5, int pageNumber = 1)
        {
            List<VillaDTO> list = new List<VillaDTO>();
            var response = await _villaService.GetAllAsync<APIResponse>(HttpContext.Session.GetString(StaticDetails.SessionTokenName), pageSize, pageNumber);

            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result));
            }
            var paginatedResults = new PaginatedListDTO<VillaDTO>(list, list.Count, pageNumber, pageSize); // fix this for total results

            return await Task.Run(() => View(paginatedResults));
        }

        public async Task<IActionResult> CreateVilla()
        {
            return await Task.Run(() => View());
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVilla(VillaCreateDTO model)
        {

            if (ModelState.IsValid)
            {
                var response = await _villaService.CreateAsync<APIResponse>(model, HttpContext.Session.GetString(StaticDetails.SessionTokenName));
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Villa created succesfully";
                    return await Task.Run(() => RedirectToAction(nameof(IndexVilla)));
                }
            }

            TempData["error"] = "Error encountered";
            return await Task.Run(() => View(model));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateVilla(int villaId)
        {
            var user = HttpContext.User.Claims;// TO DO DELETE
            var response = await _villaService.GetAsync<APIResponse>(villaId, HttpContext.Session.GetString(StaticDetails.SessionTokenName));
            if (response != null && response.IsSuccess)
            {
                VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
                return await Task.Run(() => View(_mapper.Map<VillaUpdateDTO>(model)));
            }
            return await Task.Run(() => NotFound());
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVilla(VillaUpdateDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await _villaService.UpdateAsync<APIResponse>(model, HttpContext.Session.GetString(StaticDetails.SessionTokenName));
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Villa updated succesfully";
                    return await Task.Run(() => RedirectToAction(nameof(IndexVilla)));
                }
            }

            TempData["error"] = "Error encountered";
            return await Task.Run(() => View(model));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteVilla(int villaId)
        {
            var response = await _villaService.GetAsync<APIResponse>(villaId, HttpContext.Session.GetString(StaticDetails.SessionTokenName));
            if (response != null && response.IsSuccess)
            {
                VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
                return await Task.Run(() => View(model));
            }
            return await Task.Run(() => NotFound());
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVilla(VillaDTO model)
        {
            var response = await _villaService.DeleteAsync<APIResponse>(model.Id, HttpContext.Session.GetString(StaticDetails.SessionTokenName));
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Villa deleted succesfully";
                return await Task.Run(() => RedirectToAction(nameof(IndexVilla)));
            }

            TempData["error"] = "Error encountered";
            return await Task.Run(() => View(model));
        }
    }
}
