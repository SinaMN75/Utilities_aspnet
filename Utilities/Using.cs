﻿global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Text.RegularExpressions;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.AspNetCore.Mvc.Infrastructure;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.Extensions.Configuration;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.ResponseCompression;
global using Microsoft.AspNetCore.OutputCaching;
global using Utilities_aspnet.Utilities;
global using Utilities_aspnet.Entities;
global using Utilities_aspnet.Repositories;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;
global using StackExchange.Redis;
global using RestSharp;
global using Swashbuckle.AspNetCore.SwaggerUI;
global using Zarinpal;
global using Zarinpal.Models;
global using System.IO.Compression;
global using Ghasedak.Core;
global using System.Security.Cryptography;
global using Utilities_aspnet.Hubs;
global using Microsoft.AspNetCore.SignalR;
global using System.Threading.RateLimiting;
global using Microsoft.AspNetCore.RateLimiting;
global using System.Reflection;
global using Swashbuckle.AspNetCore.SwaggerGen;
global using System.Net.Http.Headers;