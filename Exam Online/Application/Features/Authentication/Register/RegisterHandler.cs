using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;

namespace Exam_Online_API.Application.Features.Authentication.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly UserManager<User> _userManager;

        public RegisterHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new ConflictException("Email is already registered");

            if (await _userManager.FindByNameAsync(request.UserName) != null)
                throw new ConflictException("Username is already taken");

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber


            };
            
            if (request.IsTwoFactorEnabled)
                user.IsTwoFactorEnabled = true;

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new BusinessLogicException(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "User");

            
            return new RegisterResponse { UserId = user.Id, Email = user.Email };
        }
    }
    //    {
    //  "userName": "islam",
    //  "firstName": "Islam",
    //  "lastName": "Klieb",
    //  "email": "deonte83@ethereal.email",
    //  "phoneNumber": "01121111211",
    //  "password": "@Islam011",
    //  "confirmPassword": "@Islam011",
    //  "isTwoFactorEnabled": true
    //}
}
