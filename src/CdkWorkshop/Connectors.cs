using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;
using Constructs;
using MyCDK;

using Seconds = System.Int32;

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

//----------------------------------------------------------------------------------------------------------------------


// ======================== AWS Blueprints implementation ========================

// namespace Aws.Blueprints.Connectors
public class FunctionToQueueSubscription : IPropsFunctionToQueueSubscription
{
    public int? BatchSize { get; set; }
    public bool? Enabled { get; set; }
    public string? FilterPolicy { get; set; }
    public Seconds? MaximumBatchingWindow { get; set; }
    public HashSet<IPermission>? Permissions { get; set; }
}

// namespace Aws.Blueprints.Connectors.Permissions.FunctionToQueueSubscription

public class ReceiveMessage : IPermission
{
    public CfnPolicyProps ToCfnPolicyProps(Construct[] resources)
    {
        throw new NotImplementedException();
    }
}

public class DeleteMessage : IPermission
{
    public CfnPolicyProps ToCfnPolicyProps(Construct[] resources)
    {
        throw new NotImplementedException();
    }
}

// ======================== Function et Queue ========================

public interface IFunctionToQueueSubscription
{
    int BatchSize { get; set; }
    bool Enabled { get; set; }
    string FilterPolicy { get; set; }
    Seconds MaximumBatchingWindow { get; set; }
}

public interface IPropsFunctionToQueueSubscription
{
    int? BatchSize { get; set; }
    bool? Enabled { get; set; }
    string? FilterPolicy { get; set; }
    Seconds? MaximumBatchingWindow { get; set; }
    HashSet<IPermission>? Permissions { get; set; }
}

public interface IEsmSubscriptionToQueue : IFunctionToQueueSubscription
{
    public CfnQueue EventSource { get; }
    public CfnFunction Function { get; }
}

// in case customers want to create a ESM resource manually
public class EventSourceMappingSubscribesToQueue : IEsmSubscriptionToQueue
{
    public int BatchSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string FilterPolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int MaximumBatchingWindow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    // and required fields to initialize in constructor
    public CfnQueue EventSource { get; } // original name EventSourceArn
    public CfnFunction Function { get; } // original name FunctionName

}

public class ConnectorFunctionSubscribesToQueue : IEsmSubscriptionToQueue
{
    private IPropsFunctionToQueueSubscription _props;
    private CfnFunction _function;
    private CfnQueue _queue;

    internal ConnectorFunctionSubscribesToQueue(
        Construct scope,
        CfnFunction lambda,
        CfnQueue queue,

        IPropsFunctionToQueueSubscription props // not null-able here to be explicit
    )
    {

        _props = props;
        _function = lambda;
        _queue = queue;
    }

    public int BatchSize { get => _props.BatchSize; set => throw new NotImplementedException(); }
    public bool Enabled { get => _props.Enabled; set => throw new NotImplementedException(); }
    public string FilterPolicy { get => _props.FilterPolicy; set => throw new NotImplementedException(); }
    public int MaximumBatchingWindow { get => _props.MaximumBatchingWindow; set => throw new NotImplementedException(); }
    public CfnQueue EventSource { get => _queue; }
    public CfnFunction Function { get => _function; }

}

public class ConnectorFunctionOperatesOnQueue { }

public class ConnectorFunctionUsesDeadLetterQueue { }

// ================================== UX ==================================

public class FunctionConnections
{
    private CfnFunction _lambda;
    private Construct _scope;
    private Blueprint _blueprints;

    internal FunctionConnections(Construct scope, CfnFunction lambda, Blueprint blueprints)
    {
        _lambda = lambda;
        _scope = scope;
        _blueprints = blueprints;
    }

    // public ConnectorFunctionOperatesOnQueue operateOnQueue(
    //     CfnQueue queue,
    //     IPropsFunctionOperatesOnQueue props)
    // { return new ConnectorFunctionOperatesOnQueue(); }

    // Queue can have method triggersLambda()
    // WIP: how to deal with them? how to make sure we don't allow duplicate connections, or should we bother at all?
    public ConnectorFunctionSubscribesToQueue subscribeToQueue(
        CfnQueue queue,
        HashSet<IPermission> permissions,
        IPropsFunctionToQueueSubscription? props = null)
    {
        var propsOrDefaults = props ?? _blueprints.FunctionToQueueSubscription(); // it's just a workaround in C#
        return new ConnectorFunctionSubscribesToQueue(_scope, _lambda, queue, permissions, propsOrDefaults);
    }
}

public class MyCfnFunction : CfnFunction
{

    private FunctionConnections _connections;
    public FunctionConnections Connections { get => _connections; }

    internal MyCfnFunction(Construct scope, string id, ICfnFunctionProps? props = null) : base(scope, id, props)
    {
        _connections = new FunctionConnections(scope, this, blueprints);
    }
}

public class Client
{
    public static void MainIsh(Construct scope)
    {
        CfnQueue queue = new(scope, "MyQueue");
        MyCfnFunction function = new(scope, "MyFunction");
        function.Connections.subscribeToQueue(queue, new HashSet<IPermission> { Aws.Blueprints.Connectors.Permissions.FunctionSubscribesToQueue.ReceiveMessage });
    }
}


// Something like
// permissions parameter is required as it can't have default value apart from small number of cases like DLQ
// function.Connections.SubscribeToQueue(queue, new HashSet<IPermission> { Aws.Blueprints.Connectors.Permissions.FunctionSubscribesToQueue.ReceiveMessage });
// and then customers can do something like
// function.Connections.SubscribeToQueue(queue, new HashSet<IPermission> { MyCompany.Blueprints.Connectors.Permissions.FunctionSubscribesToQueue.ReadMessagesOnly })
// the defaults parameter can default to null and the Blueprints can be injected through CfnFunction blueprints

// It'd be great to have:
// function.Connections.SubscribeToQueue(queue).AllowReceiveMessages().AllowDeleteMessages();
//
// BUT not possible (sic!)
// because the interface is not stable even between different versions of the Blueprint
// for example, in the beginning, in AWS defaults, without having detailed action item categories in SDF, we can have only
// function.Connections.SubscribeToQueue(queue).AllowRead().AllowWrite();
// and with the next version we'll need to switch to
// function.Connections.SubscribeToQueue(queue).AllowReceiveMessages().AllowDeleteMessages();
// after adding more categories to SDF
// customers might want to have their own methos here instead of overriding a limited number of methods suggested by AWS


