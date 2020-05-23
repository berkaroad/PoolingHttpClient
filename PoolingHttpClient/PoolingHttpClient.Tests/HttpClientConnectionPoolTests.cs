using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PoolingHttpClient.Tests
{
    public class HttpClientConnectionPoolTests
    {
        [Fact]
        public void ProcessRequest()
        {
            var pool = new HttpClientConnectionPool("ProcessRequest", 4, 2);
            pool.DebugEnabled = true;
            var taskList = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                taskList.Add(pool.ProcessRequestAsync<string>(async (httpclient, state) =>
                {
                    await Task.Delay(2000);
                    return string.Empty;
                }, null));
            }
            Task.WaitAll(taskList.ToArray());
        }

        [Fact]
        public void CleanIdleHttpClient()
        {
            var pool = new HttpClientConnectionPool("CleanIdleHttpClient", 4, 1);
            pool.DebugEnabled = true;
            var random = new Random();
            var sleepTimeArray = new int[] { 10, 10, 10, 3000 };
            var timer = new Timer((state) =>
            {
                pool.CleanIdleHttpClient();
            });
            timer.Change(1000, 1000);
            var taskList = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                taskList.Add(pool.ProcessRequestAsync<string>(async (httpclient, state) =>
                {
                    await Task.Delay(1000);
                    return string.Empty;
                }, null));
                Thread.Sleep(sleepTimeArray[random.Next(0, sleepTimeArray.Length)]);
            }
            Task.WaitAll(taskList.ToArray());
            Thread.Sleep(5000);
        }
    }
}
