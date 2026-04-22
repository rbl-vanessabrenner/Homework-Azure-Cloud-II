using Library.API.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.ConfigureAppServices(builder.Configuration);

var app = builder.Build();
app.ConfigureApp(app.Environment);
app.Run();
