
using System.ComponentModel.DataAnnotations;

namespace HealthTracker.Authentication.Model.DTO.Generic
{
    public class TokenData
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}