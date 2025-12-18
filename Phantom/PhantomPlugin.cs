using MelonLoader;
using Phantom;

[assembly: MelonGame("Landfall Games", "Knightfall")]
[assembly: MelonInfo(
    typeof(PhantomPlugin),
    PhantomInfo.Name,
    PhantomInfo.Version,
    PhantomInfo.Author
)]
[assembly: MelonColor(1, 255, 102, 99)]

namespace Phantom;

public class PhantomPlugin : MelonPlugin
{
    // ReSharper disable once NullableWarningSuppressionIsUsed
    public MelonLogger.Instance Logger = null!;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("Initializing Phantom Plugin");
    }
}
