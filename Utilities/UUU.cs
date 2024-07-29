// using System.Security.Cryptography;
// using System.Text;
// using Microsoft.AspNetCore.Mvc.Filters;
// using static System.TimeSpan;
//
// namespace Utilities_aspnet.Utilities;
//
// public static class StartupExtension {
// 	public static void SetupUtilities<T>(
// 		this WebApplicationBuilder builder,
// 		string connectionStrings,
// 		UtilitiesDatabaseType databaseType = UtilitiesDatabaseType.SqlServer,
// 		string? redisConnectionString = null
// 	) where T : DbContext {
// 		builder.AddUtilitiesServices<T>(connectionStrings, databaseType, redisConnectionString);
//
// 		builder.AddUtilitiesSwagger(builder.Services.BuildServiceProvider().GetService<IServiceProvider>());
// 		builder.AddUtilitiesIdentity();
//
// 		builder.Services.Configure<FormOptions>(x => {
// 			x.ValueLengthLimit = int.MaxValue;
// 			x.MultipartBodyLengthLimit = int.MaxValue;
// 			x.MultipartHeadersLengthLimit = int.MaxValue;
// 		});
// 		builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = int.MaxValue);
// 	}
//
// 	private static void AddUtilitiesServices<T>(
// 		this WebApplicationBuilder builder,
// 		string connectionStrings,
// 		UtilitiesDatabaseType databaseType,
// 		string? redisConnectionString
// 	) where T : DbContext {
// 		builder.Services.AddOptions();
//
// 		builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
//
// 		builder.Services.AddRateLimiter(x => {
// 			x.RejectionStatusCode = 429;
// 			x.AddFixedWindowLimiter("fixed", y => {
// 				y.PermitLimit = 5;
// 				y.Window = FromSeconds(2);
// 				y.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
// 				y.QueueLimit = 10;
// 			});
// 			x.AddFixedWindowLimiter("follow", y => {
// 				y.PermitLimit = 30;
// 				y.Window = FromHours(24);
// 			});
// 		});
//
// 		builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
// 		builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
// 		builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
// 		builder.Services.AddResponseCompression(options => {
// 			options.EnableForHttps = true;
// 			options.Providers.Add<BrotliCompressionProvider>();
// 			options.Providers.Add<GzipCompressionProvider>();
// 		});
// 		builder.Services.AddScoped<DbContext, T>();
//
// 		builder.Services.AddDbContextPool<T>(options => {
// 			switch (databaseType) {
// 				case UtilitiesDatabaseType.SqlServer:
// 					options.UseSqlServer(connectionStrings, o => {
// 						o.EnableRetryOnFailure(maxRetryCount: 2, maxRetryDelay: FromSeconds(1), errorNumbersToAdd: Array.Empty<int>());
// 						o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
// 					});
// 					break;
// 				case UtilitiesDatabaseType.Postgres:
// 					options.UseNpgsql(connectionStrings, o => {
// 						AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// 						o.EnableRetryOnFailure(maxRetryCount: 2);
// 						o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
// 					});
// 					break;
// 				default:
// 					throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
// 			}
// 		});
//
// 		if (redisConnectionString is not null) {
// 			builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
// 		}
//
// 		builder.Services.AddHttpContextAccessor();
// 		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
// 		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
// 		builder.Services.AddControllersWithViews(option => option.EnableEndpointRouting = false).AddNewtonsoftJson(options => {
// 			options.SerializerSettings.ContractResolver = new DefaultContractResolver();
// 			options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
// 			options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
// 			options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
// 			options.UseCamelCasing(true);
// 		});
//
// 		builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
// 		builder.Services.AddScoped<ApiKeyAuthFilter>();
// 		builder.Services.AddOutputCache();
// 		builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// 		builder.Services.AddScoped<AppSettings>();
// 		builder.Services.AddScoped<IReportRepository, ReportRepository>();
// 		builder.Services.AddScoped<IUserRepository, UserRepository>();
// 		builder.Services.AddScoped<IMediaRepository, MediaRepository>();
// 		builder.Services.AddScoped<IFollowBookmarkRepository, FollowBookmarkRepository>();
// 		builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
// 		builder.Services.AddScoped<IProductRepository, ProductRepository>();
// 		builder.Services.AddScoped<IChatRepository, ChatRepository>();
// 		builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
// 		builder.Services.AddScoped<ICommentRepository, CommentRepository>();
// 		builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
// 		builder.Services.AddScoped<IContentRepository, ContentRepository>();
// 		builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// 		builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
// 		builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
// 		builder.Services.AddScoped<ISmsNotificationRepository, SmsNotificationRepository>();
// 		builder.Services.AddScoped<IAddressRepository, AddressRepository>();
// 		builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
// 		builder.Services.AddScoped<IAmazonS3Repository, AmazonS3Repository>();
// 		builder.Services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
// 	}
//
// 	private static void AddUtilitiesSwagger(this IHostApplicationBuilder builder, IServiceProvider? serviceProvider) {
// 		Server.Configure(serviceProvider?.GetService<IHttpContextAccessor>());
// 		builder.Services.AddEndpointsApiExplorer();
// 		builder.Services.AddSwaggerGen(c => {
// 			c.UseInlineDefinitionsForEnums();
// 			c.OrderActionsBy(s => s.RelativePath);
// 			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
// 				Description = "JWT Authorization header.\r\n\r\nExample: \"Bearer 12345abcdef\"",
// 				Name = "Authorization",
// 				In = ParameterLocation.Header,
// 				Type = SecuritySchemeType.ApiKey,
// 				Scheme = "Bearer"
// 			});
// 			c.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme() {
// 				Description = "API KEY",
// 				Name = "X-API-KEY",
// 				In = ParameterLocation.Header,
// 				Type = SecuritySchemeType.ApiKey
// 			});
//
// 			c.AddSecurityRequirement(new OpenApiSecurityRequirement {
// 				{
// 					new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
// 					Array.Empty<string>()
// 				}, {
// 					new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apiKey" } },
// 					Array.Empty<string>()
// 				},
// 			});
// 		});
// 	}
//
// 	private static void AddUtilitiesIdentity(this IHostApplicationBuilder builder) {
// 		builder.Services.AddAuthentication(options => {
// 			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
// 			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// 			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
// 		}).AddJwtBearer(options => {
// 			options.RequireHttpsMetadata = false;
// 			options.SaveToken = true;
// 			options.TokenValidationParameters = new TokenValidationParameters {
// 				RequireSignedTokens = true,
// 				ValidateIssuerSigningKey = true,
// 				ValidateIssuer = true,
// 				ValidateAudience = true,
// 				RequireExpirationTime = true,
// 				ClockSkew = Zero,
// 				ValidAudience = "https://SinaMN75.com,BetterSoft1234",
// 				ValidIssuer = "https://SinaMN75.com,BetterSoft1234",
// 				IssuerSigningKey = new SymmetricSecurityKey("https://SinaMN75.com,BetterSoft1234"u8.ToArray())
// 			};
// 		});
//
// 		builder.Services.AddAuthorization();
// 	}
//
// 	public static void UseUtilitiesServices(this WebApplication app) {
// 		app.UseCors(option => option.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
// 		app.UseRateLimiter();
// 		app.UseOutputCache();
// 		app.UseDeveloperExceptionPage();
// 		app.UseUtilitiesSwagger();
// 		app.UseStaticFiles();
// 		// app.Use(async (context, next) => {
// 		// 	await next();
// 		// 	if (context.Response.StatusCode == 401)
// 		// 		await context.Response.WriteAsJsonAsync(new GenericResponse(UtilitiesStatusCodes.UnAuthorized));
// 		// });
//
// 		app.UseAuthentication();
// 		app.UseAuthorization();
//
// 		// app.UseMiddleware<EncryptionMiddleware>();
// 		app.Use(async (httpContext, next) => {
// 			string body = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
//
// 			byte[] data = Convert.FromBase64String(body);
// 			string decodedString = Encoding.UTF8.GetString(data);
// 			httpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(decodedString));
// 			await next();
//
// 			// httpContext.Response.Body = EncryptStream(httpContext.Response.Body);
// 			// httpContext.Request.Body = DecryptStream(httpContext.Request.Body);
// 		});
// 		// app.Use((httpContext, next) => {
// 		// 	httpContext.Response.Body = EncryptStream(httpContext.Response.Body);
// 		// 	// httpContext.Request.Body = DecryptStream(httpContext.Request.Body);
// 		// 	return next();
// 		// });
// 		// app.Run(async context => {
// 		// 	string str = "nnnnnnnnnnnnnnnnnnnnnnnnnnn";
// 		// 	Console.WriteLine(str);
// 		// 	await context.Response.WriteAsync(str);
// 		// });
// 	}
//
// 	private static void UseUtilitiesSwagger(this IApplicationBuilder app) {
// 		app.UseSwagger();
// 		app.UseSwaggerUI(c => {
// 			c.DocExpansion(DocExpansion.None);
// 			c.DefaultModelsExpandDepth(2);
// 		});
// 	}
// 	
// 	private static CryptoStream EncryptStream(Stream responseStream) {
// 		Aes aes = GetEncryptionAlgorithm();
// 		CryptoStream base64EncodedStream = new CryptoStream(responseStream, new ToBase64Transform(), CryptoStreamMode.Write);
// 		CryptoStream cryptoStream = new CryptoStream(base64EncodedStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write);
// 		//CryptoStream.FlushFinalBlock() // this will not work because this needs to be push at the end of stream.
// 		return cryptoStream;
// 	}
// 	
// 	private static Stream DecryptStream(Stream cipherStream) {
// 		Aes aes = GetEncryptionAlgorithm();
// 		CryptoStream base64DecodedStream = new CryptoStream(cipherStream, new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces), CryptoStreamMode.Read);
// 		CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read);
// 		Console.WriteLine("MMMMMMMMMMM");
// 		return decryptedStream;
// 	}
// 	
// 	private static Aes GetEncryptionAlgorithm() {
// 		AesManaged aes = new AesManaged {
// 			Padding = PaddingMode.PKCS7,
// 			// Key = Convert.FromBase64String("73kczzrPtnn5GXxQtOI6m4AewK34F4IkT/yaeYZxr+M="),
// 			// IV = Convert.FromBase64String("b8LAfmzY8WxGNrZeTye4hw=="),
// 			Key = Convert.FromBase64String("73kczzrPtnn5GXxQtOI6m4AewK34F4IkT/yaeYZxr+M="),
// 			IV = Convert.FromBase64String("b8LAfmzY8WxGNrZeTye4hw=="),
// 		};
// 		return aes;
// 	}
// 	
// 	public class EncryptionMiddleware
// 	{
// 		private readonly RequestDelegate _next;
// 		public EncryptionMiddleware(RequestDelegate next)
// 		{
// 			_next = next;
// 		}
// 		// public async Task Invoke(HttpContext httpContext)
// 		// {
// 		// 	httpContext.Response.Body = EncryptStream(httpContext.Response.Body);
// 		// 	// httpContext.Request.Body = DecryptStream(httpContext.Request.Body);
// 		// 	// if (httpContext.Request.QueryString.HasValue)
// 		// 	// {
// 		// 	// 	string decryptedString = DecryptString(httpContext.Request.QueryString.Value.Substring(1));
// 		// 	// 	httpContext.Request.QueryString = new QueryString($"?{decryptedString}");
// 		// 	// }
// 		// 	// await httpContext.Request.Body.DisposeAsync();
// 		// 	// await httpContext.Response.Body.DisposeAsync();
// 		// 	await _next(httpContext);
// 		// }
// 		
// 		public async Task Invoke(HttpContext context)
// 		{
// 			// if (context.Request.ContentType.Equals("text/plain"))
// 			// {
// 				string body = await new StreamReader(context.Response.Body).ReadToEndAsync();
//
// 				byte[] data = Convert.FromBase64String(body);
// 				string decodedString = Encoding.UTF8.GetString(data);
//
// 				// context.Response.ContentType = "application/json";
// 				context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(decodedString));
// 			// }
//
// 			await _next.Invoke(context);
// 		}
// 		
// 	}
// }
//
// public class MyCustomFilter : ActionFilterAttribute
// {
// 	private MemoryStream responseBody ;
//
// 	public override void OnActionExecuting(ActionExecutingContext context){
// 		this.responseBody=new MemoryStream();
// 		// hijack the real stream with our own memory stream 
// 		context.HttpContext.Response.Body = responseBody;
// 	}
//
// 	public override void OnResultExecuted(ResultExecutedContext context)
// 	{
//
// 		responseBody.Seek(0, SeekOrigin.Begin);
//
// 		// read our own memory stream 
// 		using (StreamReader sr = new StreamReader(responseBody))
// 		{
// 			var actionResult= sr.ReadToEnd();
// 			Console.WriteLine(actionResult);
// 			// create new stream and assign it to body 
// 			// context.HttpContext.Response.Body = ;
// 		}
//
// 		// no ERROR on the next line!
//
// 		base.OnResultExecuted(context);
// 	}
// }