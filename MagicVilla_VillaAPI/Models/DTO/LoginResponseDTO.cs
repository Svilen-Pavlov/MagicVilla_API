namespace MagicVilla_VillaAPI.Models.DTO
{
    public class LoginResponseDTO
    {
        public UserDTO User { get; set; }
        public List<string> Roles { get; set; }

        public string Token { get; set; }
    }
}
