using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kakadu.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kakadu.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesController : BaseController
    {
        private readonly ILogger<ServicesController> logger;

        public ServicesController(ILogger<ServicesController> logger, IMapper mapper) : base(mapper)
        {
            this.logger = logger;
        }

        // [HttpGet]
        // public async Task<List<ServiceDTO>> Get()
        // {
            
        // }
    }
}