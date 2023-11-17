namespace MagicVilla_Web.Models.DTO
{
    public class PaginatedListDTO
    {
        public PaginatedListDTO(IEnumerable<object> items, int count, int pageNumber, int pageSize, string searchString)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            EntriesCount = count;
            SearchString = searchString;
        }
        public string SearchString { get; set; }
        public int EntriesCount { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<object> Items { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
