using EscolaApi.Data;
using EscolaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaApi.Controllers;

[ApiController]
[Route("api/v1/alunos")]
public class AlunosController : ControllerBase
{
    private readonly AppDbContext db;

    public AlunosController(AppDbContext db)
    {
        this.db = db;
    }

    // ============================================================
    // GET api/v1/alunos?page=1&size=10
    // Lista alunos com paginação
    // Corrige:
    // - verbo na URI
    // - ausência de versionamento
    // - lista sem paginação
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> GetAlunos([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (size <= 0)
        {
            size = 10;
        }

        var total = await db.Alunos.CountAsync();

        var alunos = await db.Alunos
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new
            {
                a.Id,
                a.Nome,
                a.Curso
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            size,
            total,
            totalPages = (int)Math.Ceiling(total / (double)size),
            items = alunos
        });
    }

    // ============================================================
    // GET api/v1/alunos/{id}
    // Busca aluno por ID
    // Retorna:
    // - 200 OK se encontrar
    // - 404 Not Found se não encontrar
    // ============================================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlunoPorId(int id)
    {
        var aluno = await db.Alunos
            .AsNoTracking()
            .Include(a => a.Matriculas)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aluno is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Aluno não encontrado",
                Status = 404,
                Detail = $"Não existe aluno cadastrado com o ID {id}."
            });
        }

        return Ok(aluno);
    }

    // ============================================================
    // POST api/v1/alunos
    // Cria um novo aluno
    // Retorna:
    // - 201 Created se criar
    // - 400 Bad Request se faltar informação obrigatória
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> CriarAluno([FromBody] Aluno aluno)
    {
        if (aluno is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Requisição inválida",
                Status = 400,
                Detail = "Os dados do aluno não foram enviados corretamente."
            });
        }

        if (string.IsNullOrWhiteSpace(aluno.Nome))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Campo obrigatório",
                Status = 400,
                Detail = "O campo Nome é obrigatório."
            });
        }

        if (string.IsNullOrWhiteSpace(aluno.Curso))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Campo obrigatório",
                Status = 400,
                Detail = "O campo Curso é obrigatório."
            });
        }

        aluno.Id = 0;

        db.Alunos.Add(aluno);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAlunoPorId),
            new { id = aluno.Id },
            aluno
        );
    }

    // ============================================================
    // PUT api/v1/alunos/{id}
    // Atualiza completamente os dados de um aluno
    // Retorna:
    // - 200 OK se atualizar
    // - 404 Not Found se não encontrar
    // - 400 Bad Request se faltar informação obrigatória
    // ============================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarAluno(int id, [FromBody] Aluno alunoAtualizado)
    {
        if (alunoAtualizado is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Requisição inválida",
                Status = 400,
                Detail = "Os dados do aluno não foram enviados corretamente."
            });
        }

        if (string.IsNullOrWhiteSpace(alunoAtualizado.Nome))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Campo obrigatório",
                Status = 400,
                Detail = "O campo Nome é obrigatório."
            });
        }

        if (string.IsNullOrWhiteSpace(alunoAtualizado.Curso))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Campo obrigatório",
                Status = 400,
                Detail = "O campo Curso é obrigatório."
            });
        }

        var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.Id == id);

        if (aluno is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Aluno não encontrado",
                Status = 404,
                Detail = $"Não existe aluno cadastrado com o ID {id}."
            });
        }

        aluno.Nome = alunoAtualizado.Nome;
        aluno.Curso = alunoAtualizado.Curso;

        await db.SaveChangesAsync();

        return Ok(aluno);
    }

    // ============================================================
    // DELETE api/v1/alunos/{id}
    // Remove um aluno pelo ID
    // Retorna:
    // - 204 No Content se remover
    // - 404 Not Found se não encontrar
    // ============================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarAluno(int id)
    {
        var aluno = await db.Alunos
            .Include(a => a.Matriculas)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aluno is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Aluno não encontrado",
                Status = 404,
                Detail = $"Não existe aluno cadastrado com o ID {id}."
            });
        }

        db.Alunos.Remove(aluno);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ============================================================
    // GET api/v1/alunos/{id}/matriculas
    // Lista as matrículas de um aluno
    // Corrige o erro HTML com status 500
    // Retorna:
    // - 200 OK se o aluno existir
    // - 404 Not Found se o aluno não existir
    // ============================================================
    [HttpGet("{id}/matriculas")]
    public async Task<IActionResult> GetMatriculasDoAluno(int id)
    {
        var alunoExiste = await db.Alunos.AnyAsync(a => a.Id == id);

        if (!alunoExiste)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Aluno não encontrado",
                Status = 404,
                Detail = $"Não existe aluno cadastrado com o ID {id}."
            });
        }

        var matriculas = await db.Matriculas
            .AsNoTracking()
            .Where(m => m.AlunoId == id)
            .ToListAsync();

        return Ok(matriculas);
    }

    // ============================================================
    // GET api/v1/alunos/{id}/matriculas/{matriculaId}
    // Busca uma matrícula específica de um aluno
    // Corrige o aninhamento profundo da rota antiga
    // Retorna:
    // - 200 OK se encontrar
    // - 404 Not Found se não encontrar
    // ============================================================
    [HttpGet("{id}/matriculas/{matriculaId}")]
    public async Task<IActionResult> GetMatriculaPorId(int id, int matriculaId)
    {
        var alunoExiste = await db.Alunos.AnyAsync(a => a.Id == id);

        if (!alunoExiste)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Aluno não encontrado",
                Status = 404,
                Detail = $"Não existe aluno cadastrado com o ID {id}."
            });
        }

        var matricula = await db.Matriculas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.AlunoId == id);

        if (matricula is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Matrícula não encontrada",
                Status = 404,
                Detail = $"Não existe matrícula com ID {matriculaId} para o aluno {id}."
            });
        }

        return Ok(matricula);
    }
}