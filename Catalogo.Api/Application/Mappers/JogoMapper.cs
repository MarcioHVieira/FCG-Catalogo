using Catalogo.Api.Application.DTOs;
using Catalogo.Api.Domain.Entities;
using Catalogo.Api.Infrastructure.Search.Models;
using Fcg.Common.Enums;

namespace Catalogo.Api.Application.Mappers
{
    public static class JogoMapper
    {
        public static Jogo ToDomain(this JogoAdicionarDto jogoDto)
        {
            return Jogo.CriarAlterar(null, jogoDto.Titulo, jogoDto.Descricao,
                                     jogoDto.Genero, jogoDto.Valor);
        }

        public static Jogo ToDomain(this JogoAlterarDto jogoDto)
        {
            return Jogo.CriarAlterar(jogoDto.Id, jogoDto.Titulo, jogoDto.Descricao,
                                     jogoDto.Genero, jogoDto.Valor);
        }

        public static JogoResponseDto ToDto(this Jogo jogo)
        {
            return new JogoResponseDto
            {
                Id = jogo.Id,
                Titulo = jogo.Titulo,
                Descricao = jogo.Descricao,
                Genero = jogo.Genero,
                Valor = jogo.Valor,
                Status = jogo.Ativo ? "Ativado" : "Desativado"
            };
        }

        public static JogoResponseDto ToDto(this JogoElastic jogo)
        {
            return new JogoResponseDto
            {
                Id = jogo.Id,
                Titulo = jogo.Titulo,
                Descricao = jogo.Descricao,
                Genero = (Genero)Enum.Parse(typeof(Genero), jogo.Genero),
                Valor = jogo.Valor,
                Status = jogo.Status
            };
        }

        public static JogoElastic ToElastic(this Jogo jogo)
        {
            return new JogoElastic
            {
                Id = jogo.Id,
                Titulo = jogo.Titulo,
                Descricao = jogo.Descricao,
                Genero = Enum.GetName(typeof(Genero), jogo.Genero),
                Valor = jogo.Valor,
                Status = jogo.Ativo ? "Ativo" : "Inativo"
            };
        }
    }
}
