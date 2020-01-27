using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Kakadu.WebApp.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly IMapper mapper;

        public BaseController(IMapper mapper) => this.mapper = mapper;
    }
}