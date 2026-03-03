// File: Plugin.cs
// Assembly attributes declare MelonLoader mod metadata and target game.
[assembly: MelonInfo(...)]  (line 8)
[assembly: MelonGame(...)]  (line 9)

namespace BlindMode
  class BlindModeMod : MelonMod
    // Entry point. Registers BaseClass with Il2Cpp, creates a persistent
    // GameObject, attaches BaseClass as a MonoBehaviour component, and
    // hides it from the hierarchy. Logs success or error to MelonLoader console.
    override void OnInitializeMelon() (line 15)
