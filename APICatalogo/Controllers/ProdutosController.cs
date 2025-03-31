using APICatalogo.Context;
using APICatalogo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Produto>> Get()
        {
            var produtos = _context.Produtos.ToList();
            if (produtos is null)
            {
                return NotFound("Produtos não encontrados.");
            }
            return produtos;
        }


        [HttpGet("{id}:int", Name = "ObterProduto")]
        public ActionResult<Produto> Get(int id)
        {
            var produtos = _context.Produtos.FirstOrDefault(p => p.Id == id);
            if (produtos is null)
            {
                return NotFound("Produto não encontrado.");
            }
            return produtos; 
        }

        [HttpPost]
        public ActionResult Post (Produto produto)
        {
            if(produto == null)
            {
                return BadRequest();
            }
            _context.Produtos.Add(produto);
            _context.SaveChanges();

            return new CreatedAtRouteResult("ObterProduto", new { id = produto.Id }, produto);
        }
    }
}
