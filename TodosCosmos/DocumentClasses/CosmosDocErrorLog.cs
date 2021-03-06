﻿using System;
using static TodosCosmos.Enums;

namespace TodosCosmos.DocumentClasses
{
    public class CosmosDocErrorLog : BaseDocType
    {
        public CosmosDocErrorLog(Guid pUserID, string pDescription, string pMethodName)
        {
            UserID = pUserID;
            ID = Guid.NewGuid();
            Description = pDescription;
            MethodName = pMethodName;
            DocType = (int)DocTypeEnum.Error;
            GeneratePK();
        }

        public Guid UserID { get; set; }
        public string Description { get; set; }
        public string MethodName { get; set; }
    }
}