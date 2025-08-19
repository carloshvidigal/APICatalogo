using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

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
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosCategoria(int id)
    {
        var produtos = await _uof.ProdutoRepository.GetProdutosPorCategoriaAsync(id); 
        
        if(produtos is null)
        {
            return NotFound();
        }

        //var destino = _mapper.Map<Destino>(origem);
        var produtosDTO = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDTO);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParameters)
    {
        var produtos = await _uof.ProdutoRepository.GetProdutosAsync(produtosParameters);

        return ObterProdutos(produtos);
    }


    [HttpGet("filter/preco/pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco
                                                                                produtosFilterParameters)
    {
        var produtos = await _uof.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFilterParameters);
        return ObterProdutos(produtos);
    }

    private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto> produtos)
    {
        var metadata = new
        {
            produtos.Count,
            produtos.PageSize,
            produtos.PageCount,
            produtos.TotalItemCount,
            produtos.HasNextPage,
            produtos.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var produtosDTO = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDTO);
    }

    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get()
    {
        var produtos = await _uof.ProdutoRepository.GetAllAsync();
        if (produtos is null)
        {
            return NotFound();
        }

        var produtosDTO = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDTO);
    }

    [HttpGet("{id}", Name = "ObterProduto")]
    public async Task<ActionResult<ProdutoDTO>> Get(int id)
    {
        var produto = await _uof.ProdutoRepository.GetAsync(c => c.Id == id);

        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        var produtoDTO = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produtoDTO);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDTO)
    {
        if (produtoDTO is null)
            return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDTO);

        var novoProduto = _uof.ProdutoRepository.Create(produto);
        await _uof.CommitAsync();

        var novoProdutoDTO = _mapper.Map<ProdutoDTO>(novoProduto);

        return new CreatedAtRouteResult("ObterProduto",
            new { id = novoProdutoDTO.Id }, novoProdutoDTO);
    }


    [HttpPatch("{id}/UpdatePartial")]
    public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
    {
        if (patchProdutoDTO is null || id <= 0)
        {
            return BadRequest();
        }

        var produto = await _uof.ProdutoRepository.GetAsync(c => c.Id == id);

        if (produto is null)
        {
            return NotFound();
        }

        var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);

        patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState);


        if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(produtoUpdateRequest, produto);

        _uof.ProdutoRepository.Update(produto);
        await _uof.CommitAsync();

        return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
    }


    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDTO)
    {
        if (id != produtoDTO.Id)
        {
            return BadRequest(); //400
        }

        var produto = _mapper.Map<Produto>(produtoDTO);

        var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
        await _uof.CommitAsync();

        var produtoAtualizadoDTO = _mapper.Map<ProdutoDTO>(produtoAtualizado);

        return Ok(produtoAtualizadoDTO);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Delete(int id)
    {
        var produto = await _uof.ProdutoRepository.GetAsync(p => p.Id == id);

        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        var produtoDeletado = _uof.ProdutoRepository.Delete(produto);
        await _uof.CommitAsync();

        var produtoDeletadoDTO = _mapper.Map<ProdutoDTO>(produtoDeletado);

        return Ok(produtoDeletadoDTO);
    }
}