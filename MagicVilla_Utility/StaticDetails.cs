namespace MagicVilla_Utility
{
    public static class StaticDetails
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public static string SessionTokenName = "JWTToken";
        public static string JWTAuthenticationHeaderName= "Bearer";
    }
}