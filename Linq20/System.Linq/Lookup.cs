
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public class Lookup<TKey, TElement>
        : ILookup<TKey, TElement>
    {

        private Dictionary<TKey, IGrouping<TKey, TElement>> _groups;
        private IGrouping<TKey, TElement> _nullKeyElements;

        #region Internal Members

        internal Lookup(IEqualityComparer<TKey> comparer)
        {
            _groups = new Dictionary<TKey, IGrouping<TKey, TElement>>(comparer);
        }

        internal void Add(TKey key, TElement element)
        {
            IGrouping<TKey, TElement> group = null;

            if (key == null)
            {
                if (_nullKeyElements == null)
                    _nullKeyElements = new Grouping<TKey, TElement>(default(TKey), null);
            }
            else if (!_groups.TryGetValue(key, out group))
            {
                group = new Grouping<TKey, TElement>(key, null);
                _groups.Add(key, group);
            }

            (group as Grouping<TKey, TElement>).Add(element);
        }

        #endregion

        #region ILookup<TKey,TElement> Members

        public int Count
        {
            get { return _groups.Count; }
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                IGrouping<TKey, TElement> result;
                return _groups.TryGetValue(key, out result)
                        ? result
                        : Enumerable.Empty<TElement>();
            }
        }

        public bool Contains(TKey key)
        {
            return _groups.ContainsKey(key);
        }

        #endregion

        #region IEnumerable<IGrouping<TKey,TElement>> Members

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return _groups.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
