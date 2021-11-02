using IPA;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using NiceMiss;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace HighlightBombs
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger)
        {
            Log = logger;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            var harmony = new Harmony("com.meivyn.HighlightBombs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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

        [OnExit]
        public void OnApplicationQuit()
        {
        }
    }
}
