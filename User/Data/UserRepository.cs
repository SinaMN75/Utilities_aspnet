﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Utilities_aspnet.Models.Dto;
using Utilities_aspnet.User.Dtos;
using Utilities_aspnet.User.Entities;
using Utilities_aspnet.Utilities;
using Utilities_aspnet.Utilities.Data;
using Utilities_aspnet.Utilities.Entities;
using Utilities_aspnet.Utilities.Enums;
using Utilities_aspnet.Utilities.Responses;

namespace Utilities_aspnet.User.Data;

public interface IUserRepository {
    Task<GenericResponse<UserReadDto?>> RegisterWithEmail(RegisterWithEmailDto dto);
    Task<GenericResponse<UserReadDto?>> LoginWithEmail(LoginWithEmailDto dto);
    Task<GenericResponse<string>> RegisterWithMobile(RegisterWithMobileDto dto);
    Task<GenericResponse<UserReadDto?>> LoginWithMobile(LoginWithMobileDto dto);
    Task<GenericResponse<UserReadDto?>> GetProfile(string userName, string? token);
    Task<GenericResponse<UserReadDto?>> UpdateUser(UpdateProfileDto model, string userName);
}

public class UserRepository : IUserRepository {
    private readonly UserManager<UserEntity> _userManager;
    private readonly DbContext _context;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;
    private readonly IOtpService _otp;

    public UserRepository(DbContext context, UserManager<UserEntity> userManager, IConfiguration config, IMapper mapper, IOtpService otp) {
        _context = context;
        _userManager = userManager;
        _config = config;
        _otp = otp;
        _mapper = mapper;
    }
    
    public async Task<GenericResponse<UserReadDto?>> LoginWithEmail(LoginWithEmailDto model) {
        UserEntity? user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null) return new GenericResponse<UserReadDto?>(null, UtilitiesStatusCodes.NotFound, "Email not found");

        bool result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!result) return new GenericResponse<UserReadDto?>(null, UtilitiesStatusCodes.BadRequest, "The password is incorrect!");

        JwtSecurityToken token = await CreateToken(user);

        return new GenericResponse<UserReadDto?>(GetProfile(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result,
            message: "Success");
    }

    public async Task<GenericResponse<UserReadDto?>> LoginWithMobile(LoginWithMobileDto model) {
        UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == model.Mobile);

        if (user == null) return new GenericResponse<UserReadDto>(null, UtilitiesStatusCodes.NotFound, "Mobile not found");

        if (_otp.Verify(user.Id, model.VerificationCode) != OtpResult.Ok)
            return new GenericResponse<UserReadDto>(null, UtilitiesStatusCodes.BadRequest, "Verification Code Is Not Valid");
        JwtSecurityToken token = await CreateToken(user);

        return new GenericResponse<UserReadDto?>(GetProfile(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result,
            UtilitiesStatusCodes.Success, "Success");
    }

    public async Task<GenericResponse<UserReadDto?>> RegisterWithEmail(RegisterWithEmailDto aspNetUser) {
        UserEntity? model = _context.Set<UserEntity>().FirstOrDefault(x => x.UserName == aspNetUser.UserName ||
                                                                           x.Email == aspNetUser.Email);
        if (model != null) {
            return new GenericResponse<UserReadDto?>(null, UtilitiesStatusCodes.BadRequest, "This email or username already exists");
        }

        UserEntity user = new() {
            Email = aspNetUser.Email,
            UserName = aspNetUser.UserName,
            LastLogin = null,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            CreateAccount = DateTime.Now
        };

        IdentityResult? result = await _userManager.CreateAsync(user, aspNetUser.Password);
        if (!result.Succeeded)
            return new GenericResponse<UserReadDto?>(null, UtilitiesStatusCodes.BadRequest, "The information was not entered correctly");

        JwtSecurityToken token = await CreateToken(user);

        return new GenericResponse<UserReadDto?>(GetProfile(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result,
            UtilitiesStatusCodes.Success, "Success");
    }

    public async Task<GenericResponse<string>> RegisterWithMobile(RegisterWithMobileDto aspNetUser) {
        UserEntity? model = _context.Set<UserEntity>().FirstOrDefault(x => x.PhoneNumber == aspNetUser.Mobile);
        if (model != null) {
            string? otp = _otp.SendOtp(model.Id);
            return new GenericResponse<string>(otp, UtilitiesStatusCodes.Success, "Success");
        }
        else {
            UserEntity user = new() {
                PhoneNumber = aspNetUser.Mobile,
                UserName = aspNetUser.Mobile.Replace("+", ""),
                LastLogin = null,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                CreateAccount = DateTime.Now
            };

            IdentityResult? result = await _userManager.CreateAsync(user, "P@ssw0rd!@#$%^&*");
            if (!result.Succeeded)
                return new GenericResponse<string>("", UtilitiesStatusCodes.BadRequest, "The information was not entered correctly");

            string? otp = _otp.SendOtp(user.Id);
            return new GenericResponse<string>(otp, UtilitiesStatusCodes.Success, "Success");
        }
    }

    public Task<GenericResponse<UserReadDto?>> GetProfile(string userId, string? token) {
        UserEntity? model = _context.Set<UserEntity>().Include(u => u.Media).FirstOrDefault(u => u.Id == userId);
        UserReadDto userReadDto = _mapper.Map<UserReadDto>(model);
        userReadDto.Token = token;

        return Task.FromResult(new GenericResponse<UserReadDto?>(userReadDto, UtilitiesStatusCodes.Success, "Success"));
    }



    public async Task<GenericResponse<UserReadDto?>> UpdateUser(UpdateProfileDto model, string userName)
    {
        var user = _context.Set<UserEntity>().FirstOrDefault(x => x.Id == userName);
        if (user == null) { return new GenericResponse<UserReadDto?>(null , UtilitiesStatusCodes.NotFound, "Not Found"); }
        try
        {
            
            if (model.FullName != null) user.FullName = model.FullName;
            if (model.Bio != null) user.Bio = model.Bio;
            //if (model.BirthDate != null) user.BirthDate = model.BirthDate;
            if (model.UserName != null) user.UserName = model.UserName;
            //if (model.LocationId != null) user.LocationId = model.LocationId;
            if (model.Degree != null) user.Degree = model.Degree;
            if (model.Education != null) user.Education = model.Education;
            if (model.Headline != null) user.Headline = model.Headline;
            _context.SaveChanges();
            if (model.contactInformations != null)
            {
                UserEntity users = await _context.Set<UserEntity>().Include(x => x.ContactInformations).FirstOrDefaultAsync(x => x.Id == user.Id);
                _context.Set<ContactInformationEntity>().RemoveRange(users.ContactInformations);
                foreach (var information in model.contactInformations)
                {
                    var contactInfoItem = _context.Set<ContactInfoItemEntity>().Find(information.ContactInfoItemId);
                    if (contactInfoItem == null)
                    {
                        return new GenericResponse<UserReadDto?>(null, UtilitiesStatusCodes.BadRequest, "The information was not entered correctly");
                    }
                    _context.Set<ContactInformationEntity>().Add(new ContactInformationEntity
                    {
                        Value = information.Value,
                        UserId = users.Id,
                        Visibility = information.Visibility,
                        ContactInfoItem = contactInfoItem
                    });
                    _context.SaveChanges();
                }
            }


        }
        catch
        {
            return new GenericResponse<UserReadDto?>(null, UtilitiesStatusCodes.BadRequest, "Bad Request");
        }


        return new GenericResponse<UserReadDto?>(GetProfile(user.Id, "").Result.Result, UtilitiesStatusCodes.Success, "Success");

    }


    private async Task<JwtSecurityToken> CreateToken(UserEntity user) {
        IList<string>? roles = await _userManager.GetRolesAsync(user);
        List<Claim>? claims = new() {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (roles != null) claims.AddRange(roles.Select(role => new Claim("role", role)));
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken token = new(_config["Tokens:Issuer"], _config["Tokens:Issuer"], claims, expires: DateTime.Now.AddDays(365),
            signingCredentials: creds);

        user.LastLogin = DateTime.Now;
        await _userManager.UpdateAsync(user);
        return token;
    }
}