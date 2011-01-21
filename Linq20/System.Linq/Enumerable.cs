
using System;
using System.Collections.Generic;
using System.Collections;

namespace System.Linq
{
    public static class Enumerable
    {

        #region Helpers

        enum Fallback
        {
            Default,
            Throw
        }

        class EmptyEnumerable<TElement>
        {
            private static TElement[] _instance;

            public static IEnumerable<TElement> Instance
            {
                get { return _instance ?? (_instance = new TElement[0]); }
            }
        }

        #endregion

        #region Check

        static class Check
        {
            public static void FirstAndSecond(object first, object second)
            {
                if (first == null)
                    throw new ArgumentNullException("first");
                if (second == null)
                    throw new ArgumentNullException("second");
            }

            public static void JoinSelectors(object outer, object inner, object outerKeySelector, object innerKeySelector, object resultSelector)
            {
                if (outer == null)
                    throw new ArgumentNullException("outer");
                if (inner == null)
                    throw new ArgumentNullException("inner");
                if (outerKeySelector == null)
                    throw new ArgumentNullException("outerKeySelector");
                if (innerKeySelector == null)
                    throw new ArgumentNullException("innerKeySelector");
                if (resultSelector == null)
                    throw new ArgumentNullException("resultSelector");
            }

            public static void Source(object source)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
            }

            public static void SourceAndCollectionSelectors(object source, object collectionSelector, object resultSelector)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (collectionSelector == null)
                    throw new ArgumentNullException("collectionSelector");
                if (resultSelector == null)
                    throw new ArgumentNullException("resultSelector");
            }

            public static void SourceAndFunc(object source, object func)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (func == null)
                    throw new ArgumentNullException("func");
            }

            public static void SourceAndKeyElementSelectors(object source, object keySelector, object elementSelector)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (keySelector == null)
                    throw new ArgumentNullException("keySelector");
                if (elementSelector == null)
                    throw new ArgumentNullException("elementSelector");
            }

            public static void SourceAndKeySelector(object source, object keySelector)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (keySelector == null)
                    throw new ArgumentNullException("keySelector");
            }

            public static void SourceAndPredicate(object source, object predicate)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (predicate == null)
                    throw new ArgumentNullException("predicate");
            }

            public static void SourceAndSelector(object source, object selector)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (selector == null)
                    throw new ArgumentNullException("selector");
            }
        }

        #endregion

        #region Done - Aggregate

        public static TSource Aggregate<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, TSource> func
        )
        {
            Check.SourceAndFunc(source, func);

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("No elements in source list.");

                TSource agg = enumerator.Current;
                while (enumerator.MoveNext())
                    agg = func(agg, enumerator.Current);

                return agg;
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func
        )
        {
            return Aggregate(source, seed, func, x => x);
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector
        )
        {
            Check.SourceAndFunc(source, func);
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");

            var agg = seed;
            foreach (var item in source)
                agg = func(agg, item);

            return resultSelector(agg);
        }

        #endregion

        #region Done - All

        public static bool All<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            foreach (var item in source)
                if (!predicate(item))
                    return false;

            return true;
        }

        #endregion

        #region Done - Any

        public static bool Any<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Count > 0;

            using (var enumerator = source.GetEnumerator())
                return enumerator.MoveNext();
        }

        public static bool Any<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            foreach (var item in source)
                if (predicate(item))
                    return true;

            return false;
        }

        #endregion

        #region Done - AsEnumerable

        public static IEnumerable<TSource> AsEnumerable<TSource>(
            this IEnumerable<TSource> source
        )
        {
            return source;
        }

        #endregion

        #region Done - Average

        #region Helpers

        static TResult Average<TElement, TAggregate, TResult>(
            this IEnumerable<TElement> source,
            Func<TAggregate, TElement, TAggregate> func,
            Func<TAggregate, long, TResult> result
        )
            where TElement : struct
            where TAggregate : struct
            where TResult : struct
        {
            Check.Source(source);

            var sum = default(TAggregate);
            long count = 0;

            foreach (var item in source)
            {
                sum = func(sum, item);
                count++;
            }

            if (count == 0)
                throw new InvalidOperationException();

            return result(sum, count);
        }

        static TResult? AverageNullable<TElement, TAggregate, TResult>(
            this IEnumerable<TElement?> source,
            Func<TAggregate, TElement, TAggregate> func,
            Func<TAggregate, long, TResult> result
        )
            where TElement : struct
            where TAggregate : struct
            where TResult : struct
        {
            Check.Source(source);

            var sum = default(TAggregate);
            long count = 0;

            foreach (var item in source)
            {
                if (!item.HasValue)
                    continue;


                sum = func(sum, item.Value);
                count++;
            }

            if (count == 0)
                return null;

            return result(sum, count);
        }

        #endregion

        #region Normal

        public static double Average(
            this IEnumerable<int> source
        )
        {
            return Average<int, long, double>(source, (a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average(
            this IEnumerable<long> source
        )
        {
            return Average<long, long, double>(source, (a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average(
            this IEnumerable<double> source
        )
        {
            return Average<double, double, double>(source, (a, b) => a + b, (a, b) => a / b);
        }

        public static float Average(
            this IEnumerable<float> source
        )
        {
            return Average<float, double, float>(source, (a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static decimal Average(
            this IEnumerable<decimal> source
        )
        {
            return Average<decimal, decimal, decimal>(source, (a, b) => a + b, (a, b) => a / b);
        }

        #endregion

        #region Nullable

        public static double? Average(
            this IEnumerable<int?> source
        )
        {
            Check.Source(source);

            return source.AverageNullable<int, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double? Average(
            this IEnumerable<long?> source
        )
        {
            Check.Source(source);

            return source.AverageNullable<long, long, double>((a, b) => a + b, (a, b) => (double)a / b);
        }

        public static double? Average(
            this IEnumerable<double?> source
        )
        {
            Check.Source(source);

            return source.AverageNullable<double, double, double>((a, b) => a + b, (a, b) => a / b);
        }

        public static float? Average(
            this IEnumerable<float?> source
        )
        {
            Check.Source(source);

            return source.AverageNullable<float, double, float>((a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static decimal? Average(
            this IEnumerable<decimal?> source
        )
        {
            Check.Source(source);

            return source.AverageNullable<decimal, decimal, decimal>((a, b) => a + b, (a, b) => a / b);
        }

        #endregion

        #region Normal - Selector

        public static double Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<int, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<long, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<double, double, double>((a, b) => a + b, (a, b) => a / b);
        }

        public static float Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<float, double, float>((a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static decimal Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<decimal, decimal, decimal>((a, b) => a + b, (a, b) => a / b);
        }

        #endregion

        #region Nullable - Selector

        public static double? Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int?> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<int, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double? Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long?> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<long, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double? Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double?> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<double, double, double>((a, b) => a + b, (a, b) => a / b);
        }

        public static float? Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float?> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<float, double, float>((a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static decimal? Average<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal?> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<decimal, decimal, decimal>((a, b) => a + b, (a, b) => a / b);
        }

        #endregion

        #endregion

        #region Done - Cast

        public static IEnumerable<TResult> Cast<TResult>(
            this IEnumerable source
        )
        {
            Check.Source(source);

            var castSource = source as IEnumerable<TResult>;
            if (castSource != null)
                return castSource;

            return YieldCast<TResult>(source);
        }

        static IEnumerable<TResult> YieldCast<TResult>(IEnumerable source)
        {
            foreach (var item in source)
                yield return (TResult)item;
        }

        #endregion

        #region Done - Concat

        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
        )
        {
            Check.FirstAndSecond(first, second);

            return YieldConcat(first, second);
        }

        private static IEnumerable<TSource> YieldConcat<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second
        )
        {
            foreach (var item in first)
                yield return item;

            foreach (var item in second)
                yield return item;
        }

        #endregion

        #region Done - Contains

        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value
        )
        {
            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Contains(value);

            return Contains(source, value, null);
        }

        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer
        )
        {
            Check.Source(source);

            comparer = comparer ?? EqualityComparer<TSource>.Default;

            foreach (var item in source)
                if (comparer.Equals(item, value))
                    return true;

            return false;
        }

        #endregion

        #region Done - Count

        public static int Count<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Count;

            int count = 0;
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    checked { count++; }

            return count;
        }

        public static int Count<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            int count = 0;
            foreach (var item in source)
                if (predicate(item))
                    checked { count++; }

            return count;
        }

        #endregion

        #region Done - DefaultIfEmpty

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source
        )
        {
            return DefaultIfEmpty(source, default(TSource));
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source,
            TSource defaultValue
        )
        {
            Check.Source(source);

            return YieldDefaultIfEmpty(source, defaultValue);
        }

        static IEnumerable<TSource> YieldDefaultIfEmpty<TSource>(
            IEnumerable<TSource> source,
            TSource defaultValue
        )
        {
            using (var enumerator = source.GetEnumerator())
                if (!enumerator.MoveNext())
                    yield return defaultValue;
                else
                    do { yield return enumerator.Current; } while (enumerator.MoveNext());
        }

        #endregion

        #region Done - Distinct

        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source
        )
        {
            return Distinct(source, null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer
        )
        {
            Check.Source(source);

            comparer = comparer ?? EqualityComparer<TSource>.Default;

            return YieldDistinct(source, comparer);
        }

        static IEnumerable<TSource> YieldDistinct<TSource>(
            IEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer
        )
        {
            var hasNull = false;
            var items = new Dictionary<TSource, Object>(comparer);

            foreach (var item in source)
            {
                if (item == null)
                {
                    if (hasNull)
                        continue;

                    hasNull = true;
                }
                else
                {
                    if (items.ContainsKey(item))
                        continue;

                    items.Add(item, null);
                }

                yield return item;
            }
        }

        #endregion

        #region Done - ElementAt(OrDefault)

        public static TSource ElementAt<TSource>(
            this IEnumerable<TSource> source,
            int index
        )
        {
            Check.Source(source);

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            var list = source as IList<TSource>;
            if (list != null)
                return list[index];

            return ElementAt(source, index, Fallback.Throw);
        }

        public static TSource ElementAtOrDefault<TSource>(
            this IEnumerable<TSource> source,
            int index
        )
        {
            Check.Source(source);

            if (index < 0)
                return default(TSource);

            var list = source as IList<TSource>;
            if (list != null)
                return index < list.Count ? list[index] : default(TSource);

            return ElementAt(source, index, Fallback.Default);
        }


        static TSource ElementAt<TSource>(
            IEnumerable<TSource> source,
            int index,
            Fallback fallback
        )
        {
            int counter = 0;

            foreach (var item in source)
                if (index == counter++)
                    return item;

            if (fallback == Fallback.Throw)
                throw new ArgumentOutOfRangeException("index");

            return default(TSource);
        }

        #endregion

        #region Done - Empty

        public static IEnumerable<TResult> Empty<TResult>()
        {
            return EmptyEnumerable<TResult>.Instance;
        }

        #endregion

        #region Done - Except

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
        )
        {
            return Except(first, second, null);
        }

        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            Check.FirstAndSecond(first, second);

            comparer = comparer ?? EqualityComparer<TSource>.Default;

            return YieldExcept(first, second, comparer);
        }

        static IEnumerable<TSource> YieldExcept<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            // var items = new HashSet<TSource>(second, comparer);
            var items = new Dictionary<TSource, Object>(comparer);
            foreach (var item in second)
                items.Add(item, null);

            foreach (var item in first)
                if (!items.ContainsKey(item))
                    yield return item;
        }

        #endregion

        #region Done - First(OrDefault)

        public static TSource First<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                    return list[0];
            }
            else
            {
                using (var enumerator = source.GetEnumerator())
                    if (enumerator.MoveNext())
                        return enumerator.Current;
            }

            throw new InvalidOperationException();
        }

        public static TSource First<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return First(source, predicate, Fallback.Throw);
        }

        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            return First(source, x => true, Fallback.Default);
        }

        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return First(source, predicate, Fallback.Default);
        }

        static TSource First<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            Fallback fallback
        )
        {
            foreach (var item in source)
                if (predicate(item))
                    return item;

            if (fallback == Fallback.Throw)
                throw new InvalidOperationException();

            return default(TSource);
        }

        #endregion

        #region Done - GroupBy

        /*
         * We cannot yield a group until we are sure every item that needs to be in the group
         * is effectively in there. This means that a groupby has to run over the entire enumerable.
         * This means we can use ToLookup, and skip the delayed execution part.
         */

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return ToLookup(source, keySelector);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return ToLookup(source, keySelector, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector
        )
        {
            return ToLookup(source, keySelector, elementSelector);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return ToLookup(source, keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector
        )
        {
            return GroupBy(source, keySelector)
                .Select(g => resultSelector(g.Key, g));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector
        )
        {
            return GroupBy(source, keySelector, elementSelector)
                .Select(g => resultSelector(g.Key, g));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return GroupBy(source, keySelector, comparer)
                .Select(g => resultSelector(g.Key, g));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return GroupBy(source, keySelector, elementSelector, comparer)
                .Select(g => resultSelector(g.Key, g));
        }

        #endregion

        #region Done - GroupJoin

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector
        )
        {
            return GroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);

            comparer = comparer ?? EqualityComparer<TKey>.Default;

            return YieldGroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> YieldGroupJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            ILookup<TKey, TInner> innerKeys = ToLookup(inner, innerKeySelector, comparer);

            foreach (var item in outer)
            {
                var outerKey = outerKeySelector(item);

                if (innerKeys.Contains(outerKey))
                    yield return resultSelector(item, innerKeys[outerKey]);
                else
                    yield return resultSelector(item, Empty<TInner>());
            }
        }

        #endregion

        #region Done - Intersect

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
        )
        {
            return Intersect(first, second, null);
        }

        public static IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            Check.FirstAndSecond(first, second);

            comparer = comparer ?? EqualityComparer<TSource>.Default;

            return YieldIntersect(first, second, comparer);
        }

        static IEnumerable<TSource> YieldIntersect<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            // var items = new HashSet<TSource>(second, comparer);
            var items = new Dictionary<TSource, Object>(comparer);
            foreach (var item in second)
                items.Add(item, null);

            foreach (var item in first)
                if (items.Remove(item))
                    yield return item;
        }

        #endregion

        #region Done - Join

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector
        )
        {
            return Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);

            comparer = comparer ?? EqualityComparer<TKey>.Default;

            return YieldJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> YieldJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            ILookup<TKey, TInner> innerKeys = ToLookup(inner, innerKeySelector, comparer);

            foreach (var item in outer)
            {
                var outerKey = outerKeySelector(item);

                if (innerKeys.Contains(outerKey))
                    foreach (var innerElement in innerKeys[outerKey])
                        yield return resultSelector(item, innerElement);
            }
        }

        #endregion

        #region Done - Last(OrDefault)

        public static TSource Last<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            // Check if empty
            var collection = source as ICollection<TSource>;
            if (collection != null && collection.Count == 0)
                throw new InvalidOperationException();

            var list = source as IList<TSource>;
            if (list != null)
                return list[list.Count - 1];

            return Last(source, x => true, Fallback.Throw);
        }

        public static TSource Last<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return Last(source, predicate, Fallback.Throw);
        }

        public static TSource LastOrDefault<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var list = source as IList<TSource>;
            if (list != null)
                return list.Count > 0
                    ? list[list.Count - 1]
                    : default(TSource);

            return Last(source, x => true, Fallback.Default);
        }

        public static TSource LastOrDefault<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return Last(source, predicate, Fallback.Default);
        }

        static TSource Last<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            Fallback fallback
        )
        {
            var empty = true;
            var value = default(TSource);

            foreach (var item in source)
            {
                if (!predicate(item))
                    continue;

                empty = false;
                value = item;
            }

            if (!empty)
                return value;

            if (fallback == Fallback.Throw)
                throw new InvalidOperationException();

            return value;
        }

        #endregion

        #region Done - LongCount

        public static long LongCount<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var array = source as TSource[];
            if (array != null)
                return array.LongLength;

            long count = 0;
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    checked { count++; }

            return count;
        }

        public static long LongCount<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            long count = 0;
            foreach (var item in source)
                if (predicate(item))
                    checked { count++; }

            return count;
        }

        #endregion

        #region MinMax Helper

        static TSource? IterateNullable<TSource>(
            IEnumerable<TSource?> source,
            TSource seed,
            Func<TSource, TSource, TSource> selector
        )
            where TSource : struct
        {
            bool empty = true;
            TSource? value = seed;

            foreach (var item in source)
            {
                if (!item.HasValue)
                    continue;

                value = value.HasValue
                    ? selector(item.Value, value.Value)
                    : item.Value;

                empty = false;
            }

            return empty
                ? null
                : value;
        }

        #endregion

        #region Done - Max

        #region Normal

        public static int Max(
            this IEnumerable<int> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Max);
        }

        public static long Max(
            this IEnumerable<long> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Max);
        }

        public static double Max(
            this IEnumerable<double> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Max);
        }

        public static float Max(
            this IEnumerable<float> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Max);
        }

        public static decimal Max(
            this IEnumerable<decimal> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Max);
        }

        #endregion

        #region Nullable

        public static int? Max(
            this IEnumerable<int?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, int.MinValue, Math.Max);
        }

        public static long? Max(
            this IEnumerable<long?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, long.MinValue, Math.Max);
        }

        public static double? Max(
            this IEnumerable<double?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, double.MinValue, Math.Max);
        }

        public static float? Max(
            this IEnumerable<float?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, float.MinValue, Math.Max);
        }

        public static decimal? Max(
            this IEnumerable<decimal?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, decimal.MinValue, Math.Max);
        }

        #endregion

        #region Normal - Selector

        public static int Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static long Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static double Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static float Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static decimal Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal> selector
        )
        {
            return source.Select(selector).Max();
        }

        #endregion

        #region Nullable - Selector

        public static int? Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int?> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static long? Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long?> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static double? Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double?> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static float? Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float?> selector
        )
        {
            return source.Select(selector).Max();
        }

        public static decimal? Max<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal?> selector
        )
        {
            return source.Select(selector).Max();
        }

        #endregion

        public static TSource Max<TSource>(
            this IEnumerable<TSource> source
        )
        {
            var comparer = Comparer<TSource>.Default;

            return source.Aggregate((x, y) => comparer.Compare(x, y) > 0 ? x : y);
        }

        public static TResult Max<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Max();
        }

        #endregion

        #region Done - Min

        #region Normal

        public static int Min(
            this IEnumerable<int> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Min);
        }

        public static long Min(
            this IEnumerable<long> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Min);
        }

        public static double Min(
            this IEnumerable<double> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Min);
        }

        public static float Min(
            this IEnumerable<float> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Min);
        }

        public static decimal Min(
            this IEnumerable<decimal> source
        )
        {
            Check.Source(source);

            return Aggregate(source, Math.Min);
        }

        #endregion

        #region Nullable

        public static int? Min(
            this IEnumerable<int?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, int.MaxValue, Math.Min);
        }

        public static long? Min(
            this IEnumerable<long?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, long.MaxValue, Math.Min);
        }

        public static double? Min(
            this IEnumerable<double?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, double.MaxValue, Math.Min);
        }

        public static float? Min(
            this IEnumerable<float?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, float.MaxValue, Math.Min);
        }

        public static decimal? Min(
            this IEnumerable<decimal?> source
        )
        {
            Check.Source(source);

            return IterateNullable(source, decimal.MaxValue, Math.Min);
        }

        #endregion

        #region Normal - Selector

        public static int Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static long Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static double Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static float Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static decimal Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal> selector
        )
        {
            return source.Select(selector).Min();
        }

        #endregion

        #region Nullable - Selector

        public static int? Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int?> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static long? Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long?> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static double? Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double?> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static float? Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float?> selector
        )
        {
            return source.Select(selector).Min();
        }

        public static decimal? Min<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal?> selector
        )
        {
            return source.Select(selector).Min();
        }

        #endregion

        public static TSource Min<TSource>(
            this IEnumerable<TSource> source
        )
        {
            var comparer = Comparer<TSource>.Default;

            return source.Aggregate((x, y) => comparer.Compare(x, y) < 0 ? x : y);
        }

        public static TResult Min<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Min();
        }

        #endregion

        #region Done - OfType

        public static IEnumerable<TResult> OfType<TResult>(
            this IEnumerable source
        )
        {
            Check.Source(source);

            return YieldOfType<TResult>(source);
        }

        static IEnumerable<TResult> YieldOfType<TResult>(
            IEnumerable source
        )
        {
            foreach (var item in source)
                if (item is TResult)
                    yield return (TResult)item;
        }

        #endregion

        #region OrderBy(Descending)

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return OrderBy(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer
        )
        {
            Check.SourceAndKeySelector(source, keySelector);

            throw new NotImplementedException();
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return OrderByDescending(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer
        )
        {
            Check.SourceAndKeySelector(source, keySelector);

            throw new NotImplementedException();
        }

        #endregion

        #region Done - Range

        public static IEnumerable<int> Range(
            int start,
            int count
        )
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            long upto = ((long)start + count) - 1;

            if (upto > Int32.MaxValue)
                throw new ArgumentOutOfRangeException();

            return YieldRange(start, (int)upto);
        }

        static IEnumerable<int> YieldRange(
            int start,
            int upto
        )
        {
            for (int i = start; i <= upto; i++)
                yield return i;
        }

        #endregion

        #region Done - Repeat

        public static IEnumerable<TResult> Repeat<TResult>(
            TResult element,
            int count
        )
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            return YieldRepeat(element, count);
        }

        static IEnumerable<TSource> YieldRepeat<TSource>(TSource element, int count)
        {
            for (int i = 0; i < count; i++)
                yield return element;
        }

        #endregion

        #region Done - Reverse

        public static IEnumerable<TSource> Reverse<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var list = source as IList<TSource>
                ?? new List<TSource>(source);

            return YieldReverse(list);
        }

        static IEnumerable<TSource> YieldReverse<TSource>(
            IList<TSource> source
        )
        {
            for (int i = source.Count - 1; i >= 0; i--)
                yield return source[i];
        }

        #endregion

        #region Done - Select

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return YieldSelect(source, selector);
        }

        static IEnumerable<TResult> YieldSelect<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TResult> selector
        )
        {
            foreach (var item in source)
                yield return selector(item);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return YieldSelect(source, selector);
        }

        static IEnumerable<TResult> YieldSelect<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, int, TResult> selector
        )
        {
            int counter = 0;

            foreach (var item in source)
                yield return selector(item, counter++);
        }

        #endregion

        #region Done - SelectMany

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TResult>> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return YieldSelectMany(source, selector);
        }

        static IEnumerable<TResult> YieldSelectMany<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TResult>> selector
        )
        {
            foreach (var element in source)
                foreach (var item in selector(element))
                    yield return item;
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TResult>> selector
        )
        {
            Check.SourceAndSelector(source, selector);

            return YieldSelectMany(source, selector);
        }

        static IEnumerable<TResult> YieldSelectMany<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TResult>> selector
        )
        {
            int count = 0;

            foreach (var element in source)
                foreach (var item in selector(element, count++))
                    yield return item;
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector
        )
        {
            Check.SourceAndCollectionSelectors(source, collectionSelector, resultSelector);

            return YieldSelectMany(source, collectionSelector, resultSelector);
        }

        static IEnumerable<TResult> YieldSelectMany<TSource, TCollection, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector
        )
        {
            foreach (var item in source)
                foreach (var collection in collectionSelector(item))
                    yield return resultSelector(item, collection);
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector
        )
        {
            Check.SourceAndCollectionSelectors(source, collectionSelector, resultSelector);

            return YieldSelectMany(source, collectionSelector, resultSelector);
        }

        static IEnumerable<TResult> YieldSelectMany<TSource, TCollection, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector
        )
        {
            int count = 0;

            foreach (var item in source)
                foreach (var collection in collectionSelector(item, count++))
                    yield return resultSelector(item, collection);
        }

        #endregion

        #region Done - SequenceEqual

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
        )
        {
            return SequenceEqual(first, second, null);
        }

        public static bool SequenceEqual<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            Check.FirstAndSecond(first, second);

            comparer = comparer ?? EqualityComparer<TSource>.Default;

            using (var firstEnumerator = first.GetEnumerator())
            using (var secondEnumerator = second.GetEnumerator())
            {
                while (firstEnumerator.MoveNext())
                {
                    if (!secondEnumerator.MoveNext())
                        return false;

                    if (!comparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                        return false;
                }

                // Check if there are more items in the second.
                return !secondEnumerator.MoveNext();
            }
        }

        #endregion

        #region Done - Single(OrDefault)

        public static TSource Single<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            return Single(source, x => true, Fallback.Throw);
        }

        public static TSource Single<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return Single(source, predicate, Fallback.Throw);
        }

        public static TSource SingleOrDefault<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            return Single(source, x => true, Fallback.Default);
        }

        public static TSource SingleOrDefault<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return Single(source, predicate, Fallback.Default);
        }

        static TSource Single<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            Fallback fallback
        )
        {
            var found = false;
            var value = default(TSource);

            foreach (var item in source)
            {
                if (!predicate(item))
                    continue;

                if (found)
                    throw new InvalidOperationException();

                found = true;
                value = item;
            }

            if (!found && fallback == Fallback.Throw)
                throw new InvalidOperationException();

            return value;
        }

        #endregion

        #region Done - Skip

        public static IEnumerable<TSource> Skip<TSource>(
            this IEnumerable<TSource> source,
            int count
        )
        {
            Check.Source(source);

            return YieldSkip(source, count);
        }

        static IEnumerable<TSource> YieldSkip<TSource>(
            IEnumerable<TSource> source,
            int count
        )
        {
            var enumerator = source.GetEnumerator();

            try
            {
                while (count-- > 0)
                    if (!enumerator.MoveNext())
                        yield break;

                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            finally { enumerator.Dispose(); }
        }

        #endregion

        #region Done - SkipWhile

        public static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return YieldSkipWhile(source, predicate);
        }

        static IEnumerable<TSource> YieldSkipWhile<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            bool yield = false;

            foreach (var item in source)
            {
                if (yield)
                    yield return item;

                else
                {
                    if (predicate(item))
                        continue;

                    yield return item;
                    yield = true;
                }
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return YieldSkipWhile(source, predicate);
        }

        static IEnumerable<TSource> YieldSkipWhile<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate
        )
        {
            int count = 0;
            bool yield = false;

            foreach (var item in source)
            {
                if (yield)
                    yield return item;

                else
                {
                    if (predicate(item, count++))
                        continue;

                    yield return item;
                    yield = true;
                }
            }
        }

        #endregion

        #region Done - Sum

        #region Normal

        public static int Sum(
            this IEnumerable<int> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0, (x, y) => checked(x + y));
        }

        public static long Sum(
            this IEnumerable<long> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0L, (x, y) => checked(x + y));
        }

        public static double Sum(
            this IEnumerable<double> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0d, (x, y) => x + y);
        }

        public static float Sum(
            this IEnumerable<float> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0f, (x, y) => x + y);
        }

        public static decimal Sum(
            this IEnumerable<decimal> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0m, (x, y) => checked(x + y));
        }

        #endregion

        #region Nullable

        public static int? Sum(
            this IEnumerable<int?> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0, (x, y) => checked(x + y.GetValueOrDefault()));
        }

        public static long? Sum(
            this IEnumerable<long?> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0L, (x, y) => checked(x + y.GetValueOrDefault()));
        }

        public static double? Sum(
            this IEnumerable<double?> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0d, (x, y) => x + y.GetValueOrDefault());
        }

        public static float? Sum(
            this IEnumerable<float?> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0f, (x, y) => x + y.GetValueOrDefault());
        }

        public static decimal? Sum(
            this IEnumerable<decimal?> source
        )
        {
            Check.Source(source);

            return Aggregate(source, 0m, (x, y) => checked(x + y.GetValueOrDefault()));
        }

        #endregion

        #region Normal - Selector

        public static int Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static long Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static double Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static float Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static decimal Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal> selector
        )
        {
            return source.Select(selector).Sum();
        }

        #endregion

        #region Nullable - Selector

        public static int? Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int?> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static long? Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, long?> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static double? Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double?> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static float? Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, float?> selector
        )
        {
            return source.Select(selector).Sum();
        }

        public static decimal? Sum<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, decimal?> selector
        )
        {
            return source.Select(selector).Sum();
        }

        #endregion

        #region Nullable



        #endregion

        #endregion

        #region Done - Take

        public static IEnumerable<TSource> Take<TSource>(
            this IEnumerable<TSource> source,
            int count
        )
        {
            Check.Source(source);

            return YieldTake(source, count);
        }

        static IEnumerable<TSource> YieldTake<TSource>(
            IEnumerable<TSource> source,
            int count
        )
        {
            if (count <= 0)
                yield break;

            int counter = 0;
            foreach (var item in source)
            {
                yield return item;

                if (++counter == count)
                    yield break;
            }
        }

        #endregion

        #region Done - TakeWhile

        public static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return YieldTakeWhile(source, predicate);
        }

        static IEnumerable<TSource> YieldTakeWhile<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            foreach (var item in source)
            {
                if (!predicate(item))
                    yield break;

                yield return item;
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return YieldTakeWhile(source, predicate);
        }

        static IEnumerable<TSource> YieldTakeWhile<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate
        )
        {
            int count = 0;
            foreach (var item in source)
            {
                if (!predicate(item, count++))
                    yield break;

                yield return item;
            }
        }

        #endregion

        #region ThenBy(Descending)

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return ThenBy(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer
        )
        {
            throw new NotImplementedException();
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return ThenByDescending(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer
        )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Done - ToArray

        public static TSource[] ToArray<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
            {
                var array = new TSource[collection.Count];
                array.CopyTo(array, 0);
                return array;
            }

            return new List<TSource>(source).ToArray();
        }

        #endregion

        #region Done - ToDictionary

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return ToDictionary(source, keySelector, null);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return ToDictionary(source, keySelector, x => x, comparer);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector
        )
        {
            return ToDictionary(source, keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);

            if (comparer == null)
                comparer = EqualityComparer<TKey>.Default;

            var dictionary = new Dictionary<TKey, TElement>(comparer);
            foreach (var item in source)
                dictionary.Add(keySelector(item), elementSelector(item));

            return dictionary;
        }

        #endregion

        #region Done - ToList

        public static List<TSource> ToList<TSource>(
            this IEnumerable<TSource> source
        )
        {
            Check.Source(source);

            var list = source as List<TSource>;
            if (list != null)
                return list;

            return new List<TSource>(source);
        }

        #endregion

        #region Done - ToLookup

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            return ToLookup(source, keySelector, x => x, null);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return ToLookup(source, keySelector, x => x, comparer);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector
        )
        {
            return ToLookup(source, keySelector, elementSelector, null);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);

            comparer = comparer ?? EqualityComparer<TKey>.Default;

            var lookup = new Lookup<TKey, TElement>(comparer);

            foreach (var element in source)
                lookup.Add(
                    keySelector(element),
                    elementSelector(element)
                );

            return lookup;
        }

        #endregion

        #region Done - Where

        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return YieldWhere(source, predicate);
        }

        static IEnumerable<TSource> YieldWhere<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            foreach (var item in source)
                if (predicate(item))
                    yield return item;
        }

        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate
        )
        {
            Check.SourceAndPredicate(source, predicate);

            return YieldWhere(source, predicate);
        }

        static IEnumerable<TSource> YieldWhere<TSource>(
            IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate
        )
        {
            int counter = 0;

            foreach (var item in source)
                if (predicate(item, counter++))
                    yield return item;
        }

        #endregion

        #region Done - Union

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
        )
        {
            return Union(first, second, null);
        }

        public static IEnumerable<TSource> Union<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            Check.FirstAndSecond(first, second);

            comparer = comparer ?? EqualityComparer<TSource>.Default;

            return YieldUnion(first, second, comparer);
        }

        static IEnumerable<TSource> YieldUnion<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
        )
        {
            // var items = new HashSet<TSource>(comparer);
            var items = new Dictionary<TSource, Object>(comparer);

            foreach (var item in first)
                if (!items.ContainsKey(item))
                {
                    items.Add(item, null);
                    yield return item;
                }

            foreach (var item in second)
                if (!items.ContainsKey(item))
                {
                    items.Add(item, null);
                    yield return item;
                }
        }

        #endregion

        #region Done - Zip (4.0)

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector
        )
        {
            Check.FirstAndSecond(first, second);
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");

            return YieldZip(first, second, resultSelector);
        }

        static IEnumerable<TResult> YieldZip<TFirst, TSecond, TResult>(
            IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector
        )
        {
            using (var enumeratorFirst = first.GetEnumerator())
            using (var enumeratorSecond = second.GetEnumerator())
                while (enumeratorFirst.MoveNext() && enumeratorSecond.MoveNext())
                    yield return resultSelector(enumeratorFirst.Current, enumeratorSecond.Current);
        }

        #endregion

    }
}
