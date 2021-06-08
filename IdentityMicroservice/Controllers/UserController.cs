using AutoMapper;
using IdentityApi.ViewModels;
using IdentityCore.Interfaces.Services;
using IdentityCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserGroupService _userGroupService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IUserGroupService userGroupService, IMapper mapper)
        {
            _userService = userService;
            _userGroupService = userGroupService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PortalUserGetView>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(_mapper.Map<IEnumerable<PortalUserGetView>>(await _userService.GetAllUsers()));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PortalUserGetView), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = _mapper.Map<PortalUserGetView>(await _userService.GetUser(id));

            if (user == null) return NotFound();

            return Ok(user);
        }

        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(PortalUserGetView), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUsernameId(string username)
        {
            var user = _mapper.Map<PortalUserGetView>(await _userService.GetUserByUsername(username));

            if (user == null) return NotFound();

            return Ok(user);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PortalUserGetView), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<IdentityError>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(PortalUserPostView user, string password)
        {
            if (string.IsNullOrEmpty(password)) return BadRequest("Password must be specified.");

            var response = await _userService.CreateUser(_mapper.Map<IdentityMSUser>(user), password);

            if (!response.IsSuccess) return BadRequest(response.Errors);

            return Ok(_mapper.Map<PortalUserGetView>(response.User));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();

            await _userService.DeleteUser(user);

            return Ok();
        }

        [HttpPost("ChangePassword/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<IdentityError>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangePassword(string username, string currentPassword, 
            string newPassword, bool forcePasswordChange = false)
        {
            var result = await _userService.ChangePassword(username, currentPassword, newPassword, forcePasswordChange);
            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("PasswordResetByAdministrator/{username}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<IdentityError>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PasswordResetByAdministrator(string username,
                    string newPassword, bool forcePasswordChange = false)
        {
            var result = await _userService.PasswordResetByAdministrator(username, newPassword, forcePasswordChange);
            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("ForgotPassword/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            var result = await _userService.ForgottenPasswordRequest(username);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("ForgotPasswordComplete/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<IdentityError>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPasswordComplete(string username, string token, string password)
        {
            var result = await _userService.ForgottenPasswordComplete(username, token, password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpGet("{tenantId:int}/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> SearchUsersPaged(int tenantId, int page, int pageSize, string searchTerm = null)
        {
            var results = await _userService.SearchUsersPaged(tenantId, page, pageSize, searchTerm);
            return Ok(results);
        }

        [HttpGet("searchalltenants{page:int}/{pageSize:int}")]
        public async Task<IActionResult> SearchUsersPagedAllTenantsByUsername(int page, int pageSize, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                throw new ArgumentException("searchTerm cannot be null");

            var results = await _userService.SearchUsersPagedAllTenantsByUsername(page, pageSize, searchTerm);
            return Ok(results);
        }

        //[HttpGet("moduleAccessRelations/{userId:int}")]
        //public async Task<IActionResult> GetUserModuleAccessRelations(int userId)
        //{
        //    try
        //    {
        //        var results = await _userService.GetUserModuleAccessRelations(userId);
        //        return Ok(results);
        //    }
        //    catch (UserNotFoundException ex)
        //    {
        //        return NotFound("User was not found");
        //    }
        //}

        //[HttpPost("saveUserConfigurations/{tenantId:int}")]
        //[ProducesResponseType(typeof(UsersConfigurationsGetView), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(UsersConfigurationsGetView), StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> SaveUserConfigurations(UsersConfigurationsViewModel usersModel)
        //{
        //    var request = _mapper.Map<UsersConfigurations>(usersModel);
        //    var result = await _userService.SaveUserConfigurations(request);
        //    var viewModel = _mapper.Map<UsersConfigurationsGetView>(result);

        //    if (!viewModel.Succeeded)
        //        return UnprocessableEntity(viewModel);

        //    return Ok(viewModel);
        //}


        //[HttpPost("updateUserConfiguration/{tenantId:int}")]
        //[ProducesResponseType(typeof(ChangedUserConfigurationGetView), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ChangedUserConfigurationGetView), StatusCodes.Status422UnprocessableEntity)]
        //public async Task<IActionResult> UpdateUserConfiguration(ChangedUserConfigurationPostView userModel)
        //{
        //    var request = _mapper.Map<ChangedUserConfiguration>(userModel);
        //    var result = await _userService.UpdateUser(request);
        //    var viewModel = _mapper.Map<ChangedUserConfigurationGetView>(result);

        //    if (!viewModel.Succeeded)
        //        return UnprocessableEntity(viewModel);

        //    return Ok(viewModel);
        //}

        [HttpPut("userGroup/{userGroupId:int}/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddUserToUserGroup(int userGroupId, int userId)
        {
            if (userGroupId <= 0 || userId <= 0) return BadRequest("Ids must be greater than zero");

            await _userGroupService.AddUserToUserGroup(userGroupId, userId);
            return Ok();
        }

        [HttpDelete("userGroup/{userGroupId:int}/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveUserFromUserGroup(int userGroupId, int userId)
        {
            if (userGroupId <= 0 || userId <= 0) return BadRequest("Ids must be greater than zero");

            await _userGroupService.RemoveUserFromUserGroup(userGroupId, userId);
            return Ok();
        }

        [HttpGet("emailInUse")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckIfEmailExists(string email)
        {
            return Ok(await _userService.CheckIfEmailExists(email));
        }

        [HttpGet("userNameInUse")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckIfUserNameExists(string userName)
        {
            return Ok(await _userService.CheckIfUserNameExists(userName));
        }

        [HttpGet("emailInUseByOther")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public Task<bool> CheckIfEmailExistsForOtherUser(string email, int currentUserId)
        {
            return _userService.CheckIfEmailExistsForOtherUser(email, currentUserId);
        }

        [HttpGet("userNameInUseByOther")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public Task<bool> CheckIfUserNameExistsForOtherUser(string userName, int currentUserId)
        {
            return _userService.CheckIfUserNameExistsForOtherUser(userName, currentUserId);
        }

        [HttpDelete("deleteUserForInactiveTenant/{tenantId}")]
        public async Task<IActionResult> DeleteUserForInactiveTenant(int tenantId)
        {
            var result = await _userService.DeleteUserForInactiveTenant(tenantId);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("updateIsAdministrator")]
        public async Task<IActionResult> UpdateIsAdministratort(int userId,bool isAdministrator)
        {
            var result = await _userService.UpdateIsAdministrator(userId, isAdministrator);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }
    }
}