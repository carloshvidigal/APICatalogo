using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    //private readonly IProdutoRepository _produtoRepository;
    //private readonly IRepository<Produto> _repository;  // como o repositorio específico - ProdutoRepository - herda do repositorio
                                                         // genérico, poderia usar somente _produtoRepository pois ele contem todos 
                                                        // os métodos do genérico e o métodos específicos
                                                       // ou seja, todos métodos que foram usados nessa class
    private readonly IMapper _mapper;

    public ProdutosController(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    [HttpGet("produtos/{id}")]
    public ActionResult <IEnumerable<ProdutoDTO>> GetProdutosCategoria(int id)
    {
        var produtos = _uof.ProdutoRepository.GetProdutosPorCategoria(id); 
        
        if(produtos is null)
        {
            return NotFound();
        }

        //var destino = _mapper.Map<Destino>(origem);
        var produtosDTO = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDTO);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProdutoDTO>> Get()
    {
        var produtos = _uof.ProdutoRepository.GetAll();
        if (produtos is null)
        {
            return NotFound();
        }

        var produtosDTO = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDTO);
    }

    [HttpGet("{id}", Name = "ObterProduto")]
    public ActionResult<ProdutoDTO> Get(int id)
    {
        var produto = _uof.ProdutoRepository.Get(c => c.Id == id);

        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        var produtoDTO = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produtoDTO);
    }

    [HttpPost]
    public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDTO)
    {
        if (produtoDTO is null)
            return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDTO);

        var novoProduto = _uof.ProdutoRepository.Create(produto);
        _uof.Commit();

        var novoProdutoDTO = _mapper.Map<ProdutoDTO>(novoProduto);

        return new CreatedAtRouteResult("ObterProduto",
            new { id = novoProdutoDTO.Id }, novoProdutoDTO);
    }

    [HttpPut("{id:int}")]
    public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDTO)
    {
        if (id != produtoDTO.Id)
        {
            return BadRequest(); //400
        }

        var produto = _mapper.Map<Produto>(produtoDTO);

        var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
        _uof.Commit();

        var produtoAtualizadoDTO = _mapper.Map<ProdutoDTO>(produtoAtualizado);

        return Ok(produtoAtualizadoDTO);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<ProdutoDTO> Delete(int id)
    {
        var produto = _uof.ProdutoRepository.Get(p => p.Id == id);

        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        var produtoDeletado = _uof.ProdutoRepository.Delete(produto);
        _uof.Commit();

        var produtoDeletadoDTO = _mapper.Map<ProdutoDTO>(produtoDeletado);

        return Ok(produtoDeletadoDTO);
    }
}