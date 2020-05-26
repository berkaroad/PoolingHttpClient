using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PoolingHttpClient
{
    /// <summary>
    /// HttpClient connection pool
    /// </summary>
    public class HttpClientConnectionPool : IDisposable
    {
        private ConcurrentQueue<Tuple<long, HttpClient>> _pool = new ConcurrentQueue<Tuple<long, HttpClient>>();
        private string _poolName;
        private int _maxPoolSize;
        private int _maxIdleTicks;
        private volatile int _poolSize;
        private volatile int _isDisposed = 0;

        /// <summary>
        /// Default max idle seconds (default: 10s)
        /// </summary>
        public const int DEFAULT_MAX_IDLE_SECONDS = 10;

        /// <summary>
        /// HttpClient connection pool
        /// </summary>
        /// <param name="poolName">Pool name</param>
        /// <param name="maxPoolSize">Max pool size</param>
        /// <param name="maxIdleSeconds">Max idle seconds</param>
        public HttpClientConnectionPool(string poolName, int maxPoolSize, int maxIdleSeconds)
        {
            _poolName = poolName;
            if (maxPoolSize > 0)
            {
                _maxPoolSize = maxPoolSize;
            }
            else
            {
                _maxPoolSize = 2;
            }
            if (maxIdleSeconds > 0)
            {
                _maxIdleTicks = 10000000 * maxIdleSeconds;
            }
            else
            {
                _maxIdleTicks = 10000000 * DEFAULT_MAX_IDLE_SECONDS;
            }
        }

        /// <summary>
        /// Finalize HttpClientConnectionPool
        /// </summary>
        ~HttpClientConnectionPool()
        {
            Dispose(false);
        }

        /// <summary>
        /// Debug enabled or not
        /// </summary>
        public bool DebugEnabled { get; set; }

        /// <summary>
        /// Process request
        /// </summary>
        /// <typeparam name="TResult">Request result</typeparam>
        /// <param name="requestHandler">Request handler</param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<TResult> ProcessRequestAsync<TResult>(Func<HttpClient, object, Task<TResult>> requestHandler, object state)
        {
            if (_isDisposed == 1)
            {
                throw new ObjectDisposedException(nameof(HttpClientConnectionPool));
            }
            var client = GetObj();
            try
            {
                var result = await requestHandler(client, state).ConfigureAwait(false);
                return result;
            }
            finally
            {
                ReturnObj(client);
            }
        }

        /// <summary>
        /// Clean idle http client
        /// </summary>
        public void CleanIdleHttpClient()
        {
            if (_pool.TryPeek(out Tuple<long, HttpClient> item))
            {
                if (item.Item1 + _maxIdleTicks < DateTime.Now.Ticks)
                {
                    if (_pool.TryDequeue(out Tuple<long, HttpClient> removedItem))
                    {
                        removedItem.Item2.Dispose();
                        Interlocked.Decrement(ref _poolSize);
                        if (DebugEnabled)
                        {
                            Console.WriteLine($"[{_poolName}]:Clean idle, pool size={_poolSize}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private HttpClient GetObj()
        {
            Tuple<long, HttpClient> clientTuple;
            while (!_pool.TryDequeue(out clientTuple))
            {
                if (Interlocked.Increment(ref _poolSize) <= _maxPoolSize)
                {
                    if (DebugEnabled)
                    {
                        Console.WriteLine($"[{_poolName}]:Get new, pool size={_poolSize}");
                    }
                    return new HttpClient();
                }
                else
                {
                    Interlocked.Decrement(ref _poolSize);
                    Thread.Sleep(1);
                }
            }
            if (DebugEnabled)
            {
                Console.WriteLine($"[{_poolName}]:Get from pool, pool size={_poolSize}");
            }
            return clientTuple.Item2;
        }

        private void ReturnObj(HttpClient client)
        {
            client.DefaultRequestHeaders.Clear();
            try
            {
                client.CancelPendingRequests();
            }
            catch { }
            finally
            {
                _pool.Enqueue(new Tuple<long, HttpClient>(DateTime.Now.Ticks, client));
                if (DebugEnabled)
                {
                    Console.WriteLine($"[{_poolName}]:Return to pool, pool size={_poolSize}");
                }
            }
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                Tuple<long, HttpClient> clientTuple;
                while (_pool.TryDequeue(out clientTuple))
                {
                    try
                    {
                        clientTuple.Item2.CancelPendingRequests();
                    }
                    catch { }
                    try
                    {
                        clientTuple.Item2.Dispose();
                    }
                    catch { }
                    Interlocked.Decrement(ref _poolSize);
                }
            }
        }
    }
}
