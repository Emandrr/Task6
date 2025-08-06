
using Microsoft.EntityFrameworkCore;

namespace Task6.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Presentation> Presentations { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<TextElement> TextElements { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Presentation>()
                .HasMany(p => p.Slides)
                .WithOne(s => s.Presentation)
                .HasForeignKey(s => s.PresentationId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Slide>()
                .HasMany(s => s.TextElements)
                .WithOne(t => t.Slide)
                .HasForeignKey(t => t.SlideId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Slide>()
                .HasIndex(s => s.PresentationId);

            modelBuilder.Entity<TextElement>()
                .HasIndex(t => t.SlideId);
           
        }
    }

}
