using CL.NerdLab.Lay_out.API.Controllers.Base;
using CL.NerdLab.Lay_out.API.Models;
using CL.NerdLab.Lay_out.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CL.NerdLab.Lay_out.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialRegistroCargaController : BaseController<HistorialRegistroCarga>
    {
        public HistorialRegistroCargaController(IGenericRepository<HistorialRegistroCarga> repository) : base(repository)
        {
        }
    }
}