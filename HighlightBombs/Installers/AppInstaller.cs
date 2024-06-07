using Zenject;

namespace HighlightBombs.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (Outline.outlineMaterialSource == null)
            {
                Container.BindInterfacesTo<QuickOutlineMaterialLoader>().AsSingle();
            }
        }
    }
}
