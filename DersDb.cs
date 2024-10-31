using Microsoft.EntityFrameworkCore;

class DersDb : DbContext
{
    public DersDb(DbContextOptions<DersDb> options)
        : base(options) { }
    
    public DbSet<Ders> dersler => Set<Ders>();
}