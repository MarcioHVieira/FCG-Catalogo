using Catalogo.Api.Domain.Entities;
using Fcg.Common.Entities;
using Fcg.Common.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Infrastructure.Data
{
    public class CatalogoDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Jogo> Jogos { get; set; }

        public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("catalogo");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogoDbContext).Assembly);
        }

        public async Task Salvar(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                if (entry.State == EntityState.Added)
                    entry.Property("DataCadastro").CurrentValue = DateTime.UtcNow;

                if (entry.State == EntityState.Modified)
                    entry.Property("DataCadastro").IsModified = false;
            }

            var salvo = await base.SaveChangesAsync(cancellationToken) > 0;

            if (!salvo)
                throw new DbUpdateException("Houve um erro ao tentar persistir os dados");
        }
    }
}
