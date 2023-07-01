namespace Utilities_aspnet.Utilities;

public static class StartupExtension {
	public static void SetupUtilities<T>(
		this WebApplicationBuilder builder,
		string connectionStrings,
		DatabaseType databaseType = DatabaseType.SqlServer,
		string? redisConnectionString = null) where T : DbContext {
		builder.AddUtilitiesServices<T>(connectionStrings, databaseType);

		if (redisConnectionString != null) builder.AddRedis(redisConnectionString);

		IServiceProvider? serviceProvider = builder.Services.BuildServiceProvider().GetService<IServiceProvider>();

		builder.AddUtilitiesSwagger(serviceProvider);
		builder.AddUtilitiesIdentity();

		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});
		builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = int.MaxValue);
	}

	private static void AddUtilitiesServices<T>(this WebApplicationBuilder builder, string connectionStrings, DatabaseType databaseType) where T : DbContext {
		builder.Services.AddOptions();
		builder.Services.AddOutputCache(x => {
			x.AddPolicy("content", new OutputCachePolicy(TimeSpan.FromHours(24), "content"));
			x.AddPolicy("category", new OutputCachePolicy(TimeSpan.FromHours(24), "category"));
		});
		
		builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

		builder.Services.AddRateLimiter(x => {
			x.AddFixedWindowLimiter("fixed", y => {
				y.PermitLimit = 10;
				y.Window = TimeSpan.FromSeconds(1);
				y.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				y.QueueLimit = 2;
			});
		});

		builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
		builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
		builder.Services.AddResponseCompression(options => {
			options.EnableForHttps = true;
			options.Providers.Add<BrotliCompressionProvider>();
			options.Providers.Add<GzipCompressionProvider>();
		});
		builder.Services.AddScoped<DbContext, T>();

		builder.Services.AddDbContextPool<T>(options => {
			switch (databaseType) {
				case DatabaseType.SqlServer:
					options.UseSqlServer(connectionStrings, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
					break;
				case DatabaseType.MySql:
					options.UseMySql(connectionStrings, new MySqlServerVersion(new Version(8, 0, 28)),
					                 o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
			}
		});

		builder.Services.AddMemoryCache();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddSignalR();
		builder.Services.AddScoped<CustomInterceptor>();
		builder.Services.AddHttpClient("my-client").AddHttpMessageHandler<CustomInterceptor>();
		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
		builder.Services.AddControllersWithViews(option => option.EnableEndpointRouting = false).AddNewtonsoftJson(options => {
			options.SerializerSettings.ContractResolver = new DefaultContractResolver();
			options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
			options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			options.UseCamelCasing(true);
		});

		builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		builder.Services.AddScoped<AppSettings>();
		builder.Services.AddScoped<IReportRepository, ReportRepository>();
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IMediaRepository, MediaRepository>();
		builder.Services.AddScoped<IFollowBookmarkRepository, FollowBookmarkRepository>();
		builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
		builder.Services.AddScoped<IProductRepository, ProductRepository>();
		builder.Services.AddScoped<IChatRepository, ChatRepository>();
		builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
		builder.Services.AddScoped<IFormRepository, FormRepository>();
		builder.Services.AddScoped<ICommentRepository, CommentRepository>();
		builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
		builder.Services.AddScoped<IContentRepository, ContentRepository>();
		builder.Services.AddScoped<IOrderRepository, OrderRepository>();
		builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
		builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
		builder.Services.AddScoped<ISmsNotificationRepository, SmsNotificationRepository>();
		builder.Services.AddScoped<IAddressRepository, AddressRepository>();
		builder.Services.AddScoped<IWithdrawRepository, WithdrawRepository>();
		builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
	}

	private static void AddUtilitiesSwagger(this WebApplicationBuilder builder, IServiceProvider? serviceProvider) {
		Server.Configure(serviceProvider?.GetService<IHttpContextAccessor>());
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c => {
			c.UseInlineDefinitionsForEnums();
			c.OrderActionsBy(s => s.RelativePath);
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
				Description = "JWT Authorization header.\r\n\r\nExample: \"Bearer 12345abcdef\"",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});

			c.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{
					new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
					Array.Empty<string>()
				}
			});
		});
	}

	private static void AddRedis(this WebApplicationBuilder builder, string connectionString) {
		builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
	}

	public static void AddUtilitiesIdentity(this WebApplicationBuilder builder) {
		builder.Services.AddIdentity<UserEntity, IdentityRole>(options => { options.SignIn.RequireConfirmedAccount = false; }).AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<DbContext>().AddDefaultTokenProviders();
		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters {
				RequireSignedTokens = true,
				ValidateIssuerSigningKey = true,
				ValidateIssuer = true,
				ValidateAudience = true,
				RequireExpirationTime = true,
				ClockSkew = TimeSpan.Zero,
				ValidAudience = "https://SinaMN75.com",
				ValidIssuer = "https://SinaMN75.com",
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("https://SinaMN75.com"))
			};
		});

		builder.Services.Configure<IdentityOptions>(options => {
			options.Password.RequireDigit = false;
			options.Password.RequiredLength = 4;
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequireUppercase = false;
			options.Password.RequireLowercase = false;
			options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
		});
	}

	public static void UseUtilitiesServices(this WebApplication app) {
		app.UseCors(option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseRateLimiter();
		app.UseOutputCache();
		app.UseDeveloperExceptionPage();
		app.UseUtilitiesSwagger();
		app.UseStaticFiles();
		app.UseAuthentication();
		app.UseAuthorization();

		app.MapHub<ChatHub>("/hubs/ChatHub");
	}

	private static void UseUtilitiesSwagger(this IApplicationBuilder app) {
		app.UseSwagger();
		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DefaultModelsExpandDepth(2);
		});
	}
}

internal class OutputCachePolicy : IOutputCachePolicy {
	private readonly TimeSpan _timeSpan;
	private readonly string _tag;

	public OutputCachePolicy(TimeSpan seconds, string tag) {
		_timeSpan = seconds;
		_tag = tag;
	}

	public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

	public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

	public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation) {
		context.Tags.Add(_tag);
		context.AllowCacheLookup = true;
		context.AllowCacheStorage = true;
		context.AllowLocking = true;
		context.EnableOutputCaching = true;
		context.ResponseExpirationTimeSpan = _timeSpan;
		context.CacheVaryByRules.QueryKeys = "*";
		context.CacheVaryByRules.VaryByHost = true;
		context.CacheVaryByRules.HeaderNames = "*";
		return ValueTask.CompletedTask;
	}
}