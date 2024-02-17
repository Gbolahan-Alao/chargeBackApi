namespace Fileuploads.Models
{
    public class TeamaptUploadedFilesInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int TotalItems { get; set; }

        public int TotalSuccessful {  get; set; }

        public int TotalFailed { get; set; }

        public DateTime UploadDate { get; set; }
        public string FileUrl { get; set; }

        
    }
}
