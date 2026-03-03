# llm-docs

Reference documentation for the BlindMode mod, gathered during the refactoring process.

## Files

- **game-screens.md** - Complete mapping of game screens, ViewControllers, dialog types, UI hierarchy paths, EOM element labels, named buttons, and duel-specific UI. This is the primary reference for understanding what the mod intercepts and announces.
- **modding-resources.md** - External resources for MelonLoader/Il2Cpp/Harmony modding, links to other Master Duel mods, and analysis of `dump.cs.txt` (the de facto API reference).

## Quick Reference

- To find a game type's API: `grep -n "ClassName" dump.cs.txt`
- To find what ViewController a screen uses: see game-screens.md "Known ViewControllers" section
- To find what button name maps to what announcement: see game-screens.md "Named Buttons" section
- MelonLoader Il2Cpp constraints: see modding-resources.md section 1
