﻿#region

global using System;
global using System.Collections.Generic;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Threading.RateLimiting;
global using System.Reflection;
global using System.Globalization;
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
global using Utilities_aspnet.Utilities;
global using Utilities_aspnet.Entities;
global using Utilities_aspnet.Repositories;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;
global using Newtonsoft.Json.Converters;
global using Amazon.Runtime;
global using Amazon.S3;
global using Amazon.S3.Model;
global using RestSharp;
global using Swashbuckle.AspNetCore.SwaggerUI;
global using OfficeOpenXml;
global using System.Data;
global using ClosedXML.Excel;
global using Microsoft.Extensions.Hosting;

#endregion