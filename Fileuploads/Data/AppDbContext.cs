using Microsoft.EntityFrameworkCore;
using Fileuploads.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<UploadedFile> UploadedFiles { get; set; }
    public DbSet<UploadedFileInfo> UploadedFileInfos { get; set; }
    public DbSet<TeamaptUploadedFilesInfo> TeamaptUploadedFilesInfo { get; set; }
    public DbSet<FairmoneyUploadedFilesInfo> FairmoneyUploadedFilesInfo { get; set; }
    public DbSet<PalmpayUploadedFilesInfo> PalmpayUploadedFilesInfo { get; set; }

    public DbSet<FairmoneyUploadedFile> FairmoneyUploadedFile { get; set; }

    public DbSet<PalmpayUploadedFile> PalmpayUploadedFile { get; set; }
    public DbSet<TeamaptUploadedFile> TeamaptUploadedFile { get; set; }


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
