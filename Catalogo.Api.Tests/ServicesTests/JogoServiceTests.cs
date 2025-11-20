using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Catalogo.Api.Application.Services;
using Catalogo.Api.Application.DTOs;
using Catalogo.Api.Domain.Entities;
using Catalogo.Api.Domain.Interfaces;
using Catalogo.Api.Infrastructure.Search.Services;
using Catalogo.Api.Application.Mappers;
using Fcg.Common.Enums;
using Catalogo.Api.Infrastructure.Search.Models;

namespace Catalogo.Api.Tests.ServicesTests
{
    public class JogoServiceTests
    {
        private readonly Mock<IJogoRepository> _jogoRepositoryMock;
        private readonly Mock<ISearchService> _searchServiceMock;
        private readonly Mock<ILogger<JogoService>> _loggerMock;
        private readonly JogoService _service;

        public JogoServiceTests()
        {
            _jogoRepositoryMock = new Mock<IJogoRepository>();
            _searchServiceMock = new Mock<ISearchService>();
            _loggerMock = new Mock<ILogger<JogoService>>();
            _service = new JogoService(_jogoRepositoryMock.Object, _searchServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ObterJogo_DeveRetornarJogo_QuandoEncontrado()
        {
            var jogoId = Guid.NewGuid();
            var jogo = Jogo.CriarAlterar(jogoId, "Jogo Teste", "Jogo para realizar testes", Genero.Acao, 0);
            _jogoRepositoryMock.Setup(r => r.ObterPorId(jogoId)).ReturnsAsync(jogo);

            var result = await _service.ObterJogo(jogoId);

            Assert.NotNull(result);
            Assert.Equal(jogoId, result.Id);
        }

        [Fact]
        public async Task ObterJogo_DeveLancarExcecao_QuandoNaoEncontrado()
        {
            var jogoId = Guid.NewGuid();
            _jogoRepositoryMock.Setup(r => r.ObterPorId(jogoId)).ReturnsAsync((Jogo)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObterJogo(jogoId));
        }

        [Fact]
        public async Task ObterJogoPorTitulo_DeveRetornarJogo_QuandoEncontrado()
        {
            var titulo = "Test";
            var jogo = Jogo.CriarAlterar(null, titulo, "Jogo para realizar testes", Genero.Acao, 0);
            _jogoRepositoryMock.Setup(r => r.ObterPorTitulo(titulo)).ReturnsAsync(jogo);

            var result = await _service.ObterJogoPorTitulo(titulo);

            Assert.NotNull(result);
            Assert.Equal(titulo, result.Titulo);
        }

        [Fact]
        public async Task ObterJogoPorTitulo_DeveLancarExcecao_QuandoNaoEncontrado()
        {
            var titulo = "Test";
            _jogoRepositoryMock.Setup(r => r.ObterPorTitulo(titulo)).ReturnsAsync((Jogo)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObterJogoPorTitulo(titulo));
        }

        [Fact]
        public async Task ObterJogos_DeveRetornarListaDeJogos()
        {
            var jogos = new List<Jogo>
            {
                Jogo.CriarAlterar(null, "Jogo Teste A", "Jogo A para realizar testes", Genero.Acao, 0),
                Jogo.CriarAlterar(null, "Jogo Teste B", "Jogo B para realizar testes", Genero.Acao, 0)
            };
            _jogoRepositoryMock.Setup(r => r.ObterTodos()).ReturnsAsync(jogos);

            var result = await _service.ObterJogos();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ObterJogosAtivos_DeveRetornarListaDeJogosAtivos()
        {
            var jogos = new List<Jogo>
            {
                Jogo.CriarAlterar(null, "Jogo Teste A", "Jogo A para realizar testes", Genero.Acao, 0),
                Jogo.CriarAlterar(null, "Jogo Teste B", "Jogo B para realizar testes", Genero.Acao, 0)
            };
            _jogoRepositoryMock.Setup(r => r.ObterTodosAtivos()).ReturnsAsync(jogos);

            var result = await _service.ObterJogosAtivos();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AdicionarJogo_DeveAdicionarJogo_QuandoValido()
        {
            var dto = new JogoAdicionarDto { Titulo = "Jogo Teste", Descricao = "Descrição do Jogo", Genero = Genero.Aventura, Valor = 0 };
            var jogo = dto.ToDomain();
            _jogoRepositoryMock.Setup(r => r.Existe(It.IsAny<Guid>(), dto.Titulo)).ReturnsAsync(false);
            _jogoRepositoryMock.Setup(r => r.Adicionar(It.IsAny<Jogo>())).Returns(Task.CompletedTask);
            _searchServiceMock.Setup(s => s.IndexarJogoAsync(It.IsAny<JogoElastic>())).Returns(Task.CompletedTask);

            await _service.AdicionarJogo(dto);

            _jogoRepositoryMock.Verify(r => r.Adicionar(It.IsAny<Jogo>()), Times.Once);
            _searchServiceMock.Verify(s => s.IndexarJogoAsync(It.IsAny<JogoElastic>()), Times.Once);
        }

        [Fact]
        public async Task AdicionarJogo_DeveLancarExcecao_QuandoGeneroInvalido()
        {
            var dto = new JogoAdicionarDto { Titulo = "Jogo Teste", Descricao = "Descrição do Jogo", Genero = (Genero)999, Valor = 0 };
            var jogo = dto.ToDomain();

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarJogo(dto));
        }

        [Fact]
        public async Task AdicionarJogo_DeveLancarExcecao_QuandoJogoJaExiste()
        {
            var dto = new JogoAdicionarDto { Titulo = "Novo", Descricao = "Descrição do Jogo", Genero = Genero.Aventura, Valor = 0 };
            var jogo = dto.ToDomain();
            _jogoRepositoryMock.Setup(r => r.Existe(It.IsAny<Guid>(), dto.Titulo)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AdicionarJogo(dto));
        }

        [Fact]
        public async Task AlterarJogo_DeveAlterarJogo_QuandoValido()
        {
            var dto = new JogoAlterarDto { Id = Guid.NewGuid(), Titulo = "Alterado", Descricao = "Descrição do Jogo", Genero = Genero.Aventura, Valor = 0 };
            _jogoRepositoryMock.Setup(r => r.Existe(dto.Id, dto.Titulo)).ReturnsAsync(false);
            _jogoRepositoryMock.Setup(r => r.Alterar(It.IsAny<Jogo>(), It.IsAny<bool>())).Returns(Task.CompletedTask);
            _searchServiceMock.Setup(s => s.IndexarJogoAsync(It.IsAny<JogoElastic>())).Returns(Task.CompletedTask);

            await _service.AlterarJogo(dto);

            _jogoRepositoryMock.Verify(r => r.Alterar(It.IsAny<Jogo>(), It.IsAny<bool>()), Times.Once);
            _searchServiceMock.Verify(s => s.IndexarJogoAsync(It.IsAny<JogoElastic>()), Times.Once);
        }

        [Fact]
        public async Task AlterarJogo_DeveLancarExcecao_QuandoGeneroInvalido()
        {
            var dto = new JogoAlterarDto { Id = Guid.NewGuid(), Titulo = "Alterado", Descricao = "Descrição do Jogo", Genero = (Genero)999, Valor = 0 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AlterarJogo(dto));
        }

        [Fact]
        public async Task AlterarJogo_DeveLancarExcecao_QuandoJogoJaExiste()
        {
            var dto = new JogoAlterarDto { Id = Guid.NewGuid(), Titulo = "Alterado", Descricao = "Descrição do Jogo", Genero = Genero.Aventura, Valor = 0 };
            _jogoRepositoryMock.Setup(r => r.Existe(dto.Id, dto.Titulo)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AlterarJogo(dto));
        }

        [Fact]
        public async Task AtivarJogo_DeveChamarRepositorioELogar()
        {
            var jogoId = Guid.NewGuid();
            _jogoRepositoryMock.Setup(r => r.Ativar(jogoId)).Returns(Task.CompletedTask);

            await _service.AtivarJogo(jogoId);

            _jogoRepositoryMock.Verify(r => r.Ativar(jogoId), Times.Once);
        }

        [Fact]
        public async Task DesativarJogo_DeveChamarRepositorioELogar()
        {
            var jogoId = Guid.NewGuid();
            _jogoRepositoryMock.Setup(r => r.Desativar(jogoId)).Returns(Task.CompletedTask);

            await _service.DesativarJogo(jogoId);

            _jogoRepositoryMock.Verify(r => r.Desativar(jogoId), Times.Once);
        }
    }
}
