using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
[EnableRateLimiting("fixedwindow")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly ILogger<CategoriasController> _logger;

    private readonly IMemoryCache _cache;
    private const string CacheCategoriasKey = "CacheCategorias";

    public CategoriasController(ILogger<CategoriasController> logger, IUnitOfWork uof, IMemoryCache cache)
    {
        _logger = logger;
        _uof = uof;
        _cache = cache;
    }
    /// <summary>
    /// Obtem uma lista de objetos Categoria
    /// </summary>
    /// <returns>Uma lista de objetos Categoria</returns>
   // [Authorize]
    [HttpGet]
    [DisableRateLimiting]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        if (!_cache.TryGetValue(CacheCategoriasKey, out IEnumerable<Categoria>? categorias))
        {
            categorias = await _uof.CategoriaRepository.GetAllAsync();

            if (categorias is null && !categorias.Any())       
            {
                _logger.LogWarning("Não existem categorias!");
                return NotFound("Não existem categorias.");
            }
            SetCache(CacheCategoriasKey, categorias);
        }

        var categoriasDTO = categorias.ToCategoriaDTOList();

        return Ok(categoriasDTO);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters)
    {
        var categorias = await _uof.CategoriaRepository.GetCategoriasAsync(categoriasParameters);

        return ObterCategorias(categorias);

    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFiltro)
    {
        var categoriasFiltradas = await _uof.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro);

        return ObterCategorias(categoriasFiltradas);

    }
    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
    {
        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriasDTO = categorias.ToCategoriaDTOList();

        return Ok(categoriasDTO);
    }

    /// <summary>
    /// Obtem uma categoria pelo seu ID 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Objetos Categoria</returns>
    [HttpGet("{id:int}", Name = "ObterCategoria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        var cacheKey = GetCategoriaCacheKey(id);

        if (!_cache.TryGetValue(cacheKey, out Categoria? categoria))
        {
            categoria = await _uof.CategoriaRepository.GetAsync(c => c.Id == id);

            if (categoria is  null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada...");
                return NotFound($"Categoria com id= {id} não encontrada...");
            }

            SetCache(cacheKey, categoria);
        }

        var categoriaDTO = categoria.ToCategoriaDTO();

        return Ok(categoriaDTO);
    }

    /// <summary>
    /// Inclui uma nova Categoria
    /// </summary>
    /// <remarks>
    /// Exemplo de Request:
    /// 
    ///    POST api/categorias
    ///    {
    ///        "categoriaId": 1,
    ///        "nome": "categoria",
    ///        "imagemURL": "http://teste.net/1.jpg"
    ///    }   
    /// </remarks>
    /// <param name="categoriaDTO">objeto Categoria</param>
    /// <returns>O objeto Categoria incluída</returns>
    /// <remarks>Retorna um objeto Categoria incluído</remarks>
    /// 
    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDTO)
    {
        if (categoriaDTO is null)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        var categoria = categoriaDTO.ToCategoria();

        var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
        await _uof.CommitAsync();

        var novaCategoriaDTO = categoriaCriada.ToCategoriaDTO();

        InvalidateCacheAfterChange(categoriaCriada.Id, categoriaCriada);

        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDTO.Id }, novaCategoriaDTO);
    }

    [HttpPut("{id:int}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDTO)
    {
        if (id != categoriaDTO?.Id || categoriaDTO is null || id <= 0)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        var categoria = categoriaDTO.ToCategoria();

        var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
        await _uof.CommitAsync();

        InvalidateCacheAfterChange(id, categoriaAtualizada);


        var categoriaAtualizadaDTO = categoriaAtualizada.ToCategoriaDTO();

        return Ok(categoriaAtualizadaDTO);
    }

    [HttpDelete("{id:int}")]
 //   [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _uof.CategoriaRepository.GetAsync(c => c.Id == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada...");
            return NotFound($"Categoria com id={id} não encontrada...");
        }

        var categoriaExcluida = _uof.CategoriaRepository.Delete(categoria);
        await _uof.CommitAsync();

        InvalidateCacheAfterChange(id);

        var categoriaExcluidaDTO = categoriaExcluida.ToCategoriaDTO();

        return Ok(categoriaExcluidaDTO);
    }

    private string GetCategoriaCacheKey(int id) => $"CacheCategoria_{id}";
    private void SetCache<T>(string key, T data)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            SlidingExpiration = TimeSpan.FromSeconds(15),
            Priority = CacheItemPriority.High
        };
        _cache.Set(key, data, cacheOptions);
    }
    private void InvalidateCacheAfterChange(int id, Categoria? categoria = null)
    {
        _cache.Remove(CacheCategoriasKey);
        _cache.Remove(GetCategoriaCacheKey(id));

        if (categoria != null)
        {
            SetCache(GetCategoriaCacheKey(id), categoria);
        }
    }
}