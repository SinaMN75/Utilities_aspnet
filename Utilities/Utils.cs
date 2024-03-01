namespace Utilities_aspnet.Utilities;

public static class StartupExtension {
	public static void SetupUtilities<T>(
		this WebApplicationBuilder builder,
		string connectionStrings,
		UtilitiesDatabaseType databaseType = UtilitiesDatabaseType.SqlServer,
		string? redisConnectionString = null
	) where T : DbContext {
		builder.AddUtilitiesServices<T>(connectionStrings, databaseType, redisConnectionString);

		builder.AddUtilitiesSwagger(builder.Services.BuildServiceProvider().GetService<IServiceProvider>());
		builder.AddUtilitiesIdentity();

		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});
		builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = int.MaxValue);
	}

	private static void AddUtilitiesServices<T>(
		this WebApplicationBuilder builder,
		string connectionStrings,
		UtilitiesDatabaseType databaseType,
		string? redisConnectionString
	) where T : DbContext {
		builder.Logging.AddRinLogger();
		builder.Services.AddRin();
		builder.Services.AddOptions();
		builder.Services.AddUtilitiesOutputCache("everything", TimeSpan.FromHours(2), false);
		builder.Services.AddUtilitiesOutputCache("appSetting", TimeSpan.FromHours(24), false);
		builder.Services.AddUtilitiesOutputCache("content", TimeSpan.FromHours(24), false);
		builder.Services.AddUtilitiesOutputCache("category", TimeSpan.FromHours(1), false);
		builder.Services.AddUtilitiesOutputCache("address", TimeSpan.FromHours(1));
		builder.Services.AddUtilitiesOutputCache("comment", TimeSpan.FromHours(1));
		builder.Services.AddUtilitiesOutputCache("transaction", TimeSpan.FromMinutes(1));

		builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

		builder.Services.AddRateLimiter(x => {
			x.RejectionStatusCode = 429;
			x.AddFixedWindowLimiter("fixed", y => {
				y.PermitLimit = 5;
				y.Window = TimeSpan.FromSeconds(2);
				y.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				y.QueueLimit = 10;
			});
			x.AddFixedWindowLimiter("follow", y => {
				y.PermitLimit = 30;
				y.Window = TimeSpan.FromHours(24);
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
				case UtilitiesDatabaseType.SqlServer:
					options.UseSqlServer(connectionStrings, o => {
						o.EnableRetryOnFailure(maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(1), errorNumbersToAdd: new int[] { });
						o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
					});
					break;
				case UtilitiesDatabaseType.Postgres:
					options.UseNpgsql(connectionStrings, o => {
						o.EnableRetryOnFailure(maxRetryCount: 2);
						o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
					});
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
			}
		});

		if (redisConnectionString is not null) {
			builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
		}

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
		//Encrypt/Decrypt
		//builder.Services.AddTransient<EncryptionMiddleware>();
		//Encrypt/Decrypt
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
		builder.Services.AddScoped<ICommentRepository, CommentRepository>();
		builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
		builder.Services.AddScoped<IContentRepository, ContentRepository>();
		builder.Services.AddScoped<IOrderRepository, OrderRepository>();
		builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
		builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
		builder.Services.AddScoped<ISmsNotificationRepository, SmsNotificationRepository>();
		builder.Services.AddScoped<IAddressRepository, AddressRepository>();
		builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
		builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
		builder.Services.AddScoped<IAmazonS3Repository, AmazonS3Repository>();
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

	public static void AddUtilitiesIdentity(this WebApplicationBuilder builder) {
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

	public static void UseUtilitiesServices(this WebApplication app) {
		app.UseCors(option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseRin();
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
		
		app.UseRinDiagnosticsHandler();

		//Encrypt/Decrypt
		//app.UseMiddleware<EncryptionMiddleware>();
		//Encrypt/Decrypt
	}

	private static void UseUtilitiesSwagger(this IApplicationBuilder app) {
		app.UseSwagger();
		app.UseSwaggerUI(c => {
			c.DocExpansion(DocExpansion.None);
			c.DefaultModelsExpandDepth(2);
		});
	}

	private static void AddUtilitiesOutputCache(this IServiceCollection service, string tag, TimeSpan expire, bool varyByHeaders = true) =>
		service.AddOutputCache(x => {
			x.AddPolicy(tag, cachePolicyBuilder => {
				cachePolicyBuilder.SetVaryByRouteValue("*");
				if (varyByHeaders)
					cachePolicyBuilder.SetVaryByHeader("*");
				cachePolicyBuilder.SetVaryByHost(true);
				cachePolicyBuilder.SetVaryByQuery("*");
				cachePolicyBuilder.Expire(expire);
				cachePolicyBuilder.AddPolicy<OutputCachePolicy>().VaryByValue(context => {
					context.Request.EnableBuffering();
					using StreamReader reader = new(context.Request.Body, leaveOpen: true);
					Task<string> body = reader.ReadToEndAsync();
					context.Request.Body.Position = 0;
					KeyValuePair<string, string> keyVal = new("requestBody", body.Result);
					return keyVal;
				});
			});
		});
}

internal class OutputCachePolicy : IOutputCachePolicy {
	public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
	public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
	public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
}