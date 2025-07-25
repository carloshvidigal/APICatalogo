using APICatalogo.Models;
using APICatalogo.Pagination;
using System.Runtime.InteropServices;

namespace APICatalogo.Repositories
{
    public interface ICategoriaRepository : IRepository<Categoria>
    {
        PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParams);

        PagedList<Categoria> GetCategoriasFiltroNome (CategoriasFiltroNome categoriasParams);
    }
}
