using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigDataProject.Entities.UserForm
{
    [Table("Document")]
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public byte[] Stream { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public DateTime CreatedOnUtc { get; set; }
    }
}
