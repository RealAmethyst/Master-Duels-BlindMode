# UIHelpers.cs

Static utility class for finding and formatting UI text from Unity game objects.

## UIHelpers (line 19)
Static class. All methods operate on game object hierarchies, searching for text components
(`ExtendedTextMeshProUGUI`, `RubyTextGX`, `TMP_SubMeshUI`) across three game-specific text types.

### Methods

- `StripTags(string text) -> string` (line 21)
  // Removes XML/HTML tags via regex. Used to clean TMP rich text before speaking.

- `ParseEnumFromLastChar<T>(string text) -> string` (line 24)
  // Private. Parses the last character of a string as an integer and maps it to an enum name.
  // Used for extracting attribute/rarity values from sprite/element names that encode the value as a trailing digit.

- `GetElement(string attrname) -> string` (line 31)
  // Wrapper around ParseEnumFromLastChar for `BaseClass.Attribute` enum. Input is typically a sprite name.

- `GetRarity(string rarity) -> string` (line 33)
  // Wrapper around ParseEnumFromLastChar for `Rarity` enum. Input is typically a sprite name.

- `FindListExtendedTextElement(GameObject obj, string objPath = "", bool useRegex = true) -> List<(string, string)>` (line 35)
  // Checks the given object itself AND recursively searches all children. Returns a list of (path, text) tuples.
  // The path key is "{parent.name}/{component.name}" for identifying which element the text came from.
  // Deduplicates results with Distinct(). Falls back to GameObject.Find(objPath) if obj is null.

- `FindExtendedTextElement(GameObject obj, string objPath = "", bool useRegex = true) -> string` (line 52)
  // Single-result variant: checks the given object itself, then delegates to FindInChildren for child traversal.
  // Returns the first non-banned text found, or null if none.

- `FindInChildrenList(GameObject obj, string objPath = "", bool useRegex = true) -> List<(string, string)>` (line 66)
  // Iterates direct children only (one level), collecting all text components.
  // Recurses into grandchildren if a child has children of its own, via recursive self-call.
  // Returns (path, text) tuples. Used by FindListExtendedTextElement to build the full list.

- `FindInChildren(GameObject obj, string objPath = "", bool useRegex = true) -> string` (line 109)
  // Single-result variant of FindInChildrenList. Iterates direct children and uses GetComponentInChildren
  // (deep search) on each, returning the first non-banned text found. Returns null if none found.

- `IsBannedText(GameObject textElement, string text, bool useRegex) -> bool` (line 134)
  // Returns true (banned/skip) if: element is null, text is empty, object is inactive, text matches
  // the banned regex, or text is in the static `bannedText` set (from BaseClass).
  // The regex pattern varies: outside menus (currentMenu == NONE) and non-Button elements, also bans
  // strings ending with `.` or `!` (treats them as placeholder/filler text).

- `GetSelectionPosition(SelectionButton button) -> string` (line 141)
  // Returns a position string like ", 2 of 5" by counting active sibling SelectionButton components.
  // Returns null if the button has no parent, is the only active button, or an exception occurs.
  // Used to append positional context to spoken button text.

- `GetUITextElements() -> void` (line 173)
  // Populates `currentElement` (BaseClass.PreviewElement) with card/item data from the current UI.
  // Dispatches by `currentMenu`: handles DuelPass and DECK special cases first, then item preview
  // (when SnapContentManager is null and not in DECK/SOLO/DUEL), then falls through to a prioritized
  // list of card detail panel paths (CardBrowser, DeckEdit, DeckBrowser, DuelClient) using path conditions.
  // Note: `SnapContentManager.currentPage % 3` computes which clone index to use in the CardBrowser path.

- `FormatInfo() -> string` (line 241)
  // Builds a human-readable speech string from `currentElement` fields.
  // Output varies by context: card detail view (SOLO/DUEL/DECK or SnapContentManager active) includes
  // ATK/DEF/stars/element/etc.; SHOP context shows category/time left/price; default shows name + description.
  // Filters null/whitespace entries and joins with newlines.
  // Note: In DECK context, `Attributes` has surrounding brackets stripped via `[1..^1]` slice.
