﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Collections.Generic;
using TodosShared;
using System.Security.Claims;
using TodosFunctionsApi.JwtSecurity;
using static TodosShared.TSEnums;
using TodosCosmos;
using static TodosGlobal.GlobalClasses;
using TodosGlobal;
using TodosCosmos.DocumentClasses;

namespace TodosFunctionsApi.API
{
    public class FunUser
    {

    
        private List<WebApiUserTypesEnum> AllowedRoles = new List<WebApiUserTypesEnum>
            {
                WebApiUserTypesEnum.Authorized,
                WebApiUserTypesEnum.Admin
            };


        [FunctionName("FunUserGetAll")]
        public async Task<ActionResult<IEnumerable<TSUser>>> GetAll(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/getall")] HttpRequest req,
           ILogger log)
        {

            List<WebApiUserTypesEnum>  localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                WebApiUserTypesEnum.Admin
            };

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);


            Guid UserID =Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "Requested all users", MethodBase.GetCurrentMethod());

            List<TSUser> users = new List<TSUser>();


                users = await CosmosAPI.cosmosDBClientUser.GetAllUsers();

           

            return users;
        }

        [FunctionName("FunUserAuthorize")]
        public async Task<ActionResult<TSUser>> Authorize(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/authorize")] HttpRequest req,
          ILogger log)
        {
          
            TSUser tsUser = await MyFromBody<TSUser>.FromBody(req);

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, AllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "Requested authentication", MethodBase.GetCurrentMethod());

            TSUser result = new TSUser();

           
                tsUser.ID = UserID;

                result = CosmosAPI.cosmosDBClientUser.GetUser(tsUser).Result;

                if (!result.ID.Equals(Guid.Empty))
                {
                    await TodosCosmos.LocalFunctions.NotifyAdmin("New login " + result.UserName);

                    await CosmosAPI.cosmosDBClientSetting.UpdateSettingCounter(Guid.Empty, "LiveUsersCount", true);

                }
                else
                {

                    result.UserName = "Error!";
                    result.FullName = "Can't find user!";
                  
                }

           

            return result;
        }


        [FunctionName("FunUserSendMail")]
        public async Task<ActionResult<TSEmail>> SendMail(
         [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/sendmail")] HttpRequest req,
         ILogger log)
        {

            TSEmail tsEmail = await MyFromBody<TSEmail>.FromBody(req);

            List<WebApiUserTypesEnum> localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                WebApiUserTypesEnum.NotAuthorized,
                 WebApiUserTypesEnum.Authorized,
                  WebApiUserTypesEnum.Admin
            };

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);

      
            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "send mail", MethodBase.GetCurrentMethod());


            string MachineID = LocalFunctions.CmdGetValueFromClaim(User.Claims, "MachineID", 10);


 

            TSEmail result = await TodosCosmos.LocalFunctions.SendEmail(tsEmail,req.HttpContext.Connection.RemoteIpAddress.ToString(), MachineID);
            result.To = string.Empty;
            result.OperationCode = 0;

            return result;

        }


        [FunctionName("FunUserPassRecovery")]
        public async Task<ActionResult<TSEmail>> PassRecovery(
  [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/passrecovery")] HttpRequest req,
  ILogger log)
        {

            TSEmail tsEmail = await MyFromBody<TSEmail>.FromBody(req);

            List<WebApiUserTypesEnum> localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                WebApiUserTypesEnum.NotAuthorized,
                 WebApiUserTypesEnum.Authorized,
                  WebApiUserTypesEnum.Admin
            };

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "Pass Recovery", MethodBase.GetCurrentMethod());

         

            string newPass = GlobalFunctions.GetSalt();

            CosmosDocUser cosmosDocUser = await CosmosAPI.cosmosDBClientUser.FindUserByUserName(tsEmail.To);
            if (cosmosDocUser != null)
            {
                cosmosDocUser.Salt = GlobalFunctions.GetSalt();
                cosmosDocUser.HashedPassword = GlobalFunctions.CmdHashPasswordBytes(newPass, cosmosDocUser.Salt);

                bool b = await CosmosAPI.cosmosDBClientUser.UpdateUserDoc(cosmosDocUser);

                if (b)
                {
                    tsEmail.To = cosmosDocUser.Email;
                    TSEmail result = await TodosCosmos.LocalFunctions.SendEmail(tsEmail, string.Empty, newPass);
                    result.To = string.Empty;
                    result.OperationCode = 0;

                    return result;
                }
                else
                {
                    return new TSEmail { Result = "Error: Could not recover user password" };
                }
            }
            else
            {
                return new TSEmail { Result = "Error: User not found" };
            }
        }



        [FunctionName("FunUserCheckUserName")]
        public async Task<ActionResult<TSEmail>> CheckUserName(
  [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/checkusername")] HttpRequest req,
  ILogger log)
        {

            TSEmail tsEmail = await MyFromBody<TSEmail>.FromBody(req);

            List<WebApiUserTypesEnum> localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                WebApiUserTypesEnum.NotAuthorized,
                 WebApiUserTypesEnum.Authorized,
                  WebApiUserTypesEnum.Admin
            };

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "Check UserName", MethodBase.GetCurrentMethod());


           


            CosmosDocUser user = await CosmosAPI.cosmosDBClientUser.FindUserByUserName(tsEmail.To);

            TSEmail result = new TSEmail
            {
                To = string.Empty,
                OperationCode = 0
            };

            if (user is null)
            {
                result.Result = "OK";
            }
            else
            {
                result.Result = "UserName already exists";
            }

            return result;

        }



        [FunctionName("FunUserCheckEmail")]
        public async Task<ActionResult<TSEmail>> CheckEmail(
[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/checkemail")] HttpRequest req,
ILogger log)
        {

            TSEmail tsEmail = await MyFromBody<TSEmail>.FromBody(req);

            List<WebApiUserTypesEnum> localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                 WebApiUserTypesEnum.NotAuthorized,
                 WebApiUserTypesEnum.Authorized,
                 WebApiUserTypesEnum.Admin
            };

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "Check Email", MethodBase.GetCurrentMethod());


           


            CosmosDocUser user = await CosmosAPI.cosmosDBClientUser.FindUserByEmail(tsEmail.To);

            TSEmail result = new TSEmail
            {
                To = string.Empty,
                OperationCode = 0
            };

            if (user is null)
            {
                result.Result = "OK";
            }
            else
            {
                result.Result = "Email already exists";
            }

            return result;

        }



        [FunctionName("FunUserLogout")]
        public async Task<ActionResult> Logout(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/logout")] HttpRequest req,
ILogger log)
        {


            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, AllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));


            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "user logout", MethodBase.GetCurrentMethod());

            await CosmosAPI.cosmosDBClientSetting.UpdateSettingCounter(Guid.Empty, "LiveUsersCount", false);

            return new OkObjectResult("OK");
        }


        [FunctionName("FunUserAdd")]
        public async Task<ActionResult> Add(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/add")] HttpRequest req,
        ILogger log)
        {

            TSUser tsUser = await MyFromBody<TSUser>.FromBody(req);

            List<WebApiUserTypesEnum> localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                 WebApiUserTypesEnum.NotAuthorized,
                 WebApiUserTypesEnum.Authorized,
                  WebApiUserTypesEnum.Admin
            };


            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);

            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "post user", MethodBase.GetCurrentMethod());


           


            await CosmosAPI.cosmosDBClientEmailedCode.DeleteExpiredEmaiedCodes();


            string MachineID = LocalFunctions.CmdGetValueFromClaim(User.Claims, "MachineID", 10);

            string IPAddress = req.HttpContext.Connection.RemoteIpAddress.ToString();



            CosmosEmailedCode emailedCode = await CosmosAPI.cosmosDBClientEmailedCode.FindEmaiedCode(tsUser.Email, IPAddress, MachineID);

            if (emailedCode != null)
            {
                if (emailedCode.Code.ToLower().Equals(tsUser.EmailedCode))
                {

                    await CosmosAPI.cosmosDBClientEmailedCode.DeleteEmailedCodes(tsUser.Email);

                    tsUser.ID = Guid.NewGuid();
                    tsUser.CreateDate = DateTime.Now;


                    if (await CosmosAPI.cosmosDBClientUser.AddUser(tsUser))
                    {
                        await CosmosAPI.cosmosDBClientSetting.UpdateSettingCounter(Guid.Empty, "UsersCount", true);


                        await TodosCosmos.LocalFunctions.NotifyAdmin("New user");

                        return new OkObjectResult("OK");
                    }
                    else
                    {
                        return new OkObjectResult("Error:Can't add new user!");
                    }
                }
                else
                {
                    return new OkObjectResult("Error:Emailed code is not correct!");
                }
            }
            else
            {

                await CosmosAPI.cosmosDBClientError.AddErrorLog(Guid.Empty, "EmaiedCode expected but not found", MethodBase.GetCurrentMethod());
                return new OkObjectResult("Error:Server can't find emailed code to compare!");
            }

        }

        [FunctionName("ChangePassword")]
        public async Task<ActionResult> ChangePassword(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/changepassword")] HttpRequest req,
        ILogger log)
        {

            TSUser tsUser = await MyFromBody<TSUser>.FromBody(req);

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, AllowedRoles);

            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "change password", MethodBase.GetCurrentMethod());


           


            await CosmosAPI.cosmosDBClientEmailedCode.DeleteExpiredEmaiedCodes();


            string MachineID = LocalFunctions.CmdGetValueFromClaim(User.Claims, "MachineID", 10);

            string IPAddress = req.HttpContext.Connection.RemoteIpAddress.ToString();



            CosmosEmailedCode emailedCode = await CosmosAPI.cosmosDBClientEmailedCode.FindEmaiedCode(tsUser.Email, IPAddress, MachineID);

            if (emailedCode != null)
            {
                if (emailedCode.Code.ToLower().Equals(tsUser.EmailedCode))
                {

                    await CosmosAPI.cosmosDBClientEmailedCode.DeleteEmailedCodes(tsUser.Email);


                    TSUser currUser = (await CosmosAPI.cosmosDBClientUser.FindUserByID(UserID)).toTSUser();

                    currUser.Password = tsUser.Password;


                    if (await CosmosAPI.cosmosDBClientUser.UpdateUser(currUser, false))
                    {
                        await TodosCosmos.LocalFunctions.NotifyAdmin("password change");

                        return new OkObjectResult("OK");
                    }
                    else
                    {
                        return new OkObjectResult("Error:Can't add new user!");
                    }
                }
                else
                {
                    return new OkObjectResult("Error:Emailed code is not correct!");
                }
            }
            else
            {

                await CosmosAPI.cosmosDBClientError.AddErrorLog(Guid.Empty, "EmaiedCode expected but not found", MethodBase.GetCurrentMethod());
                return new OkObjectResult("Error:Server can't find emailed code to compare!");
            }

        }


        [FunctionName("FunUserGetLiveUsers")]
        public async Task<ActionResult<IEnumerable<TSUserOpen>>> GetLiveUsers(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/getliveusers")] HttpRequest req,
          ILogger log)
        {

            List<WebApiUserTypesEnum> localAllowedRoles = new List<WebApiUserTypesEnum>
            {
                WebApiUserTypesEnum.NotAuthorized,
                 WebApiUserTypesEnum.Authorized,
                  WebApiUserTypesEnum.Admin
            };

            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, localAllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));

            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "get live users", MethodBase.GetCurrentMethod());

          
                return await CosmosAPI.cosmosDBClientUser.GetLiveUsers();

            
        }


        [FunctionName("FunUserUpdate")]
        public async Task<ActionResult> Update(
     [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "User/update")] HttpRequest req,
     ILogger log)
        {

            TSUser tsUser = await MyFromBody<TSUser>.FromBody(req);


            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, AllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "put user", MethodBase.GetCurrentMethod());

           

                bool b = await CosmosAPI.cosmosDBClientUser.UpdateUser(tsUser, false);

                if (b)
                {

                    return new OkResult();
                }
                else
                {
                    return new UnprocessableEntityResult();
                }
           


        }


        [FunctionName("FunUserDelete")]
        public async Task<ActionResult> Delete(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "User/delete")] HttpRequest req,
    ILogger log)
        {

            TSUser tsUser = await MyFromBody<TSUser>.FromBody(req);


            ClaimsPrincipal User = MyTokenValidator.Authenticate(req, AllowedRoles);


            Guid UserID = Guid.Parse(LocalFunctions.CmdGetValueFromClaim(User.Claims, "UserID", 10));
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(UserID, "delete user", MethodBase.GetCurrentMethod());

          


                bool b = await CosmosAPI.cosmosDBClientUser.DeleteUser(tsUser);

                if (b)
                {

                    return new OkResult();
                }
                else
                {
                    return new UnprocessableEntityResult();
                }
           


        }
    }
}