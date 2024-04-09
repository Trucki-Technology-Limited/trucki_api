using Microsoft.EntityFrameworkCore;

namespace trucki.Models.ResponseModels
{
    public class PaginatedListDto<T>
    {
        public PageMeta MetaData { get; set; }
        public IEnumerable<T> Data { get; set; }
        public PaginatedListDto()
        {
            Data = new List<T>();
        }
    }
    public class PageMeta
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }
    public static class PagedList<T>
    {
        public static async Task<PaginatedListDto<T>> PaginatesAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 1 : pageSize;
            var total = await source.CountAsync();
            var paginatedList = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var pagedMeta = CreatePageMeta(pageNumber, pageSize, total);
            return new PaginatedListDto<T>
            {
                MetaData = pagedMeta,
                Data = paginatedList
            };
        }
        public static PageMeta CreatePageMeta(int pageNumber, int pageSize, int total)
        {
            var total_pages = total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
            return new PageMeta
            {
                Page = pageNumber,
                PerPage = pageSize,
                Total = total,
                TotalPages = total_pages
            };
        }

        public static PaginatedListDto<T> Paginate(IQueryable<T> source, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 1 : pageSize;
            var paginatedList = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var pagedMeta = CreatePageMeta(pageNumber, pageSize, source.Count());
            return new PaginatedListDto<T>
            {
                MetaData = pagedMeta,
                Data = paginatedList
            };
        }

        public static PaginatedListDto<T> Paginates(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (!source.Any())
                return new PaginatedListDto<T>
                {
                    Data = new List<T>()
                };
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? source.Count() : pageSize;
            var paginatedList = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var pagedMeta = CreatePageMeta(pageNumber, pageSize, source.ToList().Count());
            return new PaginatedListDto<T>
            {
                MetaData = pagedMeta,
                Data = paginatedList
            };
        }

        public static PaginatedListDto<T> Paginates(IEnumerable<T> source, int pageNumber, int pageSize, int totalCount)
        {
            if (!source.Any())
                return new PaginatedListDto<T>
                {
                    Data = new List<T>()
                };
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? source.Count() : pageSize;
            var paginatedList = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var pagedMeta = CreatePageMeta(pageNumber, pageSize, totalCount);
            return new PaginatedListDto<T>
            {
                MetaData = pagedMeta,
                Data = paginatedList
            };
        }

        //public static PaginatedListDto<T> Fail(string errorMessage)
        //{
        //    return new 
        //}
    }
}
