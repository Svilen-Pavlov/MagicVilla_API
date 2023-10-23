using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _db;
        private readonly IMapper _mapper;
        private string _secretKey;


        public UserRepository(ApplicationDBContext db, IMapper mapper, IConfiguration configuration)
        {
            _mapper = mapper;
            _db = db;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }
        public async Task<bool> IsUniqueUser(string username)
        {
            var user = await _db.LocalUsers.FirstOrDefaultAsync(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await _db.LocalUsers.FirstOrDefaultAsync(x => x.UserName.ToLower() == loginRequestDTO.UserName.ToLower() &&
            x.Password.ToLower() == loginRequestDTO.Password.ToLower());

            if (user == null)
            {
                return new LoginResponseDTO()
                {
                    User = null,
                    Token = ""
                };

            }
            //if user is found, generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }), // all token claims
                Expires = DateTime.Now.Date.AddDays(7), // expiration date
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), //tokencredentials
            };

            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new LoginResponseDTO
            {
                User = user,
                Token = tokenHandler.WriteToken(token),
            };
            return loginResponseDTO;
        }

        public async Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            LocalUser user = _mapper.Map<LocalUser>(registrationRequestDTO);

            await _db.LocalUsers.AddAsync(user);
            await _db.SaveChangesAsync();

            user.Password = "";

            return user;
        }
    }
}
