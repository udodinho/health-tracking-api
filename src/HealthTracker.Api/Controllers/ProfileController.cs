using AutoMapper;
using HealthTracker.Configuration.Messages;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.Entities.Dtos.Outgoing.Profile;
using HealtTracker.Entities.DbSet;
using HealtTracker.Entities.Dtos.Generic;
using HealtTracker.Entities.Dtos.Incoming.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ProfileController : BaseController
    {
        public ProfileController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userAuth,
            IMapper mapper) : base(unitOfWork, userAuth, mapper)
        {
        }

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var loggedInUser = await _userAuth.GetUserAsync(HttpContext.User);

            var result = new Result<ProfileDto>();
            if (loggedInUser == null)
            {
                result.Error = CustomError(404, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeNotFound);
                return NotFound(result);
            }

            var identityId = new Guid(loggedInUser.Id);
            var profile = await _unitOfWork.Users.GetByIdentityId(identityId);

            if (profile == null)
            {
                result.Error = CustomError(404, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeNotFound);
                return NotFound(result);
            }

            var mappedProfile = _mapper.Map<ProfileDto>(profile);

            result.Content = mappedProfile;
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            var result = new Result<ProfileDto>();

            if (!ModelState.IsValid)
            {
                result.Error = CustomError(404, ErrorMessages.Generic.InvalidPayload, ErrorMessages.Generic.TypeBadRequest);
                return BadRequest(result);
            }

            var loggedInUser = await _userAuth.GetUserAsync(HttpContext.User);

            if (loggedInUser == null)
            {
                {
                    result.Error = CustomError(404, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeNotFound);
                    return NotFound(result);
                }
            }

            var identityId = new Guid(loggedInUser.Id);
            var profile = await _unitOfWork.Users.GetByIdentityId(identityId);

            if (profile == null)
            {
                result.Error = CustomError(404, ErrorMessages.Profile.UserNotFound, ErrorMessages.Generic.TypeNotFound);
                return NotFound(result);
            }

            profile.Address = updateProfileDto.Address;
            profile.Country = updateProfileDto.Country;
            profile.MobileNumber = updateProfileDto.MobileNumber;
            profile.Gender = updateProfileDto.Gender;

            var updateProfile = await _unitOfWork.Users.UpdateUserProfile(profile);

            if (updateProfile)
            {
                await _unitOfWork.CompleteAsync();
                var mappedProfile = _mapper.Map<ProfileDto>(profile);
                result.Content = mappedProfile;
                return Ok(result);
            }

            result.Error = CustomError(500, ErrorMessages.Generic.SomethingWentWrong, ErrorMessages.Generic.UnableToProcess);

            return BadRequest();
        }
    }
}