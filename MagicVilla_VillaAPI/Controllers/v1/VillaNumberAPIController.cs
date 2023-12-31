﻿using Asp.Versioning;
using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillaNumberAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IVillaNumberRepository _dbVillaNumbers;
        private readonly IVillaRepository _dbVillas;
        private readonly IMapper _mapper;

        public VillaNumberAPIController(IVillaNumberRepository dbVilla, IMapper mapper, IVillaRepository dbVillas)
        {
            _dbVillaNumbers = dbVilla;
            _dbVillas = dbVillas;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet("GetString")]
        public IEnumerable<string> Get()
        {
            return new string[] { "VERSION 1", "VERSION 1 , 2" };
        }

        [HttpGet]
        //[MapToApiVersion("1.0")]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers([FromQuery] string? searchString,
            [FromQuery] int pageSize = 0, [FromQuery] int pageNumber = 1)
        {
            try
            {
                IEnumerable<VillaNumber> villaNumberList;
                Pagination pagination = new Pagination { PageNumber = pageNumber, PageSize = pageSize, TotalResultsCount = 0 };
                if (searchString != null)
                {
                    villaNumberList = await _dbVillaNumbers.GetAllAsync(x => x.Villa.Name.ToLower().Contains(searchString), includeProperties: "Villa", pageSize: pageSize, pageNumber: pageNumber);
                }
                else
                {
                    villaNumberList = await _dbVillaNumbers.GetAllAsync(includeProperties: "Villa", pageSize: pageSize, pageNumber: pageNumber);
                }

                pagination.TotalResultsCount = villaNumberList.Count(); //if filters are added mirror VillaController approach

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                _response.StatusCode = System.Net.HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villaNumber = await _dbVillaNumbers.GetAsync(x => x.VillaNo == id);
                if (villaNumber == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                if (createDTO.VillaNo == 0) //not done by instructor
                {
                    ModelState.AddModelError("ErrorMessages", "VillaNumber cannot be \"0\"");
                    return BadRequest(ModelState);
                }
                if (await _dbVillaNumbers.GetAsync(x => x.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "VillaNumber already exists!");
                    return BadRequest(ModelState);
                }

                if (createDTO == null) // empty data DTO
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.Result = createDTO; // not done by instructor
                    return BadRequest(_response);
                }

                if (await _dbVillas.GetAsync(x => createDTO.VillaId == x.Id) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is invalid");
                    return BadRequest(ModelState);
                }

                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);
                await _dbVillaNumbers.CreateAsync(villaNumber);


                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.Created;

                return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var villaNumber = await _dbVillaNumbers.GetAsync(x => x.VillaNo == id);
                if (villaNumber == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _dbVillaNumbers.RemoveAsync(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (id <= 0 || updateDTO.VillaNo != id)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var model = await _dbVillaNumbers.GetAsync(x => x.VillaNo == id, false); // no tracking since the tracker will throw an error due reinstantiate the object from tracker later
                if (model == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                if (await _dbVillas.GetAsync(x => updateDTO.VillaId == x.Id) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is invalid");
                    return BadRequest(ModelState);
                }

                model = _mapper.Map<VillaNumber>(updateDTO); // when this is done object is reinstanciated and EFC attepms tracking it anew so it creates duplicate tracking

                await _dbVillaNumbers.UpdateAsync(model);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                _response.Result = model;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id:int}", Name = "UpdatePartialVillaNumber")]
        public async Task<ActionResult<APIResponse>> UpdatePartialVillaNumber(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDTO)
        {
            try
            {
                if (id <= 0 || patchDTO == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var model = await _dbVillaNumbers.GetAsync(x => x.VillaNo == id, false); // no tracking since the tracker will throw an error due to reinstantiating the object from tracker later
                if (model == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                VillaNumberUpdateDTO villaNumberDto = _mapper.Map<VillaNumberUpdateDTO>(model);

                patchDTO.ApplyTo(villaNumberDto, ModelState); // db model > dto > db model for the sake of JsonPatch

                if (!ModelState.IsValid)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("ModelState is Invalid");
                    return BadRequest(ModelState);
                }

                model = _mapper.Map<VillaNumber>(villaNumberDto); // when this is done object is reinstanciated and EFC attepms tracking it anew so it creates duplicate tracking

                await _dbVillaNumbers.UpdateAsync(model);

                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
