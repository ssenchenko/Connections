using Amazon.CDK;
using Constructs;

namespace MyCDK;

public interface INewStackProps : IWithBlueprints
{
    IStackProps OldStackProps { get; set; }
}

public class NewStackProps : INewStackProps
{
    public IStackProps OldStackProps { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public IAllBlueprints Blueprints => throw new System.NotImplementedException();

    public IAllPermissions Permissions => throw new System.NotImplementedException();
}


public class MyStack : Stack, IConstructWithBlueprints
{
    IAllBlueprints? _blueprints;
    public IAllBlueprints? Blueprints { get => _blueprints; }

    IAllPermissions? _permissions;
    public IAllPermissions? Permissions { get => _permissions; }

    public MyStack(Construct scope, string id, INewStackProps props = null) : base(scope, id, props?.OldStackProps)
    {
        IAllBlueprints? defaultBlueprints = (scope is IConstructWithBlueprints) ? ((IConstructWithBlueprints)scope).Blueprints : null;
        IAllPermissions? defaultPermissions = (scope is IConstructWithBlueprints) ? ((IConstructWithBlueprints)scope).Permissions : null;
        _blueprints = props?.Blueprints ?? defaultBlueprints;
        _permissions = props?.Permissions ?? defaultPermissions;
    }
}
