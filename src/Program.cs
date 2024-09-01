using Microsoft.Extensions.Options;
using MongoDB.Driver;
using KnowledgeExtractionTool.Infra;
using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Extensions;
using KnowledgeExtractionTool.Data;
using KnowledgeExtractionTool.Data.Types;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

# region DIs
builder.Services.AddSingleton<KnowledgeExtractorService>();
builder.Services.AddHttpClient<LanguageModelQueryService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddApiAutentication(builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>());

builder.Services.AddSingleton<IMongoClient>(sp => {
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    return new MongoClient(appSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp => {
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(appSettings.DatabaseName);
});

builder.Services.AddSingleton<UsersRepository>(sp => {
    var mongoDb = sp.GetRequiredService<IMongoDatabase>();
    var settings = new DatabaseSettings { useInMemory = true, inMemoryOnly = false }; 
    return new UsersRepository(mongoDb, "Users", settings);
});

builder.Services.AddSingleton<GraphRepository>(sp => {
    var mongoDb = sp.GetRequiredService<IMongoDatabase>();
    var settings = new DatabaseSettings { useInMemory = true, inMemoryOnly = false }; 
    return new GraphRepository(mongoDb, "Graphs", settings);
});

builder.Services.AddSingleton<JwtProvider>(sp => {
    var options = sp.GetRequiredService<IOptions<JwtOptions>>();
    return new JwtProvider(options);
});

builder.Services.AddSingleton<UserService>(sp => {
    var mongoDb = sp.GetRequiredService<IMongoDatabase>();
    var provider = sp.GetRequiredService<JwtProvider>();
    var usersRepository = sp.GetRequiredService<UsersRepository>();
    var logger = sp.GetRequiredService<ILogger<KnowledgeExtractorService>>();
    return new UserService(mongoDb, provider, usersRepository, logger);
});
# endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost") // Replace with your client URL
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KnowledgeExtractorTool API");
        c.RoutePrefix = string.Empty;
    });
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Maps controllers to routes

app.Run();