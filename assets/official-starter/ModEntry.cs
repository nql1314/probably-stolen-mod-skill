using UnityEngine;

namespace Nicokobo.OfficialStarter
{
    // 开发模板：只记录生命周期，不修改存档或发放物品。
    public sealed class ModEntry : IMod
    {
        private string modId;

        public void Init(ModManifest manifest)
        {
            modId = manifest.ID;
        }

        public void OnEnable()
        {
            ModHook.OnModAssetsLoaded += AssetsLoaded;
            ModHook.OnGameLoadedInit += GameLoaded;
        }

        public void OnDisable()
        {
            ModHook.OnModAssetsLoaded -= AssetsLoaded;
            ModHook.OnGameLoadedInit -= GameLoaded;
        }

        private void AssetsLoaded(LoadedMod mod)
        {
            if (mod.Manifest.ID == modId)
                Debug.Log("[" + modId + "] Assets loaded");
        }

        private void GameLoaded()
        {
            Debug.Log("[" + modId + "] Run loaded");
        }
    }
}
