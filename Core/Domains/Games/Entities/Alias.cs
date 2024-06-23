using Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domains.Games.Entities
{
    public class Alias : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Game;

        public int PlayerId { get; set; }
        public Player Player { get; set; }
        [MaxLength(100)]
        public string OcrText { get; set; }
        [MaxLength(20)]
        public string TextType { get; set; }

        public static readonly string TextType_IGN = "ign";
    }
}