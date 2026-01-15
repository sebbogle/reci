using Microsoft.JSInterop;

namespace Reci.Core;

public class WakeLockSentinel
{
    public required int Id { get; init; }

    public required IJSObjectReference JsObjectReference { get; init; }
}
