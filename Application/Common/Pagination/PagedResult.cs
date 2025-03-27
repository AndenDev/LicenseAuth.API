using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Pagination
{
    public class PagedResult<TResult>
    {
  
        public IEnumerable<TResult> Items { get; set; } = [];
        public int TotalPages { get; set; } = 0;
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public bool HasMore { get; set; } = false;
        public int TotalCount { get; set; } = 0;
    }
}
