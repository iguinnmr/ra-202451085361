using System.Text.Json;
using ServicoProdutos.Models;

namespace ServicoProdutos.Repositorios;

/// <summary>
/// Implementação ORIENTADA A DOCUMENTOS — VOCÊ vai completá-la.
/// Cada produto é UM ARQUIVO JSON (um documento por agregado), como
/// num MongoDB simplificado: dados/produto-1.json, dados/produto-2.json...
/// Não há tabela, não há esquema imposto pelo "banco": o documento é o dado.
/// </summary>
public class ProdutoRepositorioDocumento : IProdutoRepositorio
{
    private readonly string _pasta;

    public ProdutoRepositorioDocumento(IConfiguration config)
    {
        _pasta = config["PastaDocumentos"] ?? "dados";
        Directory.CreateDirectory(_pasta);
    }

    private string CaminhoDoDocumento(int id) => Path.Combine(_pasta, $"produto-{id}.json");

    public List<Produto> ObterTodos()
    {
var produtos = new List<Produto>();

foreach (var arquivo in Directory.EnumerateFiles(_pasta, "produto-*.json"))
{
    var json = File.ReadAllText(arquivo);
    var produto = JsonSerializer.Deserialize<Produto>(json);

    if (produto != null)
    {
        produtos.Add(produto);
    }
}

return produtos.OrderBy(p => p.Id).ToList();
    }

    public Produto? ObterPorId(int id)
    {
      var caminho = CaminhoDoDocumento(id);

if (!File.Exists(caminho))
{
    return null;
}

var json = File.ReadAllText(caminho);

return JsonSerializer.Deserialize<Produto>(json);
    }

    public List<Produto> ObterAbaixoDe(decimal precoMaximo)
    {
       var produtos = ObterTodos();

return produtos
    .Where(p => p.Preco < precoMaximo)
    .ToList();
    }

    public Produto Criar(Produto produto)
    {
       var produtos = ObterTodos();

var proximoId = produtos.Count == 0
    ? 1
    : produtos.Max(p => p.Id) + 1;

produto.Id = proximoId;

var json = JsonSerializer.Serialize(
    produto,
    new JsonSerializerOptions { WriteIndented = true });

File.WriteAllText(CaminhoDoDocumento(produto.Id), json);

return produto;
    }
}
