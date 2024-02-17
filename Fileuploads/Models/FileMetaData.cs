using System;

namespace Fileuploads.Models { 
    public class FileMetadata
    {
        public int Id { get; set; }
        public int UploadedFileId { get; set; } // Foreign key to link to the corresponding UploadedFile
        public string FileName { get; set; }
        public DateTime UploadDateTime { get; set; }
        public string DownloadLink { get; set; }
        public UploadedFile UploadedFile { get; set; } // Navigation property to reference UploadedFilepublic int TotalItems { get; set; } 
        public int TotalItems { get; set; }


    }
}
