# Current Status

## Branch
`claude-mod-cleanup` (based off `main`)

## Completed Prompts
- sanity-checks-setup.md
- information-gathering-and-checking.md
- code-directory-construction.md
- input-handling.md (skipped — game has built-in navigation, mod is a speech overlay)
- string-builder.md (skipped — only ~8% string building, not a string builder mod)
- low-level-cleanup.md

## Pending Prompts
- high-level-cleanup.md
- finalization.md

## Low-Level Cleanups Done
1. Fix SpeakText dedup: strip tags before comparing
2. Fix SpeakScreenHeader dedup: strip tags before comparing
3. Fix dialog re-announcement: strip (Clone) suffix from dialog key
4. Standardize TMP_SubMeshUI to use m_TextComponent consistently
5. Fix HistotyButton typo → HistoryButton
6. Add logging to bare catch blocks (7 files)
7. Remove unused Tolk.dll wrappers (~28 lines removed)
8. Extract FindSiblingText helper, removing goto
9. Extract parent/grandparent locals + merge Dismantle/Create logic
10. Simplify Title screen version reading with GetElement
11. Extract FormatDialogAnnouncement helper
12. Restructure FormatInfo resultList
13. Remove unused imports

## Scratchpad Files
- current_status.md (this file)
- code-index/ — method/class/field index with line numbers for all 8 source files (NOTE: line numbers are now stale after cleanups)

## Documentation Files
- llm-docs/CLAUDE.md — index of llm-docs
- llm-docs/game-screens.md — all ViewControllers, menus, dialogs, duel features
- llm-docs/modding-resources.md — dump.cs.txt usage, key namespaces, assembly deps
