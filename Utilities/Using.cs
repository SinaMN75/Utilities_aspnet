#region

global using System;
global using System.Collections.Generic;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Threading.RateLimiting;
global using System.Text;
global using System.Diagnostics;
global using System.Security.Cryptography;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.AspNetCore.Mvc.Infrastructure;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.Extensions.Configuration;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.AspNetCore.OutputCaching;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.Extensions.Primitives;
global using Utilities_aspnet.Utilities;
global using Utilities_aspnet.Entities;
global using Utilities_aspnet.Repositories;
global using Swashbuckle.AspNetCore.SwaggerUI;

#endregion