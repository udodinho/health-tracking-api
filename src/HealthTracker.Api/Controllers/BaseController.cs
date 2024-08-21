
using AutoMapper;
using HealthTracker.DataService.IConfiguration;
using HealtTracker.Entities.Dtos.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
         public IUnitOfWork _unitOfWork;
         public UserManager<IdentityUser> _userAuth;
         public IMapper _mapper;

        public BaseController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userAuth, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userAuth = userAuth;
            _mapper = mapper;
        }

        internal Error CustomError(int code, string message, string type)
        {
            return new Error ()
            {
                Code = code,
                Message = message,
                Type = type
            };
        }
    }
}