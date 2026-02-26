using CL.NerdLab.Lay_out.API.Controllers.Base;
using CL.NerdLab.Lay_out.API.Models;
using CL.NerdLab.Lay_out.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CL.NerdLab.Lay_out.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatiosController : BaseController<Patios>
    {
        public PatiosController(IGenericRepository<Patios> repository) : base(repository)
        {
        }
    }
}