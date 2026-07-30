import re
import json
import os

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
EPISODE_FILES = [
    os.path.join(BASE_DIR, "Episode 23.asset"),
    os.path.join(BASE_DIR, "Episode 24.asset"),
    os.path.join(BASE_DIR, "Episode 24.5.asset"),
]
TRANSLATIONS_FILE = os.path.join(BASE_DIR, "strings_to_translate.json")


def unicode_escape(text):
    result = []
    for ch in text:
        cp = ord(ch)
        if cp > 127:
            result.append(f'\\u{cp:04X}')
        elif ch == '"':
            result.append('\\"')
        elif ch == '\\':
            result.append('\\\\')
        else:
            result.append(ch)
    return '"' + ''.join(result) + '"'


def decode_escapes(s):
    """Decode \\uXXXX and \\UXXXXXXXX sequences (YAML stores them literally)."""
    s = re.sub(r'\\U([0-9A-Fa-f]{8})', lambda m: chr(int(m.group(1), 16)), s)
    s = re.sub(r'\\u([0-9A-Fa-f]{4})', lambda m: chr(int(m.group(1), 16)), s)
    return s


def patch_content(content, translations):
    pattern = re.compile(
        r'([ \t]+)- languageEnum: 0\r?\n'
        r'\1  LanguageGenericType: ([^\n\r{][^\n\r]*)\r?\n'
        r'\1- languageEnum: 1\r?\n'
        r'\1  LanguageGenericType: (?:Choice \d+)?\r?\n'
    )

    matches = list(pattern.finditer(content))
    hits = 0
    misses = []

    # Collect substitutions in reverse order to preserve offsets
    subs = []
    for m in matches:
        indent = m.group(1)
        english_raw = m.group(2)
        english_key = decode_escapes(english_raw.strip().strip('"'))
        if not english_key:
            continue
        japanese = translations.get(english_key, '')
        if not japanese:
            misses.append(english_key)
            continue
        jp_encoded = unicode_escape(japanese)
        new_str = (
            f"{indent}- languageEnum: 0\n"
            f"{indent}  LanguageGenericType: {english_raw}\n"
            f"{indent}- languageEnum: 1\n"
            f"{indent}  LanguageGenericType: {jp_encoded}\n"
        )
        subs.append((m.start(), m.end(), new_str))
        hits += 1

    # Apply in reverse
    result = content
    for start, end, new_str in sorted(subs, reverse=True):
        result = result[:start] + new_str + result[end:]

    return result, hits, misses


def main():
    with open(TRANSLATIONS_FILE, encoding='utf-8') as f:
        translations = json.load(f)
    print(f"Loaded {len(translations)} translations")

    for filepath in EPISODE_FILES:
        label = os.path.basename(filepath)
        if not os.path.exists(filepath):
            print(f"Missing: {label}")
            continue
        with open(filepath, encoding='utf-8') as f:
            content = f.read()
        patched, hits, misses = patch_content(content, translations)
        with open(filepath, 'w', encoding='utf-8', newline='') as f:
            f.write(patched)
        print(f"{label}: {hits} translated, {len(misses)} missed")
        if misses:
            for m in misses[:5]:
                print(f"  MISS: {m!r}")

    print("Done.")


if __name__ == "__main__":
    main()
