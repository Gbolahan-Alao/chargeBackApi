using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileUpload.Models
{
    public class PalmpayUploadedFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string MaskedPan { get; set; }
        public string Rrn { get; set; }
        public string Stan { get; set; }
        public string TerminalId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string AccountToBeCredited { get; set; }


    }

}
