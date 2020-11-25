using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SmartHome.Classes
{
    public class Pagination<T> : IEnumerable<T>
    {
        public int Index { get; private set; }
        public int Total { get; private set; }
        private IEnumerable<T> _enum { get; set; }

        public Pagination(IEnumerable<T> items, int count, int index, int pages)
        {
            Index = index;
            Total = (int)Math.Ceiling(count / (double)pages);

            _enum = items;
        }

        public bool HasPrevious { get => Index > 1; }
        public bool HasNext { get => Index < Total; }

        public static Pagination<T> CreateAsync(IEnumerable<T> source, int index, int pages)
        {
            var count = source.Count();
            var items = source.Skip((index - 1) * pages).Take(pages);
            return new Pagination<T>(items, count, index, pages);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enum.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _enum.GetEnumerator();
        }
    }
}
