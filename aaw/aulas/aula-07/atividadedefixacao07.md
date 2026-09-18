# HANDOUT — AULA 07

## Caça às Vulnerabilidades

*Revisão de segurança de uma API .NET — Arquitetura de Aplicações Web*

> **Nomes:** Igor Nóbrega Moreira e Gabriel Luiz Sá Silva   **Turma:** Arquitetura de Aplicações Web   **Data:** 17 / 02 / 2026

## VULNERABILIDADE 01 — A busca de clientes

> `GET /api/clientes/buscar?nome=...`

Endpoint de busca usado pela tela de atendimento. O parâmetro nome vem direto da caixa de busca do site.

```text
 1  [HttpGet("buscar")]
 2  public IActionResult Buscar(string nome)
 3  {
 4      var sql = "SELECT * FROM Clientes WHERE Nome = '"
 5                + nome + "'";
 6      var clientes = _db.Clientes.FromSqlRaw(sql).ToList();
 7      return Ok(clientes);
 8  }
```

**Sua análise:**

1. Qual é a falha?

O código coleta o texto que esta digitado no campo de busca juntando com a instrução, isso pode acarretar o problema porque o sistema confia cegamente no que vem na tela, assim se acrescentarem um código SQL no lugar do nome a consulta vai ser executada com o que estiver no campo.

2. Qual o dano possível em produção?

O possível dano vem de vários pontos como a violação da LGPD retornando dados de usuários cadastrados, pode deletar tabelas ou altera-las juntamente do seus campos e se dar permissão para logar como admin.

3. Como corrigir?

A solução seria utilizarmos parâmetros SQL fazendo com que o banco trate a entrada do usuário apenas como texto e não como código.

## VULNERABILIDADE 02 — A consulta de faturas

> `GET /api/faturas/{id}`

Endpoint usado pelo app para exibir a fatura do cartão. O usuário está autenticado quando chama esta rota.

```text
 1  [HttpGet("{id}")]
 2  public IActionResult GetFatura(int id)
 3  {
 4      var fatura = _db.Faturas.Find(id);
 5      if (fatura == null) return NotFound();
 6      return Ok(fatura);
 7  }
```

**Sua análise:**

Questão 2

1. Qual é a falha?

A consulta ate verifica se o usuário foi autenticado, porém não valida se a fatura é do mesmo, assim podendo consultar faturas de outros usuários apenas alterando a URL.

2. Qual o dano possível em produção?

Alguns dos possíveis danos são a violação da LGPD visto que qualquer usuário pode visualizar a fatura de outro alterando a URL e pode baixar o banco inteiro.

3. Como corrigir?

Será necessário coletar o ID do usuário logado com token de autenticação JWT/Session e incluir esse ID na busca do banco de dados, garantindo que ele só consiga ver a fatura se ela pertencer a ele.

## VULNERABILIDADE 03 — A configuração do servidor

> `Program.cs (roda igual em dev e em produção)`

Trecho de inicialização da API, idêntico em todos os ambientes. Este arquivo está versionado no Git da empresa.

```text
 1  public const string Conn =
 2      "Server=prod-db;Database=Banco;User=sa;" +
 3      "Password=Newton@2026!";
 4
 5  var app = WebApplication.CreateBuilder(args).Build();
 6  app.UseDeveloperExceptionPage();
 7  app.Run();
```

**Sua análise:**

1. Qual é a falha?
    O arquivo contem a senha do banco de dados aberta (Password=Newton@2026!). Com isso, qualquer pessoa com acesso ao repositório descobre a chave de acesso do banco. Além disso, a página de erro detalhada mostra o "coração" do código quando algo dá errado, facilitando a vida de um invasor.

2. Qual o dano possível em produção?
    Comprometimento total do banco de dados por vazamento de credenciais, exposição de falhas e estruturas internas do sistema para atacantes.

3. Como corrigir?
    Remover as credenciais do código-fonte (utilizando variáveis de ambiente ou gerenciadores de segredos) e restringir a página de erro detalhada apenas ao ambiente de desenvolvimento.

## VULNERABILIDADE 04 — A atualização de perfil

> `PUT /api/usuarios/{id}`

Endpoint que o app chama quando o usuário edita o próprio perfil. O corpo da requisição é o JSON enviado pelo cliente.

```text
 1  public class UsuarioUpdate
 2  {
 3      public string Nome  { get; set; }
 4      public string Email { get; set; }
 5      public string Role  { get; set; }   // "user" | "admin"
 6  }
 7
 8  [HttpPut("{id}")]
 9  public IActionResult Atualizar(int id, UsuarioUpdate dto)
10  {
11      _repo.AtualizarTudo(id, dto);
12      return NoContent();
13  }
```

**Sua análise:**

Resposta 4

1. Qual é a falha?
    O sistema confia cegamente nos dados enviados pelo cliente no corpo da requisição e permite que o campo Role  seja alterado livremente por qualquer pessoa através da tela de edição de perfil.    

2. Qual o dano possível em produção?
    Escalonamento de privilégios não autorizado, permitindo que um usuário comum mude o próprio perfil para administrador (admin).

3. Como corrigir?
    Remover o campo Role do objeto de atualização de perfis comuns ou isolar a alteração de papéis em um endpoint administrativo protegido e restrito.

## DESAFIO

1. Qual das 4 falhas um scanner automático de código teria MAIS dificuldade de encontrar? Por quê?

    A maior dificuldade seria no momento de encontrar a vulnerabilidade da número 2, visto que o scanner vê uma consulta simples no banco pelo ID considerando o código correto, sem saber se o id deveria ou não pertencer aquele usuário.
