using AutoMapper;
using HealthTracker.Configuration.Messages;
using HealthTracker.DataService.IConfiguration;
using HealthTracker.Entities.Dtos.Outgoing.Profile;
using HealtTracker.Entities;
using HealtTracker.Entities.DbSet;
using HealtTracker.Entities.Dtos.Generic;
using HealtTracker.Entities.Dtos.Incoming;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    
    public class UserController : BaseController
    {

        public UserController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userAuth,
            IMapper mapper) : base(unitOfWork, userAuth, mapper)
        {
        }

        [HttpGet]
        [Route("testrun")]
        public IActionResult TestRun()
        {
            return Ok("Success");
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _unitOfWork.Users.GetAll();
            var result = new PageResult<User>()
            {
                Content = users.ToList(),
                ResultCount = users.Count()
            };

            return Ok(result);

        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            var mappedUser = _mapper.Map<User>(user);

            await _unitOfWork.Users.Add(mappedUser);
            await _unitOfWork.CompleteAsync();

            var result = new Result<UserDto>
            {
                Content = user
            };

            return CreatedAtRoute(new { id = mappedUser.Id }, result);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto userDto)
        {
            var user = await _unitOfWork.Users.GetById(id);

            if (user == null)
            {
                return NotFound($"No user found with the {id}");
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Country = userDto.Country;
            user.Phone = userDto.Phone;
            user.DateofBirth = Convert.ToDateTime(userDto.DateofBirth);

            await _unitOfWork.CompleteAsync();
            return Ok(user);


        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetSingleUser(Guid id)
        {
            var user = await _unitOfWork.Users.GetById(id);

            var result = new Result<ProfileDto>();

            if (user != null)
            {
                var mappedProfile = _mapper.Map<ProfileDto>(user);
                result.Content = mappedProfile;
                return Ok(result);
            };

                result.Error = CustomError(404, ErrorMessages.User.UserNotFound, ErrorMessages.Generic.ObjectNotFound);
                return NotFound(result);

        }
    }
}