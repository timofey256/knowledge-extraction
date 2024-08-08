using KnowledgeExtractionTool.Infra;
using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Infra.Services.Interfaces;
using KnowledgeExtractionTool.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));

# region DIs
builder.Services.AddSingleton<ITextProcessorService>(provider => {
    var knowledgeExtractorService = provider.GetRequiredService<KnowledgeExtractorService>();
    return new TextProcessorService(knowledgeExtractorService);
});

builder.Services.AddSingleton<KnowledgeExtractorService>();
builder.Services.AddHttpClient<LanguageModelQueryService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddApiAutentication(builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>());

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    return new MongoClient(appSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(appSettings.DatabaseName);
});

builder.Services.AddSingleton<JwtProvider>(sp => {
    var options = sp.GetRequiredService<IOptions<JwtOptions>>();
    return new JwtProvider(options);
});

builder.Services.AddSingleton<UserService>(sp =>
{
    var mongoDb = sp.GetRequiredService<IMongoDatabase>();
    var provider = sp.GetRequiredService<JwtProvider>();
    var logger = sp.GetRequiredService<ILogger<KnowledgeExtractorService>>();
    return new UserService(mongoDb, provider, logger);
});
# endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Maps controllers to routes

app.Run();