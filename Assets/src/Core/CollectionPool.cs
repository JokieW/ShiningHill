using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SH.Core
{
    public static class CollectionPool
    {
        private static class Pool<C, T> where C : ICollection<T>, new()
        {
            private static readonly int _DRY_REPLENISH_COUNT = 2;
            private static readonly Stack<C> _collections = new Stack<C>(_DRY_REPLENISH_COUNT);
            private static Stats _currentStats = new Stats();

            static Pool()
            {
                lock (_collections)
                {
                    CheckForWetPool();
                }
            }

            public static Stats GetStats()
                => _currentStats;

            public static C Request()
            {
                lock (_collections)
                {
                    CheckForWetPool();
                    _currentStats.TimesRequested++;
                    return _collections.Pop();
                }
            }

            public static void Return(C collection)
            {
                lock (_collections)
                {
                    _currentStats.TimesReturned++;
                    _collections.Push(collection);
                }
            }

            private static void CheckForWetPool()
            {
                if (_collections.Count == 0)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("CollectionPool Dry Replenish");
                    for (int i = 0; i < _DRY_REPLENISH_COUNT; i++)
                    {
                        _collections.Push(new C());
                        _currentStats.TimesAllocated++;
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        private static class Pool<C> where C : ICollection, new()
        {
            private static readonly int _DRY_REPLENISH_COUNT = 2;
            private static readonly Stack<C> _collections = new Stack<C>(_DRY_REPLENISH_COUNT);
            private static Stats _currentStats = new Stats();

            static Pool()
            {
                lock (_collections)
                {
                    CheckForWetPool();
                }
            }

            public static Stats GetStats()
                => _currentStats;

            public static C Request()
            {
                lock (_collections)
                {
                    CheckForWetPool();
                    _currentStats.TimesRequested++;
                    return _collections.Pop();
                }
            }

            public static void Return(C collection)
            {
                lock (_collections)
                {
                    _currentStats.TimesReturned++;
                    _collections.Push(collection);
                }
            }

            private static void CheckForWetPool()
            {
                if (_collections.Count == 0)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("CollectionPool Dry Replenish");
                    for (int i = 0; i < _DRY_REPLENISH_COUNT; i++)
                    {
                        _collections.Push(new C());
                        _currentStats.TimesAllocated++;
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        public struct Stats
        {
            public int TimesRequested;
            public int TimesReturned;
            public int TimesAllocated;
        }

        public static Stats GetStats<C, T>() where C : ICollection<T>, new()
            => Pool<C, T>.GetStats();

        public static Stats GetStats<C>() where C : ICollection, new()
            => Pool<C>.GetStats();

        private static class EmptyArrayPool<T>
        {
            public static readonly T[] Empty = new T[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<C, T>(out C collection) where C : ICollection<T>, new()
        {
            collection = Pool<C, T>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<T>(out List<T> collection)
        {
            collection = Pool<List<T>, T>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<T>(out HashSet<T> collection)
        {
            collection = Pool<HashSet<T>, T>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<T>(out LinkedList<T> collection)
        {
            collection = Pool<LinkedList<T>, T>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<T>(out Stack<T> collection)
        {
            collection = Pool<Stack<T>>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<T>(out Queue<T> collection)
        {
            collection = Pool<Queue<T>>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Request<TKey, TValue>(out Dictionary<TKey, TValue> collection)
        {
            collection = Pool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.Request();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetEmptyArray<T>()
        {
            return EmptyArrayPool<T>.Empty;
        }

        public static void Return<C, T>(ref C collection) where C : ICollection<T>, new()
        {
            collection.Clear();
            Pool<C, T>.Return(collection);
            collection = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(ref List<T> collection)
        {
            Return<List<T>, T>(ref collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(ref HashSet<T> collection)
        {
            Return<HashSet<T>, T>(ref collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(ref LinkedList<T> collection)
        {
            Return<LinkedList<T>, T>(ref collection);
        }

        public static void Return<T>(ref Stack<T> collection)
        {
            collection.Clear();
            Pool<Stack<T>>.Return(collection);
            collection = default;
        }

        public static void Return<T>(ref Queue<T> collection)
        {
            collection.Clear();
            Pool<Queue<T>>.Return(collection);
            collection = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<TKey, TValue>(ref Dictionary<TKey, TValue> collection)
        {
            Return<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(ref collection);
        }
    }
}
