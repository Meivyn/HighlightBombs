using IPA;
using System.Collections;
using HarmonyLib;
using IPA.Loader;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace HighlightBombs
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private readonly PluginMetadata _metadata;
        private readonly Harmony _harmony;

        internal static Plugin Instance { get; private set; } = null!;
        internal static IPALogger Log { get; private set; } = null!;

        [Init]
        public Plugin(IPALogger logger, PluginMetadata metadata)
        {
            Instance = this;
            Log = logger;
            _metadata = metadata;
            _harmony = new Harmony("com.meivyn.HighlightBombs");
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(_metadata.Assembly);
            SharedCoroutineStarter.instance.StartCoroutine(LoadQuickOutlineMaterials());
        }

        public IEnumerator LoadQuickOutlineMaterials()
        {
            var quickOutlineBundleRequest = AssetBundle.LoadFromStreamAsync(_metadata.Assembly.GetManifestResourceStream("HighlightBombs.QuickOutline.Resources.outlineBundle"));
            yield return quickOutlineBundleRequest;
            var quickOutlineBundle = quickOutlineBundleRequest.assetBundle;
            if (quickOutlineBundle == null)
            {
                Log.Error("Failed To load QuickOutline Bundle");
                yield break;
            }
            var materialRequest = quickOutlineBundle.LoadAssetAsync<Material>("Outline");
            yield return materialRequest;
            Outline.outlineMaterialSource = materialRequest.asset as Material;
            Log.Debug("Loaded QuickOutline Material Asset");
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
