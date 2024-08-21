
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using HealthTracker.Authentication.Configuration;
using HealthTracker.Authentication.Model.DTO.Generic;
using HealthTracker.Authentication.Model.DTO.Incoming;
using HealthTracker.Authentication.Model.DTO.Outgoing;
using HealthTracker.Authentication.Models.DTO.Incoming;
using HealthTracker.Authentication.Models.DTO.Outgoing;
using HealthTracker.DataService.IConfiguration;
using HealtTracker.Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthTracker.Controllers
{
    public class AuthController : BaseController
    {
        
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;

        public AuthController(
            IMapper mapper,
            IUnitOfWork unitOfWork, UserManager<IdentityUser> userAuth,
            TokenValidationParameters tokenValidationParameters,
            IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork, userAuth, mapper)
        {
            _userAuth = userAuth;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto registerDto)
        {
            if (ModelState.IsValid)
            {
                var emailExist = await _userAuth.FindByEmailAsync(registerDto.Email);

                if (emailExist != null)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "Email already exist" }
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = registerDto.Email,
                    UserName = registerDto.Email,
                    EmailConfirmed = true // todo build email func
                };

                var isCreated = await _userAuth.CreateAsync(newUser, registerDto.Password);

                if (!isCreated.Succeeded)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = isCreated.Errors.Select(x => x.Description).ToList()
                    });
                }

                // Add new user
                var users = new User
                {
                    IdentityId = new Guid(newUser.Id),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email,
                    Phone = "",
                    Country = "",
                    MobileNumber = "",
                    Address = "",
                    Gender = "",
                    DateofBirth = DateTime.UtcNow   //Convert.ToDateTime(user.DateofBirth)
                };

                await _unitOfWork.Users.Add(users);
                await _unitOfWork.CompleteAsync();

                var token = await GenerateJWTToken(newUser);

                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token.JwtToken,
                    RefreshToken = token.RefreshToken
                });


            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto()
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginDto)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userAuth.FindByEmailAsync(loginDto.Email);

                if (userExist == null)
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "user does not exist" }
                    });
                }

                var isValidPassword = await _userAuth.CheckPasswordAsync(userExist, loginDto.Password);

                if (isValidPassword)
                {
                    var token = await GenerateJWTToken(userExist);

                    return Ok(new UserRegistrationResponseDto()
                    {
                        Success = true,
                        Token = token.JwtToken,
                        RefreshToken = token.RefreshToken
                    });
                }
                else
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "password is incorrect" }
                    });
                }

            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto()
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }
        }

        [HttpPost]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyToken(tokenRequestDto);

                if (result == null)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "token validation failed" }
                    });
                }

                return Ok(result);
            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto()
                {
                    Success = false,
                    Errors = new List<string>() { "invalid payload" }
                });
            }
        }

        private async Task<TokenData> GenerateJWTToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime), // todo update the expiry time to minutes
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // todo review algorithm
            };

            // Generate the security object token
            var token = jwtHandler.CreateToken(tokenDescriptor);

            // Convert the token object to string
            var jwtToken = jwtHandler.WriteToken(token);

            // Generate a refresh token
            var refreshToken = new RefreshToken
            {
                StartDate = DateTime.UtcNow,
                Token = $"{GenerateRandomString(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                JwtId = token.Id,
                Status = 1,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await _unitOfWork.RefreshTokens.Add(refreshToken);
            await _unitOfWork.CompleteAsync();

            var tokens = new TokenData
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token,
            };

            return tokens;
        }

        private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenValidationParameters, out var validatedToken);

                // validate if the string is an actual JWT token 
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result) return null;
                }

                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryTime = UnixTimeStampToDate(utcExpiryDate);

                // checking if the jwt token is expired
                if (expiryTime > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Jwt token has not expired",
                        }
                    };
                };

                var refreshTokenExist = await _unitOfWork.RefreshTokens.GetByRefreshToken(tokenRequestDto.RefreshToken);

                if (refreshTokenExist == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid refresh token",
                        }
                    };
                }

                if (refreshTokenExist.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh token has expired, please login again",
                        }
                    };
                }

                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh token has been used",
                        }
                    };
                }

                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh token has been revoked",
                        }
                    };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (refreshTokenExist.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh token does not match",
                        }
                    };
                }

                var updateResult = await _unitOfWork.RefreshTokens.MarkRefreshTokenAsUsed(refreshTokenExist);

                if (updateResult)
                {
                    await _unitOfWork.CompleteAsync();

                    var dbUser = await _userAuth.FindByIdAsync(refreshTokenExist.UserId);

                    if (dbUser == null)
                    {
                        return new AuthResult()
                        {
                            Success = false,
                            Errors = new List<string>()
                            {
                                "User doesn't exist",
                            }
                        };
                    }

                    var tokens = await GenerateJWTToken(dbUser);
                    return new AuthResult()
                    {
                        Token = tokens.JwtToken,
                        RefreshToken = tokens.RefreshToken,
                        Success = true
                    };
                }

                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                        {
                            "Error processing request",
                        }
                };


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null; // Todo Add error handling
            }
        }

        private DateTime UnixTimeStampToDate(long unixDate)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();
            return dateTime;
        }

        private static string GenerateRandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        }

    }
}