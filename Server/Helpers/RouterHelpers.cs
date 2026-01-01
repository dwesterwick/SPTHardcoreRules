using SPTarkov.Server.Core.Utils;

namespace DansDevTools.Helpers
{
    public static class RouterHelpers
    {
        public static ValueTask<string?> SerializeToValueTask(this JsonUtil jsonUtil, object obj)
        {
            string? json = jsonUtil.Serialize(obj);
            return new ValueTask<string?>(json);
        }
    }
}
