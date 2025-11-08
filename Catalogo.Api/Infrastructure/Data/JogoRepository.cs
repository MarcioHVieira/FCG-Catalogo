using Fcg.Common.Repositories;
using Catalogo.Api.Domain.Entities;
using Catalogo.Api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Infrastructure.Data
{
    public class JogoRepository : RepositoryBase<Jogo, CatalogoDbContext>, IJogoRepository
    {
        private readonly CatalogoDbContext _context;

        public JogoRepository(CatalogoDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> Existe(Guid jogoId, string titulo)
        {
            return await _context.Jogos.AsNoTracking()
                .Where(j => j.Id != jogoId && j.Titulo == titulo).AnyAsync();
        }

        public async Task<Jogo?> ObterPorTitulo(string titulo)
        {
            return await _context.Jogos.AsNoTracking()
                .FirstOrDefaultAsync(j => j.Titulo == titulo);
        }
    }
}
