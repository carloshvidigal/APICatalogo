using System.Collections.ObjectModel;

namespace APICatalogo.Models
{
    public class Categoria
    {
        public Categoria() 
        { 
            Produtos = new Collection<Produto>();
        }
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? ImagemURL { get; set; }
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
        public ICollection<Produto> Produtos { get; set; } 

    }
}
