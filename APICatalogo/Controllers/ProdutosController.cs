using APICatalogo.Context;
using APICatalogo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<Produto>>> Get()
        {
            try
            {
                var produtos = await _context.Produtos.ToListAsync();
                if (produtos is null)
                {
                    return NotFound();
                }
                return produtos;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                           "Ocorreu um problema ao tratar a sua solicitação.");
            }

        }

        [HttpGet("primeiro")]
        public async Task<ActionResult<Produto>> GetPrimeiro()
        {
            try
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync();
                if (produto is null)
                {
                    return NotFound();
                }

                return produto;
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                           "Ocorreu um problema ao tratar a sua solicitação.");
            }



        }


        [HttpGet("{id:int}", Name = "ObterProduto")]
        public async Task<ActionResult<Produto>> Get(int id)
        {
            try
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);
                if (produto is null)
                {
                    return NotFound("Produto não encontrado...");
                }
                return produto;

            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                           "Ocorreu um problema ao tratar a sua solicitação.");
            }
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

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Produto produto)
        {
            if (id !=  produto.Id)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(produto);
        }

        [HttpDelete("{id:int}")]

        public ActionResult Delete(int id)
        {
            var produto = _context.Produtos.FirstOrDefault(p => p.Id==id);

            if(produto is null)
            {
                return NotFound("Produto não localizado.");
            }

            _context.Produtos.Remove(produto);
            _context.SaveChanges();

            return Ok(produto);
        }

    }
}
