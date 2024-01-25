using System;
using System.Collections.Generic;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Constructs;
using MyCDK;

namespace Connectors;

public interface IConnector
{
}

public interface IPermission
{
    CfnPolicyProps ToCfnPolicyProps(Construct[] resources);
}

public interface IWithEnvVar
{
    string EnvVarName { get; }
}

public interface IWithResource
{
    Construct Resource { get; }
}

public interface IWithPermission
{
    IPermission Permission { get; }
}

public interface IFunctionToTableProps : IWithEnvVar, IWithPermission
{
    CfnRole LambdaExecutionRole { get; }
}

public interface IFunctionToQueueProps : IWithEnvVar, IWithPermission
{
    CfnRole LambdaExecutionRole { get; }
}

public interface IConnectorWithEnvVar : IConnector
{
    void SetEnvVariable(Construct receiver, Construct sender, string varName);
}

public interface IConnectorWithPolicy : IConnector
{
    CfnPolicy Policy { get; }
    CfnPolicy CreatePolicy(IPermission permissions, Construct[] resources);
    void AttachPolicy(CfnRole role, CfnPolicy policy);
}

public class FunctionToTableConnector : Construct, IConnectorWithEnvVar, IConnectorWithPolicy
{
    private IPermission? _permissions;
    private CfnPolicy _policy;
    public CfnPolicy Policy { get { return _policy; } }

    internal FunctionToTableConnector(
        Construct scope,
        CfnFunction lambda,
        CfnTable table,
        IFunctionToTableProps props
    ) : base(scope, "FunctionToTableConnector")
    {
        IAllPermissions defaultPermissions = (scope is IConstructWithBlueprints) ? ((IConstructWithBlueprints)scope).Permissions : null;
        _permissions = props.Permission ?? defaultPermissions?.FunctionToTable;
        // verify that _defaults and _permissions contain all required properties 
        SetEnvVariable(lambda, table, props.EnvVarName);
        _policy = CreatePolicy(_permissions, new[] { lambda });
        AttachPolicy(props.LambdaExecutionRole, _policy);
    }

    public CfnPolicy CreatePolicy(IPermission permission, Construct[] resources)
    {
        return new CfnPolicy(this, "Policy", permission.ToCfnPolicyProps(resources));
    }

    public void SetEnvVariable(Construct lambda, Construct table, string varName)
    {
        ((CfnFunction)lambda).Environment = new Dictionary<string, string>() {
                { varName, ((CfnTable)table).TableName }
            };
    }

    public void AttachPolicy(CfnRole role, CfnPolicy policy)
    {
    }
}


public class FunctionToQueueConnector : Construct, IConnectorWithEnvVar, IConnectorWithPolicy
{
    private IPermission? _permissions;
    private CfnPolicy _policy;
    public CfnPolicy Policy { get { return _policy; } }

    internal FunctionToQueueConnector(
        Construct scope,
        CfnFunction lambda,
        CfnQueue queue,
        IFunctionToQueueProps props
    ) : base(scope, "FunctionToQueueConnector")
    {
        IAllPermissions defaultPermissions = (scope is IConstructWithBlueprints) ? ((IConstructWithBlueprints)scope).Permissions : null;
        _permissions = props.Permission ?? defaultPermissions?.FunctionToQueue;
        // verify that _defaults and _permissions contain all required properties 
        SetEnvVariable(lambda, queue, props.EnvVarName);
        _policy = CreatePolicy(_permissions, new[] { lambda });
        AttachPolicy(props.LambdaExecutionRole, _policy);
    }

    public CfnPolicy CreatePolicy(IPermission permission, Construct[] resources)
    {
        return new CfnPolicy(this, "Policy", permission.ToCfnPolicyProps(resources));
    }

    public void SetEnvVariable(Construct lambda, Construct table, string varName)
    {
        ((CfnFunction)lambda).Environment = new Dictionary<string, string>() {
                { varName, ((CfnTable)table).TableName }
            };
    }

    public void AttachPolicy(CfnRole role, CfnPolicy policy)
    {
    }
}

public class FunctionConnections
{
    private CfnFunction _lambda;

    internal FunctionConnections(CfnFunction lambda)
    {
        _lambda = lambda;
    }

    public FunctionToTableConnector toTable(CfnTable table, IFunctionToTableProps props)
    {
        return new FunctionToTableConnector(_lambda.Stack, _lambda, table, props);
    }

    public FunctionToQueueConnector toQueue(CfnQueue queue, IFunctionToQueueProps props)
    {
        return new FunctionToQueueConnector(_lambda.Stack, _lambda, queue, props);
    }
}
