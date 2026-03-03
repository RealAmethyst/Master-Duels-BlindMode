# UIHelpers.cs — Code Index

**File:** `UIHelpers.cs`
**Namespace:** `BlindMode`
**Imports (static):** `BlindMode.BaseClass`

---

## `public static class UIHelpers` (line 19)

Text-finding utilities and card info formatting. All methods are static.

---

### Methods (declaration order)

#### `public static string GetElement(string attrname)` — line 21
Converts the last character of an element/attribute sprite name (a digit) into its corresponding `BaseClass.Attribute` enum name string. Returns `""` if no match.
Note: uses `BaseClass.Attribute` explicitly to avoid conflict with `System.Attribute`.

---

#### `public static string GetRarity(string rarity)` — line 37
Converts the last character of a rarity sprite name (a digit) into its corresponding `Rarity` enum name string. Returns `""` if no match.
Parallel to `GetElement`; uses `Rarity` enum from `BaseClass`.

---

#### `public static List<(string, string)> FindListExtendedTextElement(GameObject obj, string objPath = "", bool useRegex = true)` — line 53
Collects all text components (`ExtendedTextMeshProUGUI`, `RubyTextGX`, `TMP_SubMeshUI`) on `obj` itself and all descendants. Returns a deduplicated list of `(parentName/objectName, text)` tuples.
Calls `FindInChildrenList` for the recursive portion. Accepts a path string fallback for `GameObject.Find`.

---

#### `public static string FindExtendedTextElement(GameObject obj, string objPath = "", bool useRegex = true)` — line 70
Single-result variant of `FindListExtendedTextElement`. Checks `obj`'s own components first, then delegates to `FindInChildren` for the first non-banned text found in descendants. Returns the text string or falls through to `FindInChildren`'s result.

---

#### `public static List<(string, string)> FindInChildrenList(GameObject obj, string objPath = "", bool useRegex = true)` — line 84
Recursively collects all text component values from direct children and their subtrees. Returns deduplicated `(parentName/objectName, text)` tuples.
Called by `FindListExtendedTextElement` after checking the root object itself.

---

#### `public static string FindInChildren(GameObject obj, string objPath = "", bool useRegex = true)` — line 127
Depth-first search returning the first non-banned text found among children. For each child, checks direct component, then `GetComponentInChildren` for each of the three text types. Returns `null` if nothing is found.
Unlike `FindInChildrenList`, stops at the first valid result (early return).

---

#### `public static bool IsBannedText(GameObject textElement, string text, bool useRegex)` — line 186
Returns `true` (text should be skipped) if: element is null/inactive, text is null/empty, text matches the whitespace-only regex (or punctuation-ending regex when outside a menu), or text is in the `bannedText` collection.
Note: regex pattern varies — inside a menu or on a "Button"-named object it only strips whitespace-only strings; outside menus it also strips strings ending in `.` or `!`.

---

#### `public static string GetSelectionPosition(SelectionButton button)` — line 193
Counts active `SelectionButton` siblings under the same parent and returns a `, {index} of {total}` position string. Returns `null` if only one button exists or if anything throws. Used to append positional context to button announcements.

---

#### `public static void GetUITextElements()` — line 225
Populates `currenElement` (and sometimes `textToCopy`) with card/item data for the current context. Has special cases:
- `Menus.DuelPass`: sets `textToCopy` with grade and time-left from fixed UI paths.
- `Menus.DECK` "Create card" dialog: reads "Unable" state from dialog text.
- Item preview fallback (when `SnapContentManager == null` and not in DECK/SOLO/DUEL): reads from `ItemPreviewUI` path.
- Main branch: iterates a priority list of path prefixes (CardBrowser SnapContent page, DeckEdit, DeckBrowser, DuelClient CardInfo) and populates `currenElement.cardInfo` fields including element sprite lookup.

Note: `SnapContentManager` refers to `BaseClass.SnapContentManager` (the field), not the `Il2CppYgomSystem.UI.SnapContentManager` type — explicit qualification required.

---

#### `public static string FormatInfo()` — line 293
Converts `currenElement` into a human-readable multiline string for screen reader output. Returns `string.Empty` if `currenElement.Name` is blank.
Has three output formats:
- **Default** (no card detail context): Name + Description only.
- **Card detail** (`SnapContentManager != null` or SOLO/DUEL/DECK menus): full card stats (ATK, DEF, Stars, Element, Pendulum, Attributes, SpellType, Owned, Description). Includes face-down status during duels.
- **Shop** (`Menus.SHOP`): Name, Category (from Description), TimeLeft, Price.
Filters out null/whitespace entries before joining with newlines.
