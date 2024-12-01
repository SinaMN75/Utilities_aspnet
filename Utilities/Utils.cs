namespace Utilities_aspnet.Utilities;

public static class StartupExtension {
	public static void SetupUtilities<T>(
		this WebApplicationBuilder builder,
		bool addOpenTelemetry = false
	) where T : DbContext {
		builder.AddUtilitiesServices<T>();
		builder.AddUtilitiesSwagger();
		builder.AddUtilitiesIdentity();
		builder.AddUtilitiesOutputCache();
		if (addOpenTelemetry) builder.AddOpenTelemetry();
	}

	public static void UseUtilitiesServices(this WebApplication app, bool log = false) {
		if (log) app.UseMiddleware<RequestResponseLoggingMiddleware>();
		app.UseCors(option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseMiddleware<ApiKeyMiddleware>();
		app.UseRateLimiter();
		app.UseOutputCache();
		app.UseDeveloperExceptionPage();
		app.UseUtilitiesSwagger();
		app.UseStaticFiles();
		app.Use(async (context, next) => {
			await next();
			if (context.Response.StatusCode == 401)
				await context.Response.WriteAsJsonAsync(new GenericResponse(UtilitiesStatusCodes.UnAuthorized));
		});

		app.UseAuthentication();
		app.UseAuthorization();
	}

	private static void AddUtilitiesServices<T>(this WebApplicationBuilder builder) where T : DbContext {
		builder.Logging
			.AddFilter("Microsoft", LogLevel.Warning)
			.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

		builder.Services.AddOptions();
		builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
		AppSettings.Initialize(builder.Configuration);
		builder.Services.AddRateLimiter(x => {
			x.RejectionStatusCode = 429;
			x.AddFixedWindowLimiter("fixed", y => {
				y.PermitLimit = 5;
				y.Window = TimeSpan.FromSeconds(10);
				y.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				y.QueueLimit = 10;
			});
		});

		builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.AddScoped<DbContext, T>();

		builder.Services.AddDbContextPool<T>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ServerPostgres"), o => {
			AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
			o.EnableRetryOnFailure(maxRetryCount: 2);
			o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
		}));

		builder.Services.AddStackExchangeRedisCache(o => o.Configuration = builder.Configuration.GetConnectionString("Redis"));

		builder.Services.AddHttpContextAccessor();
		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
		builder.Services.AddControllersWithViews(option => option.EnableEndpointRouting = false).AddNewtonsoftJson(options => {
			options.SerializerSettings.ContractResolver = new DefaultContractResolver();
			options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
			options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			options.UseCamelCasing(true);
		});

		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});
		builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = int.MaxValue);
		Server.Configure(builder.Services.BuildServiceProvider().GetService<IServiceProvider>()?.GetService<IHttpContextAccessor>());
		builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		builder.Services.AddScoped<IReportRepository, ReportRepository>();
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IMediaRepository, MediaRepository>();
		builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
		builder.Services.AddScoped<IProductRepository, ProductRepository>();
		builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
		builder.Services.AddScoped<ICommentRepository, CommentRepository>();
		builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
		builder.Services.AddScoped<IContentRepository, ContentRepository>();
		builder.Services.AddScoped<IOrderRepository, OrderRepository>();
		builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
		builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
		builder.Services.AddScoped<ISmsNotificationRepository, SmsNotificationRepository>();
		builder.Services.AddScoped<IAddressRepository, AddressRepository>();
		builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
		builder.Services.AddScoped<IAmazonS3Repository, AmazonS3Repository>();
		builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
	}

	private static void AddUtilitiesSwagger(this WebApplicationBuilder builder) {
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
			c.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme {
				Description = "API KEY",
				Name = "X-API-KEY",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey
			});

			c.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() },
				{ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apiKey" } }, Array.Empty<string>() }
			});
		});
	}

	private static void AddUtilitiesOutputCache(this WebApplicationBuilder builder) {
		builder.Services.AddOutputCache(x => x.AddPolicy("default", y => {
			y.Cache();
			y.SetVaryByHeader("*");
			y.SetVaryByQuery("*");
			y.Expire(TimeSpan.FromHours(1));
			y.AddPolicy<CustomCachePolicy>().VaryByValue(context => {
					context.Request.EnableBuffering();
					using StreamReader reader = new(context.Request.Body, leaveOpen: true);
					Task<string> body = reader.ReadToEndAsync();
					context.Request.Body.Position = 0;
					KeyValuePair<string, string> keyVal = new("requestBody", body.Result);
					return keyVal;
				}
			);
		}));
	}

	private static void AddOpenTelemetry(this WebApplicationBuilder builder) {
		builder.Services.AddOpenTelemetry()
			.ConfigureResource(res => res.AddService("sinamn75api"))
			.WithMetrics(x => {
				x.AddAspNetCoreInstrumentation();
				x.AddOtlpExporter();
			})
			.WithTracing(x => {
				x.AddAspNetCoreInstrumentation();
				x.AddOtlpExporter();
			});

		builder.Logging.AddOpenTelemetry(o => o.AddOtlpExporter());
	}

	private static void AddUtilitiesIdentity(this WebApplicationBuilder builder) {
		builder.Services.AddAuthentication(options => {
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options => {
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters {
				RequireSignedTokens = true,
				ValidateIssuerSigningKey = true,
				ValidateIssuer = true,
				ValidateAudience = true,
				RequireExpirationTime = true,
				ClockSkew = TimeSpan.Zero,
				ValidAudience = "https://SinaMN75.com,BetterSoft1234",
				ValidIssuer = "https://SinaMN75.com,BetterSoft1234",
				IssuerSigningKey = new SymmetricSecurityKey("https://SinaMN75.com,BetterSoft1234"u8.ToArray())
			};
		});

		builder.Services.AddAuthorization();
	}

	private static void UseUtilitiesSwagger(this IApplicationBuilder app) {
		app.UseSwagger();
		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DefaultModelsExpandDepth(2);
		});
	}

	internal class CustomCachePolicy : IOutputCachePolicy {
		public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation) {
			context.AllowCacheLookup = true;
			context.AllowCacheStorage = true;
			context.AllowLocking = true;
			context.EnableOutputCaching = true;
			context.ResponseExpirationTimeSpan = TimeSpan.FromHours(6);
			return ValueTask.CompletedTask;
		}

		public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
		public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
	}
}