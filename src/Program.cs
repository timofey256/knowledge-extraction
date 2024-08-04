using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Infra.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

# region DIs
builder.Services.AddSingleton<ITextProcessorService>(provider => {
    var knowledgeExtractorService = provider.GetRequiredService<KnowledgeExtractorService>();
    return new TextProcessorService(knowledgeExtractorService);
});

builder.Services.AddSingleton<KnowledgeExtractorService>();
builder.Services.AddHttpClient<LanguageModelQueryService>();
builder.Services.AddControllers(); 
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

app.UseAuthorization();

app.MapControllers(); // Maps controllers to routes

app.Run();