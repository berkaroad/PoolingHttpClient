# PoolingHttpClient
Pooling HttpClient, for limit connection per host.

- Limit connection per host.

- Auto clean idle connection.

## Install

```
dotnet add package PoolingHttpClient
```

## Usage

```csharp
using PoolingHttpClient;
// create client, you can also use ioc to inject with IPoolingHttpClient.
var client = new DefaultPoolingHttpClient();
// set connection limit 2 for http://www.baidu.com
client.SetConnectionLimit(new Uri("http://www.baidu.com"), 2);
// Max connection idle seconds, only expiry will been clean.
client.MaxConnectionIdleSeconds = 10;
var result = await client.ExecuteHttpGetAsync("http://www.baidu.com");
// deserialize from json string
```

## Testing Pool

```csharp
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

/* result
[CleanIdleHttpClient]:Get new, pool size=1
[CleanIdleHttpClient]:Return to pool, pool size=1
[CleanIdleHttpClient]:Clean idle, pool size=0
[CleanIdleHttpClient]:Get new, pool size=1
[CleanIdleHttpClient]:Get new, pool size=2
[CleanIdleHttpClient]:Get new, pool size=3
[CleanIdleHttpClient]:Get new, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Get from pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Clean idle, pool size=3
[CleanIdleHttpClient]:Clean idle, pool size=2
[CleanIdleHttpClient]:Clean idle, pool size=1
[CleanIdleHttpClient]:Clean idle, pool size=0
[CleanIdleHttpClient]:Get new, pool size=1
[CleanIdleHttpClient]:Get new, pool size=2
[CleanIdleHttpClient]:Get new, pool size=3
[CleanIdleHttpClient]:Get new, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Return to pool, pool size=4
[CleanIdleHttpClient]:Clean idle, pool size=3
[CleanIdleHttpClient]:Clean idle, pool size=2
[CleanIdleHttpClient]:Clean idle, pool size=1
[CleanIdleHttpClient]:Clean idle, pool size=0
*/
```

## Publish History

### 1.0.1
1）Optimize cleaning idle connection.

### 1.0.0
1）Limit connection per host.

2）Support auto clean idle connection.
