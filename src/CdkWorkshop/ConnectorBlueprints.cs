using Amazon.CDK.AWS.IAM;
using Connectors;
using Constructs;

namespace Blueprints;

public interface IBlueprint
{
}

public abstract class FunctionToTableBlueprint : IBlueprint, IWithPermission
{
    protected IPermission _permission;
    public IPermission Permission { get => _permission; set => _permission = value; }
}

public abstract class FunctionToQueueBlueprint : IBlueprint, IWithPermission
{
    protected IPermission _permission;
    public IPermission Permission { get => _permission; set => _permission = value; }
}

public class FunctionToTableBlueprintAws : FunctionToTableBlueprint
{
    internal FunctionToTableBlueprintAws() : base()
    {
        _permission = new FunctionToTablePermissionAws();
    }
}

public class FunctionToQueueBlueprintAws : FunctionToQueueBlueprint
{
    internal FunctionToQueueBlueprintAws() : base()
    {
        _permission = new FunctionToQueuePermissionAws();
    }
}

class FunctionToTablePermissionAws : IPermission
{
    public CfnPolicyProps ToCfnPolicyProps(Construct[] resources)
    {
        throw new System.NotImplementedException();
    }
}

class FunctionToQueuePermissionAws : IPermission
{
    public CfnPolicyProps ToCfnPolicyProps(Construct[] resources)
    {
        throw new System.NotImplementedException();
    }
}


public interface IConnectorBlueprints
{
    FunctionToTableBlueprint FunctionToTable { get; }
    FunctionToQueueBlueprint FunctionToQueue { get; }
}

public class ConnectorBlueprintsAws : IConnectorBlueprints
{
    FunctionToTableBlueprintAws _functionToTable = new FunctionToTableBlueprintAws();
    FunctionToQueueBlueprintAws _functionToQueue = new FunctionToQueueBlueprintAws();

    public FunctionToTableBlueprint FunctionToTable => _functionToTable;

    public FunctionToQueueBlueprint FunctionToQueue => _functionToQueue;
}
