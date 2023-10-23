﻿using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<bool> IsUniqueUser(string username);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginResponseDTO);
        Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO);
    }
}
