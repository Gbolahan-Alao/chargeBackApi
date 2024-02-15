namespace FileUpload.Models
{
    public class UploadedFileInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int TotalItems { get; set; }
        public DateTime UploadDate { get; set; }
        public string FileUrl { get; set; }
    }
}
