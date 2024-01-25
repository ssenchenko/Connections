using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using MyCDK;

namespace CdkWorkshop
{
    public class CdkWorkshopStack : MyStack
    {
        internal CdkWorkshopStack(Construct scope, string id, INewStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here
        }
    }



}
