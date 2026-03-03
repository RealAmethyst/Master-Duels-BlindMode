# UIHelpers.cs — Code Index

**File:** `UIHelpers.cs`
**Namespace:** `BlindMode`
**Imports (static):** `BlindMode.BaseClass`

---

## `public static class UIHelpers`

Text-finding utilities and card info formatting. All methods are static.

---

### Methods (declaration order)

#### `internal static string StripTags(string text)`
Strips HTML/rich-text tags from text using regex `<[^>]+>`. Returns input unchanged if null/empty. Used across all files to replace inline `Regex.Replace` calls.

---

#### `private static string ParseEnumFromLastChar<T>(string text) where T : struct, Enum`
Generic helper that parses the last character as an int and returns the corresponding enum name. Used by GetElement and GetRarity.

---

#### `public static string GetElement(string attrname)`
Delegates to `ParseEnumFromLastChar<BaseClass.Attribute>`.

---

#### `public static string GetRarity(string rarity)`
Delegates to `ParseEnumFromLastChar<Rarity>`.

---

#### `public static List<(string, string)> FindListExtendedTextElement(...)`
Collects all text components on obj and descendants. Returns deduplicated `(path, text)` tuples.

---

#### `public static string FindExtendedTextElement(...)`
Single-result variant. Checks obj's components first, then delegates to `FindInChildren`.

---

#### `public static List<(string, string)> FindInChildrenList(...)`
Recursively collects text from children. Returns deduplicated tuples.

---

#### `public static string FindInChildren(...)`
Returns first non-banned text found among children via `GetComponentInChildren` for each text type. Simplified: no pre-declared variables, no redundant TryGetComponent.

---

#### `public static bool IsBannedText(...)`
Returns true if text should be skipped (null, inactive, empty, matches regex, in bannedText).

---

#### `public static string GetSelectionPosition(SelectionButton button)`
Returns `, {index} of {total}` position string for button among active siblings.

---

#### `public static void GetUITextElements()`
Populates `currentElement` with card/item data. Special cases for DuelPass, DECK create dialog, item preview, and card detail views.

---

#### `public static string FormatInfo()`
Converts `currentElement` to multiline string. Three formats: default, card detail, shop.
