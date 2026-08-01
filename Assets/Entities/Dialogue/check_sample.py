import re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

files = ['Episode 23.asset', 'Episode 24.asset', 'Episode 24.5.asset']
pattern = re.compile(
    r'- languageEnum: 0\r?\n\s+LanguageGenericType: ([^\n\r{][^\n\r]*)\r?\n'
    r'\s+- languageEnum: 1\r?\n\s+LanguageGenericType: "([^"]+)"'
)

def decode(s):
    return re.sub(r'\\u([0-9A-Fa-f]{4})', lambda m: chr(int(m.group(1), 16)), s)

for fname in files:
    with open(fname, encoding='utf-8') as f:
        content = f.read()
    matches = pattern.findall(content)
    print(fname + ': ' + str(len(matches)) + ' pairs')
    for en, jp_raw in matches[:5]:
        print('  EN: ' + en)
        print('  JP: ' + decode(jp_raw))
    print()
