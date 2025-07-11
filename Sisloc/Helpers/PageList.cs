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
        /// Indica se existe página anterior.
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Indica se existe próxima página.
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        private PageList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

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
            // Conta total de registros
            var count = await source.CountAsync();

            // Obtém somente os itens da página
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PageList<T>(items, count, pageIndex, pageSize);
        }
    }
}
