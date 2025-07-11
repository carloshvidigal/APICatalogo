using APICatalogo.Models;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace APICatalogo.DTOs.Mappings
{
    public static class CategoriaDTOMappingExtensions
    {
        public static CategoriaDTO? ToCategoriaDTO(this Categoria categoria)
        {
            if(categoria is null) {return  null;}

            return new CategoriaDTO
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                ImagemURL = categoria.ImagemURL
            };
        }

        public static Categoria? ToCategoria(this CategoriaDTO categoriaDTO)
        {
              if(categoriaDTO is null)  return null;

            return new Categoria
            {
                Id = categoriaDTO.Id,
                Nome = categoriaDTO.Nome,
                ImagemURL = categoriaDTO.ImagemURL
            };
        }

        public static IEnumerable<CategoriaDTO> ToCategoriaDTOList(this IEnumerable<Categoria> categorias)
        {
            if (categorias is null || !categorias.Any())
            {
                return new List<CategoriaDTO>();
            }

            return categorias.Select(categoria => new CategoriaDTO
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                ImagemURL = categoria.ImagemURL
            }).ToList();
        }
    }
}
