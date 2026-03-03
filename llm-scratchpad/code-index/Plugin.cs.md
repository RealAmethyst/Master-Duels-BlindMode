# Plugin.cs

Assembly-level attributes declare the mod identity for MelonLoader.

```
[assembly: MelonInfo(typeof(BlindMode.BlindModeMod), "Blind Mode", "3.0.0", "radsi & RealAmethyst")]
[assembly: MelonGame("Konami Digital Entertainment Co., Ltd.", "masterduel")]
```

## BlindModeMod (line 13)
MelonMod entry point. Registers `BaseClass` in Il2Cpp, creates a persistent root GameObject, and attaches `BaseClass` as a component.

### Methods
- `OnInitializeMelon()` -> void (line 15)
  - Calls `ClassInjector.RegisterTypeInIl2Cpp<BaseClass>()`
  - Creates `new GameObject(typeof(BaseClass).FullName)`
  - Calls `DontDestroyOnLoad` and sets `HideFlags.HideAndDontSave`
  - Adds `BaseClass` component to the GameObject
  - Logs success or catches and logs any exception
