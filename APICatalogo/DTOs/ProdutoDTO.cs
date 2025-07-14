using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APICatalogo.DTOs
{
    public class ProdutoDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(300)]
        public string? Descricao { get; set; }

        [Required]
        public decimal Preco { get; set; }

        [Required]
        [StringLength(300)]
        public string? ImagemURL { get; set; }

        public int CategoriaId { get; set; }
    }
}
