using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Kakadu.ConfigurationApi.Settings;
using Kakadu.Core.Interfaces;
using Kakadu.DTO;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kakadu.ConfigurationApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IUserService _userService;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IMapper _mapper;

        public TokenController(ILogger<TokenController> logger, IOptions<JwtSettings> jwtSettings, IUserService userService, IMapper mapper)
        {
            _logger = logger;
            _userService = userService;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult<UserDTO> Post([FromBody]JwtTokenDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = _userService.Authenticate(dto.Username, dto.Password);
            if(user == null)
               throw new HttpUnauthorizedException("Wrong username or password");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Value.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            _logger.LogInformation($"Creating JWT token for '{dto.Username}'");

            return Ok(_mapper.Map<UserDTO>(user));            
        }
    }
}