using IPA;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace HighlightBombs
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private readonly Harmony _harmony;

        internal static IPALogger Log { get; private set; } = null!;

        [Init]
        public Plugin(IPALogger logger)
        {
            Log = logger;
            _harmony = new Harmony("com.meivyn.HighlightBombs");
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            SharedCoroutineStarter.instance.StartCoroutine(LoadQuickOutlineMaterials());
        }

        public IEnumerator LoadQuickOutlineMaterials()
        {
            var quickOutlineBundleRequest = AssetBundle.LoadFromStreamAsync(Assembly.GetExecutingAssembly().GetManifestResourceStream("HighlightBombs.QuickOutline.Resources.outlineBundle"));
            yield return quickOutlineBundleRequest;
            var quickOutlineBundle = quickOutlineBundleRequest.assetBundle;
            if (quickOutlineBundle == null)
            {
                Log.Error("Failed To load QuickOutline Bundle");
                yield break;
            }
            var fillMatRequest = quickOutlineBundle.LoadAssetAsync<Material>("OutlineFill");
            yield return fillMatRequest;
            Outline.outlineFillMaterialSource = fillMatRequest.asset as Material;
            var maskMatRequest = quickOutlineBundle.LoadAssetAsync<Material>("OutlineMask");
            yield return maskMatRequest;
            Outline.outlineMaskMaterialSource = maskMatRequest.asset as Material;
            Log.Debug("Loaded QuickOutline Material Assets");
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
