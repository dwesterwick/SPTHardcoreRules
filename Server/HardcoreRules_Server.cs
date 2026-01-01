using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace DansDevTools;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader + HardcoreRules_Server.LOAD_ORDER_OFFSET)]
public class HardcoreRules_Server
{
    public const int LOAD_ORDER_OFFSET = 1;

    public HardcoreRules_Server()
    {

    }

    public Task OnLoad()
    {
        return Task.CompletedTask;
    }
}
