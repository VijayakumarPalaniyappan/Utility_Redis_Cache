## Development Setup
### High level instructions:
1. Add a reference to the Microsoft.Azure.StackExchangeRedis NuGet package in your Redis client project.
2. In your Redis connection code, first create a ConfigurationOptions instance. You can use the .Parse() method to create an instance from a Redis connection string or the cache host name alone.
   ````bash
	  var configurationOptions = ConfigurationOptions.Parse($"{cacheHostName}:6380");
   ````
4. Use one of the ConfigureForAzure* extension methods supplied by this package to configure the authentication options:
   ````C#
   // Service principal secret
    configurationOptions.ConfigureForAzureWithServicePrincipalAsync(clientId, tenantId, secret).Result;
   ````
5. Create the connection, passing in the ConfigurationOptions instance
   ````C#
	  var connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
   ````
6. Use the connectionMultiplexer to interact with Redis as you normally would.

This has been done through extension method of IServiceCollection(\Configurations\ServiceCollectionExtensions.cs)
````C#
  [ExcludeFromCodeCoverage]
  [SuppressMessage("SonarLint", "S4462", Justification = "No need of asynchronous call")]
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddRedis(this IServiceCollection service, AzureConfig azureConfig, string host, bool isDevelopment)
    {
      service.AddSingleton<IConnectionMultiplexer>(sp =>
      {
        if (isDevelopment)
        {
          var options = ConfigurationOptions.Parse(host);
          options.Ssl = true;

          return ConnectionMultiplexer.Connect(options);
        }

        var configurationOptions = ConfigurationOptions.Parse(host)
                                    .ConfigureForAzureWithServicePrincipalAsync(azureConfig.ServicePrincipalId, azureConfig.TenantId,
                                    azureConfig.ClientSecret).Result;
        configurationOptions.Ssl = true;

        return ConnectionMultiplexer.Connect(configurationOptions);
      });
      return service;
    }
  }
````
**ConnectionMultiplexer** internally holds **IDatabase**, which is used to store cache by key and value pair. Before achieving the cache, the Redis hostname and cache expiry limit should be setup in either appsettings.json, Azure keyVault or Environment variable based on application needs.
In our application, **KeyVault** provides HostName and cache expiry limit from **appsettings.json**.

````json
"RedisCacheOptions": {
  "CacheExpirationInHours": "8"
}
````
