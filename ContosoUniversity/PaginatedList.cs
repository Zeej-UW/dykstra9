using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity
{
    public class PaginatedList<T> : List<T>
    {
        // getters/setters for pageindex and total pages
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        // constructor
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        // use for previous page button
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        // use for next page button
        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }
        // Method to return only the requested page(s). Will takeover the construction of the object as 
        // construction can't be done asynchronously.
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}