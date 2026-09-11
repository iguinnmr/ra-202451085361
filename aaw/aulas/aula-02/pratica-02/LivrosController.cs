using BibliotecaApi.Models;
using BibliotecaApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly LivroRepository _repository;

    public LivrosController(LivroRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Livro>> GetAll()
    {
        return Ok(_repository.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Livro> GetById(int id)
    {
        var livro = _repository.GetById(id);

        if (livro is null)
        {
            return NotFound();
        }

        return Ok(livro);
    }

    [HttpPost]
    public ActionResult<Livro> Create(Livro livro)
    {
        if (livro is null || string.IsNullOrWhiteSpace(livro.Titulo))
        {
            return BadRequest(new
            {
                title = "Bad Request",
                status = 400,
                errors = new
                {
                    Titulo = new[] { "O campo Titulo é obrigatório" }
                }
            });
        }

        var criado = _repository.Create(livro);

        return CreatedAtAction(
            nameof(GetById),
            new { id = criado.Id },
            criado
        );
    }

    [HttpPut("{id}")]
    public ActionResult<Livro> Update(int id, Livro livro)
    {
        if (livro is null || string.IsNullOrWhiteSpace(livro.Titulo))
        {
            return BadRequest(new
            {
                title = "Bad Request",
                status = 400,
                errors = new
                {
                    Titulo = new[] { "O campo Titulo é obrigatório" }
                }
            });
        }

        var atualizado = _repository.Update(id, livro);

        if (atualizado is null)
        {
            return NotFound();
        }

        return Ok(atualizado);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var removido = _repository.Delete(id);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }
}
