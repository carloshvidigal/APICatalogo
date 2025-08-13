using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParameters)
        {
            var categorias = await GetAllAsync();

            var categoriasOrdenadas = categorias.OrderBy(p => p.Id).AsQueryable();

            //var resultado = PagedList<Categoria>.ToPagedList(categoriasOrdenadas, categoriasParameters.PageNumber, categoriasParameters.PageSize);

            var resultado = await categoriasOrdenadas.ToPagedListAsync(categoriasParameters.PageNumber,
                                                           categoriasParameters.PageSize);

            return resultado;
        }

        public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParameters)
        {
            var categorias = await GetAllAsync();

            if (!string.IsNullOrEmpty(categoriasParameters.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasParameters.Nome));
            }

            // var categoriasFiltradas = PagedList<Categoria>.ToPagedList(categorias.AsQueryable(), categoriasParams.PageNumber, categoriasParams.PageSize);

            var categoriasFiltradas = await categorias.ToPagedListAsync(categoriasParameters.PageNumber,
                                                           categoriasParameters.PageSize);
            
            return categoriasFiltradas;
        }
    }
}
