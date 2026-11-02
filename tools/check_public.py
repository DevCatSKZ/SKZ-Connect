"""Reject private workspace files and common credential/device leaks before publication.

This is a targeted check, not a replacement for reviewing a release diff.
Only paths and line numbers are reported; matching values are never printed.
"""
import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
FORBIDDEN = {'agents.md', 'handover.md', 'verlauf.md', 'uebergabe-switch-identitaet.md', 'signing.props'}
SUFFIXES = {'.jks', '.keystore', '.pfx', '.p12', '.skzpair', '.apk', '.aab'}
RULES = {
    'credential': re.compile(rb'(?:gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{30,}|AKIA[0-9A-Z]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'),
    'workspace path': re.compile(rb'[A-Z]:[\\/](?:Users|Coding)[\\/]', re.I),
    'wireless adb device': re.compile(rb'adb-[A-Za-z0-9]{8,}-[A-Za-z0-9]+\._adb-tls-connect\._tcp'),
}


def main():
    names = subprocess.check_output(['git', 'ls-files', '--cached', '--others', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
    errors = []
    for name in filter(None, names):
        path = ROOT / name
        if not path.is_file():
            continue
        if path.name.lower() in FORBIDDEN or path.suffix.lower() in SUFFIXES:
            errors.append(name + ': private file type')
            continue
        data = path.read_bytes()
        if b'\0' in data[:8192]:
            continue
        for label, pattern in RULES.items():
            for match in pattern.finditer(data):
                line = data[:match.start()].count(b'\n') + 1
                errors.append(f'{name}:{line}: {label}')
    if errors:
        print('\n'.join(errors))
        return 1
    print('Public source check passed: no private workspace files or matching credentials/device identifiers.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
