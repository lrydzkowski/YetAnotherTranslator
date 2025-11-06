using System.Runtime.CompilerServices;
using Argon;

namespace YetAnotherTranslator.Tests.Integration.Helpers;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();

        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.DontScrubGuids();

        VerifierSettings.AddExtraSettings(
            settings =>
            {
                settings.DefaultValueHandling = DefaultValueHandling.Include;
                settings.Formatting = Formatting.Indented;
            }
        );
    }
}
