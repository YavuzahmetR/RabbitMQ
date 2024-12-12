using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RabbitMQ_Watermark.Web.Models
{
    public sealed class Product
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = default!;

        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0,50)]
        public short Stock { get; set; }

        [StringLength(100)]
        public string? ImageName { get; set; }
    }
}
