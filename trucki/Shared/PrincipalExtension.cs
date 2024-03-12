using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;

namespace trucki.Shared
{
    /// <summary>
    /// This is an extension method for managing claims 
    /// </summary>
    public static class PrincipalExtension
    {
        public static string GetMobileNumber(this IIdentity identity) => GetClaimValue(identity, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone");
        public static string GetUserId(this IIdentity identity) => GetClaimValue(identity, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        public static long GetClientId(this IIdentity identity) => long.Parse(GetClaimValue(identity, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));

        public static string GetHeaderValue(this HttpRequestMessage request, string name)
        {
            IEnumerable<string> values;
            return request.Headers.TryGetValues(name, out values) ? values.FirstOrDefault<string>() : (string)null;
        }

        private static string GetClaimValue(this IEnumerable<Claim> claims, string claimType) => new List<Claim>(claims).Find((Predicate<Claim>)(c => c.Type == claimType))?.Value;

        private static string GetClaimValue(IIdentity identity, string claimType) => ((ClaimsIdentity)identity).Claims.GetClaimValue(claimType);

        public static async Task<string> GetAccessToken(this HttpContext context)
        {
            return await context.GetTokenAsync("access_token");
        }

        /*public static string GetChannel(this HttpContext context)
        {
            return context.Request.Headers["x-channel"];
        }

        public static string GetClientId(this HttpContext context)
        {
            return context.Request.Headers["x-clientid"];
        }*/

       /* public static string GetAppVersion(this HttpContext context)
        {
            return context.Request.Headers["x-appVersion"];
        }

        public static string GetAppPlatform(this HttpContext context)
        {
            return context.Request.Headers["x-appPlatform"];
        }*/

        public static TokenUserData GetSessionUser(this HttpContext context)
        {
            var data = new TokenUserData();

            string str1;
            if (context == null)
            {
                data = null;
            }
            else
            {
                ClaimsPrincipal user = context.User;
                if (user == null)
                {
                    str1 = (string)null;
                }
                else
                {
                    IEnumerable<Claim> claims = user.Claims;

                    data.UserId = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") : (string)null;
                    data.Mobile = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone") : (string)null;
                    data.Email = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") : (string)null;
                    data.FirstName = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname") : (string)null;
                    data.LastName = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname") : (string)null;
                    data.UserName = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name") : (string)null;
                    data.Gender = claims != null ? claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender") : (string)null;
                    data.HouseNumber = claims != null ? claims.GetClaimValue("houseNumber") : (string)null;
                    data.Street = claims != null ? claims.GetClaimValue("street") : (string)null;
                    data.LandMark = claims != null ? claims.GetClaimValue("landMark") : (string)null;
                    data.State = claims != null ? claims.GetClaimValue("state") : (string)null;
                    data.City = claims != null ? claims.GetClaimValue("city") : (string)null;
                    data.DateOfBirth = claims != null ? claims.GetClaimValue("dob") : (string)null;
                }
            }

            return data;
        }

        public class TokenUserData
        {
            public string UserId { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserName { get; set; }

            public string Gender { get; set; }

            public string HouseNumber { get; set; }

            public string Street { get; set; }
            public string LandMark { get; set; }

            public string City { get; set; }
            public string State { get; set; }
            public string DateOfBirth { get; set; }
        }
    }
}
