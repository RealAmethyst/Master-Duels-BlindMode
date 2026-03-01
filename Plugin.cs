using System;

using UnityEngine;

using MelonLoader;
using Il2CppInterop.Runtime.Injection;

[assembly: MelonInfo(typeof(BlindMode.BlindModeMod), "Blind Mode", "3.0.0", "radsi & RealAmethyst")]
[assembly: MelonGame("Konami Digital Entertainment Co., Ltd.", "masterduel")]

namespace BlindMode
{
    public class BlindModeMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<BaseClass>();
                var plugin = new GameObject(typeof(BaseClass).FullName);
                UnityEngine.Object.DontDestroyOnLoad(plugin);
                plugin.AddComponent<BaseClass>();
                plugin.hideFlags = HideFlags.HideAndDontSave;

                MelonLogger.Msg("Plugin has been loaded!");
            }
            catch (Exception e)
            {
                MelonLogger.Error("Error loading the plugin!");
                MelonLogger.Error(e.ToString());
            }
        }
    }
}
