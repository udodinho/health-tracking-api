
using System.ComponentModel.DataAnnotations;

namespace HealthTracker.Authentication.Model.DTO.Incoming
{
    public class TokenRequestDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}