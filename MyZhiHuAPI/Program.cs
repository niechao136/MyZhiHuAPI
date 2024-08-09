using System.Text.Json.Serialization;
using CSRedis;
using Microsoft.OpenApi.Models;
using MyZhiHuAPI.Helpers;
using MyZhiHuAPI.Middleware;
using MyZhiHuAPI.Models;
using MyZhiHuAPI.Service;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

const string myPolicy = "myPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(myPolicy, policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers(opt =>
{
    opt.RespectBrowserAcceptHeader = true;
}).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).AddNewtonsoftJson();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyZhiHuAPI", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var dbHelper = new DbHelper(configuration);
builder.Services.AddSingleton(dbHelper);
builder.Services.AddSingleton(new JwtHelper(configuration));
var rabbitMqHelper = new RabbitMqHelper(configuration);
builder.Services.AddSingleton(rabbitMqHelper);
var redisClient = new CSRedisClient(configuration["Redis:ConnectionString"]);
builder.Services.AddSingleton(redisClient);
var websocketHelp = new WebsocketHelp();
builder.Services.AddSingleton(websocketHelp);
var notifyService = new NotifyService(dbHelper, websocketHelp);
builder.Services.AddSingleton(notifyService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(myPolicy);

// app.UseRequestDecompression();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(5)
});

app.UseMyAuthorize(option =>
{
    option.CsRedisClient = redisClient;
});
app.UseRabbitMq(option =>
{
    option.Helper = rabbitMqHelper;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
).WithOpenApi().RequireCors(myPolicy);

rabbitMqHelper.Subscribe("NOTIFY", HandleNotify);

app.Run();
return;

async void HandleNotify(string msg)
{
    var notify = JsonConvert.DeserializeObject<Notify<object>>(msg);
    await notifyService.HandleNotify(notify!.Type, notify);
}
