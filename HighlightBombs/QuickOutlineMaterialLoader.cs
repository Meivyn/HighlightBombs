using System.Collections;
using IPA.Loader;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;

namespace HighlightBombs
{
    internal class QuickOutlineMaterialLoader : IInitializable
    {
        private readonly ICoroutineStarter _coroutineStarter;
        private readonly PluginMetadata _metadata;

        public QuickOutlineMaterialLoader(ICoroutineStarter coroutineStarter, UBinder<Plugin, PluginMetadata> metadata)
        {
            _coroutineStarter = coroutineStarter;
            _metadata = metadata.Value;
        }

        public void Initialize()
        {
            _coroutineStarter.StartCoroutine(LoadQuickOutlineMaterialsCoroutine());
        }

        private IEnumerator LoadQuickOutlineMaterialsCoroutine()
        {
            var quickOutlineBundleRequest = AssetBundle.LoadFromStreamAsync(_metadata.Assembly.GetManifestResourceStream("HighlightBombs.QuickOutline.QuickOutline.AssetBundles.outline"));
            yield return quickOutlineBundleRequest;
            var quickOutlineBundle = quickOutlineBundleRequest.assetBundle;
            if (quickOutlineBundle == null)
            {
                Plugin.Log.Error("Failed To load QuickOutline Bundle");
                yield break;
            }
            var materialRequest = quickOutlineBundle.LoadAssetAsync<Material>("Outline");
            yield return materialRequest;
            Outline.outlineMaterialSource = (Material)materialRequest.asset;
            Plugin.Log.Debug("Loaded QuickOutline Material Asset");
            quickOutlineBundle.UnloadAsync(false);
        }
    }
}
