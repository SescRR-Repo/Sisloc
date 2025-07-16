using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Sisloc.Helpers
{
    /// <summary>
    /// Representa uma lista paginada de itens de um determinado tipo T.
    /// </summary>
    public class PageList<T> : List<T>
    {
        /// <summary>
        /// Número da página atual (base 1).
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Total de páginas disponíveis.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Total de itens na fonte de dados.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Indica se existe página anterior.
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Indica se existe próxima página.
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// Construtor para criar uma instância de PageList.
        /// </summary>
        /// <param name="items">Lista de itens da página atual.</param>
        /// <param name="count">Total de itens na fonte de dados.</param>
        /// <param name="pageIndex">Número da página atual.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        public PageList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = count > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 1;

            this.AddRange(items);
        }

        /// <summary>
        /// Cria um PageList a partir de uma fonte IQueryable, aplicando paginação.
        /// </summary>
        /// <param name="source">Fonte de dados (IQueryable).</param>
        /// <param name="pageIndex">Número da página (inicia em 1).</param>
        /// <param name="pageSize">Quantidade de itens por página.</param>
        /// <returns>PageList contendo apenas os itens da página solicitada.</returns>
        public static async Task<PageList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            // Validações
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            // Conta total de registros
            var count = await source.CountAsync();

            // Obtém somente os itens da página
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageList<T>(items, count, pageIndex, pageSize);
        }

        /// <summary>
        /// Cria um PageList vazio para situações de erro ou consultas sem resultados.
        /// </summary>
        /// <param name="pageIndex">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>PageList vazio.</returns>
        public static PageList<T> CreateEmpty(int pageIndex = 1, int pageSize = 10)
        {
            return new PageList<T>(new List<T>(), 0, pageIndex, pageSize);
        }
    }
}