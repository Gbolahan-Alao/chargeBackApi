using Microsoft.EntityFrameworkCore;
using Fileuploads.Models;

public class FileUploadDbContext : DbContext
{
    public FileUploadDbContext(DbContextOptions<FileUploadDbContext> options)
        : base(options)
    {
    }

    public DbSet<UploadedFile> UploadedFiles { get; set; }
    public DbSet<UploadedFileInfo> UploadedFileInfos { get; set; }
    public DbSet<Merchant> Merchants { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>() // Define FileMetadata entity
            .HasKey(fm => fm.Id); // Define primary key

        modelBuilder.Entity<FileMetadata>() // Define UploadedFileId as foreign key
            .HasOne(fm => fm.UploadedFile)
            .WithMany(uf => uf.FileMetadata)
            .HasForeignKey(fm => fm.UploadedFileId);

        modelBuilder.Entity<UploadedFile>() // Configure UploadedFile entity
            .Property(f => f.TerminalId)
            .IsRequired(); // Configure TerminalId as non-nullable

        base.OnModelCreating(modelBuilder);
    }
}
