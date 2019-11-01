﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Features.UserVerification.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.UserVerification
{
    [Route("api/userVerification")]
    public class UserVerificationController : BaseController
    {
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        public UserVerificationController(IIdentityUserRegistrationService identityUserRegistrationService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
        }

        [HttpPost("verifyEmailAndAddPassword"), AllowAnonymous]
        public async Task<Result> VerifyAndStorePassword([FromBody] VerifyAndStorePasswordRequestDto request)
        {
            var verificationResult = await _identityUserRegistrationService.VerifyEmail(request.Email, request.Token);
            if (!verificationResult.IsSuccess)
            {
                return verificationResult;
            }

            return await _identityUserRegistrationService.AddPassword(request.Email, request.Password);
        }
    }
}