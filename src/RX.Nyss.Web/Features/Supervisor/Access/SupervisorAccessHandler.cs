﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSociety.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Supervisor.Access
{
    public class SupervisorAccessHandler : ResourceAccessHandler<SupervisorAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public SupervisorAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("supervisorId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int supervisorId) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToUserNationalSocieties(supervisorId);
    }
}