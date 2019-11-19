﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.TechnicalAdvisor;
using RX.Nyss.Web.Features.TechnicalAdvisor.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.TechnicalAdvisor
{
    public class TechnicalAdvisorServiceTests
    {
        private readonly TechnicalAdvisorService _technicalAdvisorService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;

        public TechnicalAdvisorServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _technicalAdvisorService = new TechnicalAdvisorService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });

            SetupTestNationalSocieties();
        }

        private User ArrangeUsersDbSetWithOneTechnicalAdvisor()
        {
            var technicalAdvisor = new TechnicalAdvisorUser
            {
                Id = 123,
                Role = Role.TechnicalAdvisor,
                EmailAddress = "emailTest1@domain.com",
                Name = "emailTest1@domain.com",
                Organization = "org org",
                PhoneNumber = "123",
                AdditionalPhoneNumber = "321"
            };

            ArrangeUsersFrom(new List<User> { technicalAdvisor });
            return technicalAdvisor;
        }


        private User ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety()
        {
            var technicalAdvisor = ArrangeUsersDbSetWithOneTechnicalAdvisor();

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety {User = technicalAdvisor, UserId = technicalAdvisor.Id, NationalSocietyId = 1, NationalSociety = _nyssContext.NationalSocieties.Find(1)},
            };

            ArrangeUserNationalSocietiesFrom(userNationalSocieties);
            technicalAdvisor.UserNationalSocieties = userNationalSocieties;

            return technicalAdvisor;
        }


        private User ArrangeUsersDbSetWithOneTechnicalAdvisorInTwoNationalSocieties()
        {
            var technicalAdvisor = ArrangeUsersDbSetWithOneTechnicalAdvisor();

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety {User = technicalAdvisor, UserId = technicalAdvisor.Id, NationalSocietyId = 1, NationalSociety = _nyssContext.NationalSocieties.Find(1)},
                new UserNationalSociety {User = technicalAdvisor, UserId = technicalAdvisor.Id, NationalSocietyId = 2, NationalSociety = _nyssContext.NationalSocieties.Find(2)},
            };

            ArrangeUserNationalSocietiesFrom(userNationalSocieties);
            technicalAdvisor.UserNationalSocieties = userNationalSocieties;

            return technicalAdvisor;
        }

        private void ArrangeUsersFrom(IEnumerable<User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);

            _nationalSocietyUserService.GetNationalSocietyUser<TechnicalAdvisorUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = existingUsers.OfType<TechnicalAdvisorUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                return user;
            });

            _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<TechnicalAdvisorUser>(Arg.Any<int>())
                .Returns(ci => _nationalSocietyUserService.GetNationalSocietyUser<TechnicalAdvisorUser>((int)ci[0]));
        }

        private void ArrangeUserNationalSocietiesFrom(IEnumerable<UserNationalSociety> userNationalSocieties)
        {
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
        }

        private void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User> { new AdministratorUser() { Id = 123, Role = Role.Administrator } });


        private void SetupTestNationalSocieties()
        {
            var nationalSociety1 = new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "Test national society 1" };
            var nationalSociety2 = new RX.Nyss.Data.Models.NationalSociety { Id = 2, Name = "Test national society 2" };
            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety> { nationalSociety1, nationalSociety2 };
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSociety1);
            _nyssContext.NationalSocieties.FindAsync(2).Returns(nationalSociety2);
        }

        [Fact]
        public async Task RegisterTechnicalAdvisor_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var registerTechnicalAdvisorRequestDto = new CreateTechnicalAdvisorRequestDto
            {
                Name = userName,
                Email = userEmail
            };

            var nationalSocietyId = 1;
            var result = await _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, registerTechnicalAdvisorRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RegisterTechnicalAdvisor_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerTechnicalAdvisorRequestDto = new CreateTechnicalAdvisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, registerTechnicalAdvisorRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task RegisterTechnicalAdvisor_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerTechnicalAdvisorRequestDto = new CreateTechnicalAdvisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, registerTechnicalAdvisorRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterTechnicalAdvisor_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerTechnicalAdvisorRequestDto = new CreateTechnicalAdvisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, registerTechnicalAdvisorRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void RegisterTechnicalAdvisor_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerTechnicalAdvisorRequestDto = new CreateTechnicalAdvisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, registerTechnicalAdvisorRequestDto).ShouldThrowAsync<Exception>();
        }


        [Fact]
        public async Task EditTechnicalAdvisor_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            ArrangeUsersFrom(new List<User> { });


            var result = await _technicalAdvisorService.UpdateTechnicalAdvisor(123, new EditTechnicalAdvisorRequestDto() {});


            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditTechnicalAdvisor_WhenEditingUserThatIsNotTechnicalAdvisor_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var result = await _technicalAdvisorService.UpdateTechnicalAdvisor(123, new EditTechnicalAdvisorRequestDto() { });

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditTechnicalAdvisor_WhenEditingUserThatIsNotTechnicalAdvisor_SaveChangesShouldNotBeCalled()
        {
            ArrangeUsersWithOneAdministratorUser();

            await _technicalAdvisorService.UpdateTechnicalAdvisor(123, new EditTechnicalAdvisorRequestDto() { });

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task EditTechnicalAdvisor_WhenEditingExistingTechnicalAdvisor_ReturnsSuccess()
        {
            ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            var result = await _technicalAdvisorService.UpdateTechnicalAdvisor(123, new EditTechnicalAdvisorRequestDto() {  });

            result.IsSuccess.ShouldBeTrue();
        }

        


        [Fact]
        public async Task EditTechnicalAdvisor_WhenEditingExistingTechnicalAdvisor_SaveChangesAsyncIsCalled()
        {
            ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            await _technicalAdvisorService.UpdateTechnicalAdvisor(123, new EditTechnicalAdvisorRequestDto() {  });

            await _nyssContext.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task EditTechnicalAdvisor_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var editRequest = new EditTechnicalAdvisorRequestDto()
            {
                Name = "New name",
                Organization = "New organization",
                PhoneNumber = "345",
                AdditionalPhoneNumber = "543",
            };

            await _technicalAdvisorService.UpdateTechnicalAdvisor(123, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as TechnicalAdvisorUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.AdditionalPhoneNumber.ShouldBe(editRequest.AdditionalPhoneNumber);
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenSuccess_SaveChangesIsCalledOnce()
        {
            //arrange
            ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            //act
            await _technicalAdvisorService.DeleteTechnicalAdvisor(1, 123);

            //assert
            await _nyssContext.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeletingFromNationalSocietyTheUserIsNotIn_ShouldReturnError()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            //act
            var result = await _technicalAdvisorService.DeleteTechnicalAdvisor(2, 123);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserIsNotAssignedToThisNationalSociety);
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeletingAUserThatDoesNotExist_ShouldReturnError()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            //act
            var result = await _technicalAdvisorService.DeleteTechnicalAdvisor(2, 321);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserNotFound);
        }


        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeletingFromLastNationalSociety_RemoveOnNyssUserIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            //act
            await _technicalAdvisorService.DeleteTechnicalAdvisor(1, 123);

            //assert
            await _nationalSocietyUserService.Received(1).DeleteNationalSocietyUser(user);
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeletingFromLastNationalSociety_RemoveOnNationalSocietyReferenceIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneTechnicalAdvisorInOneNationalSociety();

            //act
            await _technicalAdvisorService.DeleteTechnicalAdvisor(1, 123);

            //assert
            _nyssContext.UserNationalSocieties.Received(1).Remove(Arg.Is<UserNationalSociety>(uns => uns.NationalSocietyId == 1 && uns.UserId == 123));
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeletingFromNotLastNationalSociety_RemoveOnNyssUserIsNotCalled()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneTechnicalAdvisorInTwoNationalSocieties();

            //act
            await _technicalAdvisorService.DeleteTechnicalAdvisor(1, 123);

            //assert
            await _nationalSocietyUserService.Received(0).DeleteNationalSocietyUser(user);
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeletingFromNotLastNationalSociety_RemoveOnNationalSocietyReferenceIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneTechnicalAdvisorInTwoNationalSocieties();

            //act
            await _technicalAdvisorService.DeleteTechnicalAdvisor(1, 123);

            //assert
            _nyssContext.UserNationalSocieties.Received(1).Remove(Arg.Is<UserNationalSociety>(uns => uns.NationalSocietyId == 1 && uns.UserId == 123));
        }
    }
}