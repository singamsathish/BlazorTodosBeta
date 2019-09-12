using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using TodosCosmos;
using TodosGlobal;

namespace TodosFunctionsApi.API
{
    public class FunSetupdata
    {

        [FunctionName("FunSetupdata")]
        public async Task<string> Setupdata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Setupdata")] HttpRequest req,
            ILogger log)
        {
            await CosmosAPI.cosmosDBClientActivity.AddActivityLog(Guid.Empty, "Requested setup data", MethodBase.GetCurrentMethod());

            await CosmosAPI.cosmosDBClientVisitor.AddVisitor(req.HttpContext.Connection.RemoteIpAddress.ToString());


            return GlobalFunctions.CmdGetPublicData();
        }



       
    }
}