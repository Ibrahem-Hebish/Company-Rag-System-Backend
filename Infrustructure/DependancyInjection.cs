namespace Infrustructure;

public static class DependancyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if(string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddPostgresVectorStore(connectionString);

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICompanyService, CompanyService>();

        services.AddScoped<IDepartmentService, DepartmentService>();

        services.AddScoped<IEmployeeService, EmployeeService>();

        services.AddScoped<IDocumentService, DocumentService>();

        services.AddScoped<IDocumentChunkRepository, PostgresDocumentChunkRepository>();

        services.AddScoped<ITenantResolver, TenantResolver>();

        services.AddTransient<IPasswordHashingHandler, PasswordHashingHandler>();

        services.AddTransient<ITokenHandler, Abstractions.TokenHandler>();

        services.AddTransient<IFileHandler, FileHandler>();

        var newDocumentIsSavedChannel = Channel.CreateUnbounded<NewDocumentIsSavedEvent>();
        var documentIsDeletedChannel = Channel.CreateUnbounded<DocumentIsDeletedEvent>();
        var documentsDepartmentDeletedChannel = Channel.CreateUnbounded<DocumentsDepartmentIsDeletedEvent>();
        var companyIsDeletedEvent = Channel.CreateUnbounded<CompanyIsDeletedEvent>();

        services.AddSingleton(newDocumentIsSavedChannel);
        services.AddSingleton(documentIsDeletedChannel);
        services.AddSingleton(documentsDepartmentDeletedChannel);
        services.AddSingleton(companyIsDeletedEvent);


        services.AddHostedService<DocumentIsSavedEventConsumer>();
        services.AddHostedService<DocumentIsDeletedEventConsumer>();
        services.AddHostedService<DepartmentIsDeletedEventConsumer>();
        services.AddHostedService<CompanyIsDeletedEventConsumer>();

        services.AddSingleton<IDocumentChannel, DocumentChannel>();

        services.Configure<JwtOptions>(configuration.GetSection("jwt"));

        var jwt = configuration.GetSection("jwt").Get<JwtOptions>();

        var signingKey = configuration["jwtsigningkey"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.SaveToken = true;

                    opt.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt!.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt!.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                               Encoding.UTF8.GetBytes(signingKey!)),
                    };

                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Cookies["access-token"];

                            context.Token = accessToken;

                            return Task.CompletedTask;
                        },
                    };
                });

        services.Configure<AIOptions>(configuration.GetSection("AI"));

        var aiOptions = configuration.GetSection("AI").Get<AIOptions>()!;

        var ollamaEndpoint = new Uri("http://localhost:11434");

        var chatClient = new OllamaApiClient(
                ollamaEndpoint,
                aiOptions.ChatModel);

        services.AddChatClient(chatClient);

        var embeddingModel = new OllamaApiClient(
                ollamaEndpoint,
                aiOptions.EmbeddingModel);

        services.AddEmbeddingGenerator(embeddingModel);

        services.AddSingleton<IngestionDocumentReader>(_ => new MarkItDownReader(
            new FileInfo(aiOptions.ParserFilePath)));

        // Chunker

        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-5");

        var ingestionChunckerOptions = new IngestionChunkerOptions(tokenizer) 
        {
            MaxTokensPerChunk = aiOptions.MaxTokens,
            OverlapTokens = aiOptions.OverlapTokens
        
        };

        //var documentTokenChuncker = new DocumentTokenChunker(ingestionChunckerOptions);

        var documentSectionChuncker = new SectionChunker(ingestionChunckerOptions);

        services.AddSingleton<IngestionChunker<string>>(_ => documentSectionChuncker);

        // Enricher

        var classificationEnricherOptions = new ClassificationEnricher(
            new EnricherOptions(chatClient),
                ["HR", "Finance", "IT", "Management", "Marketing"],
                fallbackClass: "Unknown");

        services.AddSingleton(classificationEnricherOptions);

        return services;
    }
}
