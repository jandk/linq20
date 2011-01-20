
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    internal class Grouping<TKey, TElement>
        : IGrouping<TKey, TElement>
    {
        private TKey _key;
        private IList<TElement> _elements;

        #region Internal Members

        internal Grouping(TKey key, IList<TElement> elements)
        {
            _key = key;
            _elements = elements;
        }

        internal void Add(TElement element)
        {
            if (_elements == null)
                _elements = new List<TElement>();

            _elements.Add(element);
        }

        #endregion

        #region IGrouping<TKey,TElement> Members

        public TKey Key
        {
            get { return _key; }
        }

        #endregion

        #region IEnumerable<TElement> Members

        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
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
