
using System.ComponentModel.DataAnnotations;

namespace HealthTracker.Authentication.Model.DTO.Incoming
{
    public class UserLoginRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}