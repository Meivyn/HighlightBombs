using HarmonyLib;
using HighlightBombs.Installers;
using IPA;
using IPA.Loader;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HighlightBombs
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private readonly PluginMetadata _metadata;
        private readonly Harmony _harmony;

        internal static IPALogger Log { get; private set; } = null!;

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata, Zenjector zenjector)
        {
            _metadata = metadata;
            _harmony = new Harmony("com.meivyn.BeatSaber.HighlightBombs");
            Log = logger;
            zenjector.UseLogger(logger);
            zenjector.UseMetadataBinder<Plugin>();
            zenjector.Install<AppInstaller>(Location.App);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(_metadata.Assembly);
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
