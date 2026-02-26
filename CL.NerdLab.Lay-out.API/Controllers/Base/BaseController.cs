using CL.NerdLab.Lay_out.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CL.NerdLab.Lay_out.API.Controllers.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<T> : ControllerBase where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public BaseController(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _repository.GetAllAsync();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound("Registro no encontrado.");
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] T entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdEntity = await _repository.AddAsync(entity);
            return Ok(createdEntity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] T entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Aquí idealmente validarías que el ID de la URL coincida con el de la entidad
            await _repository.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}