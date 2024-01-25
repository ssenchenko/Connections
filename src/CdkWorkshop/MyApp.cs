using Amazon.CDK;
using Blueprints;
using Connectors;
using Constructs;

namespace MyCDK;

public interface IAllBlueprints
{
    IConnectorBlueprints Connections { get; }
    IBlueprint LambdaFunction { get; }
}

public interface IAllPermissions
{
    IPermission FunctionToTable { get; }
    IPermission FunctionToQueue { get; }
}

public sealed class AwsDefaults : IAllBlueprints
{
    public IConnectorBlueprints Connections => new ConnectorBlueprintsAws();

    public IBlueprint LambdaFunction => throw new System.NotImplementedException();

    // singleton stuff
    private static AwsDefaults instance;
    private static object syncRoot = new System.Object();

    private AwsDefaults() { }
    public static AwsDefaults Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new AwsDefaults();
                }
            }

            return instance;
        }
    }
}

public class AwsPermissions : IAllPermissions
{
    public IPermission FunctionToTable => throw new System.NotImplementedException();
    public IPermission FunctionToQueue => throw new System.NotImplementedException();

    // also singleton
    private static AwsPermissions instance;
    private static object syncRoot = new System.Object();

    private AwsPermissions() { }
    public static AwsPermissions Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new AwsPermissions();
                }
            }

            return instance;
        }
    }
}

public interface IWithBlueprints
{
    IAllBlueprints Blueprints { get; }
    IAllPermissions Permissions { get; }
}

public interface IConstructWithBlueprints : IConstruct, IWithBlueprints
{
}

public interface INewAppProps : IWithBlueprints
{
    IAppProps OldAppProps { get; set; }
}

public class MyApp : App, IConstructWithBlueprints
{
    IAllBlueprints? _blueprints;
    public IAllBlueprints? Blueprints { get => _blueprints; }

    IAllPermissions? _permissions;
    public IAllPermissions? Permissions { get => _permissions; }

    public MyApp(INewAppProps? props) : base(props?.OldAppProps)
    {
        if (props?.Blueprints != null)
        {
            _blueprints = props!.Blueprints;
        }
        if (props?.Permissions != null)
        {
            _permissions = props!.Permissions;
        }
    }
}
