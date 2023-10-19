using AutoMapper;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Models.ViewModels;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace MagicVilla_Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;

        public VillaNumberController(IMapper mapper, IVillaNumberService villaNumberService, IVillaService villaService)
        {
            this._mapper = mapper;
            this._villaNumberService = villaNumberService;
            this._villaService = villaService;
        }
        public async Task<IActionResult> IndexVillaNumber()
        {
            List<VillaNumberDTO> list = new List<VillaNumberDTO>();

            var response = await _villaNumberService.GetAsync<APIResponse>();
            if (response.IsSuccess && response.Result != null)
            {
                list = JsonConvert.DeserializeObject<List<VillaNumberDTO>>(Convert.ToString(response.Result));
            }

            return View(list);
        }

        public async Task<IActionResult> CreateVillaNumber()
        {
            VillaNumberCreateVM villaNumberVM = new();
            var response = await _villaService.GetAllAsync<APIResponse>();
            if (response.IsSuccess && response.Result != null)
            {
                villaNumberVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result)).Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVillaNumber(VillaNumberCreateVM model)
        {

            if (ModelState.IsValid)
            {
                var response = await _villaNumberService.CreateAsync<APIResponse>(model.VillaNumber);
                if (response.IsSuccess && response.Result != null)
                {
                    TempData["success"] = "VillaNumber created succesfully";
                    return RedirectToAction(nameof(IndexVillaNumber));
                }
                else
                {
                    if (response.ErrorMessages.Count > 0)
                    {
                        foreach (var errorMessage in response.ErrorMessages)
                        {
                            ModelState.AddModelError("ErrorMessages", errorMessage);
                        }
                    }
                }
            }
            var responseRepopulateList = await _villaService.GetAllAsync<APIResponse>();
            if (responseRepopulateList.IsSuccess && responseRepopulateList.Result != null)
            {
                model.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(responseRepopulateList.Result)).Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }

            TempData["error"] = "Error encountered";
            return View(model);
        }

        public async Task<IActionResult> UpdateVillaNumber(int villaNo) 
        {
            VillaNumberUpdateVM villaNumberVM = new();
            var response = await _villaNumberService.GetAsync<APIResponse>(villaNo);
            if (response.IsSuccess && response.Result != null)
            {
                VillaNumberDTO model = JsonConvert.DeserializeObject<VillaNumberDTO>(Convert.ToString(response.Result));
                villaNumberVM.VillaNumber = _mapper.Map<VillaNumberUpdateDTO>(model);
                var responseVillaList = await _villaService.GetAllAsync<APIResponse>();
                if (responseVillaList.IsSuccess && responseVillaList.Result != null) //lector's if construction assumes there's always gonna be villas. This is better
                {
                    villaNumberVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(responseVillaList.Result)).Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    });
                    return View(villaNumberVM);
                }
                return NotFound();
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVillaNumber(VillaNumberUpdateVM model)
        {
            if (ModelState.IsValid)
            {
                var response = await _villaNumberService.UpdateAsync<APIResponse>(model.VillaNumber);
                if (response.IsSuccess && response.Result != null)
                {
                    TempData["success"] = "VillaNumber updated succesfully";
                    return RedirectToAction(nameof(IndexVillaNumber));
                }
                else
                {
                    if (response.ErrorMessages.Count > 0)
                    {
                        foreach (var errorMessage in response.ErrorMessages)
                        {
                            ModelState.AddModelError("ErrorMessages", errorMessage);
                        }
                    }
                }
            }
            var responseRepopulateList = await _villaService.GetAllAsync<APIResponse>();
            if (responseRepopulateList.IsSuccess && responseRepopulateList.Result != null)
            {
                model.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(responseRepopulateList.Result)).Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }

            TempData["error"] = "Error encountered";
            return View(model);
        }

        public async Task<IActionResult> DeleteVillaNumber(int villaNo)
        {
            VillaNumberDeleteVM villaNumberVM = new();
            var response = await _villaNumberService.GetAsync<APIResponse>(villaNo);
            if (response.IsSuccess && response.Result != null)
            {
                VillaNumberDTO model = JsonConvert.DeserializeObject<VillaNumberDTO>(Convert.ToString(response.Result));
                villaNumberVM.VillaNumber = _mapper.Map<VillaNumberDTO>(model);
                var responseVillaList = await _villaService.GetAllAsync<APIResponse>();
                if (responseVillaList.IsSuccess && responseVillaList.Result != null) //lector's if construction assumes there's always gonna be villas. This is better
                {
                    villaNumberVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(responseVillaList.Result)).Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    });
                    return View(villaNumberVM);
                }
                return NotFound();
            }
            return NotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVillaNumber(VillaNumberDeleteVM model)
        {
            var response = await _villaNumberService.DeleteAsync<APIResponse>(model.VillaNumber.VillaNo);
            if (response.IsSuccess && response != null)
            {
                TempData["success"] = "VillaNumber deleted succesfully";
                return RedirectToAction(nameof(IndexVillaNumber));
            }

            TempData["error"] = "Error encountered";
            return View(model);
        }
    }
}



