using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private string _secretKey;

        public UserRepository(ApplicationDBContext db, IMapper mapper, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _db = db;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }
        public async Task<bool> IsUniqueUser(string username)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName.ToLower() == loginRequestDTO.UserName.ToLower()
            //&& x.Password.ToLower() == loginRequestDTO.Password.ToLower() // we dont need this when using the .NET identity user
            );

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (user == null || isValid == false)
            {
                return new LoginResponseDTO()
                {
                    User = null,
                    Token = ""
                };

            }
            //if user is found, generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                }), // all token claims
                Expires = DateTime.Now.Date.AddDays(7), // expiration date
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), //tokencredentials
            };

            foreach (var role in roles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                User = _mapper.Map<UserDTO>(user),
                Token = tokenHandler.WriteToken(token),
                Roles = roles.ToList(), 
            };
            return loginResponseDTO;
        }

        public async Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDTO.UserName,
                Email = registrationRequestDTO.UserName,
                NormalizedEmail = registrationRequestDTO.UserName.ToUpper(),
                Name = registrationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password); //creastes user and hashes pass
                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync("admin")==false)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin")); //seed roles for the first time
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }

                    await _userManager.AddToRoleAsync(user, "admin");
                    var userToReturn = _db.ApplicationUsers.FirstOrDefault(x=>x.UserName== registrationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch (Exception e)
            { 
            
            }

            return new UserDTO();
        }
    }
}
