using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
/// Authentication done
/// 
//
builder.Services.AddRateLimiter(options =>
{
  options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
  RateLimitPartition.GetSlidingWindowLimiter(
    partitionKey: context.Request.Headers.Host.ToString(),
    factory: partition => new SlidingWindowRateLimiterOptions
    {
      PermitLimit = 2,
      SegmentsPerWindow = 2,
      Window = TimeSpan.FromSeconds(5)
    })
  );
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
  options.OnRejected = (c, t) =>
  {
    Console.WriteLine(c.HttpContext.Request.Path);
    return ValueTask.CompletedTask;
  };

});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseRateLimiter();

app.Run();
