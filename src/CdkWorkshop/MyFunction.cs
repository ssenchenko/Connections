using Amazon.CDK.AWS.Lambda;
using Connectors;
using Constructs;

namespace MyCDK;
public class MyFunction : CfnFunction
{
    internal MyFunction(Construct scope, string id, ICfnFunctionProps props) : base(scope, id, props)
    {
        Connections = new FunctionConnections(this);
    }

    public FunctionConnections Connections { get; }
}
