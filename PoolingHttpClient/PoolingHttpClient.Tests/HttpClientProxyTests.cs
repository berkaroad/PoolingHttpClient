using System;
using Xunit;

namespace PoolingHttpClient.Tests
{
    public class HttpClientProxyTests
    {
        [Fact]
        public void SetConnectionLimit()
        {
            var client = new DefaultPoolingHttpClient();
            client.SetConnectionLimit(new Uri("http://www.baidu.com"), 123);
            client.DebugEnabled = true;
            Assert.Equal(123, client.GetConnectionLimit(new Uri("http://www.baidu.com")));
            Assert.NotEqual(123, client.GetConnectionLimit(new Uri("http://www.sina.com")));
        }

        [Fact]
        public void ExecuteHttpGet()
        {
            var client = new DefaultPoolingHttpClient();
            client.SetConnectionLimit(new Uri("http://www.baidu.com"), 2);
            client.MaxConnectionIdleSeconds = 2;
            client.DebugEnabled = true;
            var result = client.ExecuteHttpGetAsync("http://www.baidu.com").Result;
            Assert.True(!string.IsNullOrEmpty(result) && result.Contains("<title>百度一下，你就知道</title>"));
        }

        [Fact]
        public void ExecuteHttpPost()
        {
            var client = new DefaultPoolingHttpClient();
            client.SetConnectionLimit(new Uri("http://www.baidu.com"), 2);
            client.MaxConnectionIdleSeconds = 2;
            client.DebugEnabled = true;
            var result = client.ExecuteHttpPostAsync("http://www.baidu.com", "").Result;
            Assert.True(!string.IsNullOrEmpty(result) && result.Contains("<title>页面不存在_百度搜索</title>"));
        }

        [Fact]
        public void ExecuteHttpPut()
        {
            var client = new DefaultPoolingHttpClient();
            client.SetConnectionLimit(new Uri("http://www.baidu.com"), 2);
            client.MaxConnectionIdleSeconds = 2;
            client.DebugEnabled = true;
            var result = client.ExecuteHttpPutAsync("http://www.baidu.com", "").Result;
            Assert.True(!string.IsNullOrEmpty(result) && result.Contains("<p>The requested method PUT is not allowed"));
        }

        [Fact]
        public void ExecuteHttpDelete()
        {
            var client = new DefaultPoolingHttpClient();
            client.SetConnectionLimit(new Uri("http://www.baidu.com"), 2);
            client.MaxConnectionIdleSeconds = 2;
            client.DebugEnabled = true;
            var result = client.ExecuteHttpDeleteAsync("http://www.baidu.com").Result;
            Assert.True(!string.IsNullOrEmpty(result) && result.Contains("<p>The requested method DELETE is not allowed"));
        }
    }
}
