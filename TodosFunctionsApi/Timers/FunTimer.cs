//using System;
//using System.Reflection;
//using System.Threading.Tasks;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Logging;
//using TodosCosmos;
//using TodosCosmos.DocumentClasses;
//using TodosGlobal;

//namespace TodosFunctionsApi.Timers
//{
//    public static class FunTimer
//    {
//        [FunctionName("FunTimer1")]
//        public static async Task Timer1([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, ILogger log)
//        {

//            CosmosDocSetting setting = await CosmosAPI.cosmosDBClientSetting.GetSetting(Guid.Empty, "LatsStatRequest");

//            if (setting != null)
//            {

//                if (GlobalFunctions.ToUnixEpochDate(DateTime.Now.AddMinutes(-1)) < setting.TimeStamp)
//                {
//                    await CosmosAPI.cosmosDBClientUser.UpdateOfflineUsers();
//                    await CosmosAPI.cosmosDBClientUser.UpdateOnlineUsersCount();
//                }

//            }


//        }

//        [FunctionName("FunTimer2")]
//        public static async void Timer2([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
//        {

//            await CosmosAPI.cosmosDBClientEmailedCode.DeleteExpiredEmaiedCodes();

//            bool b;
//            CosmosDocSetting setting = await CosmosAPI.cosmosDBClientSetting.GetSetting(Guid.Empty, "DoActivityLog");

//            if (setting != null)
//            {
//                if (string.IsNullOrEmpty(setting.Value))
//                {
//                    if (CosmosAPI.DoActivityLog)
//                    {
//                        CosmosAPI.DoActivityLog = false;
//                    }
//                }
//                else
//                {
//                    b = bool.Parse(setting.Value);
//                    if (CosmosAPI.DoActivityLog != b)
//                    {
//                        CosmosAPI.DoActivityLog = b;
//                    }
//                }
//            }

//            await CosmosAPI.cosmosDBClientTodo.SendTodoReminders();
//        }
//    }
//}