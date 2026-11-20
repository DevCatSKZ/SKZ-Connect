"""Check the Linux deliverable, including archives built from Windows checkouts."""
import importlib.util
from pathlib import Path
import tarfile
import tempfile
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location("linux_package", Path(__file__).resolve().parents[1] / "make-linux-tar.py")
PACKAGE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PACKAGE)


class LinuxPackageTests(unittest.TestCase):
    def test_windows_sources_produce_linux_text_and_preserve_executable(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            binary = root / "bin"
            scripts = root / "steamos"
            binary.mkdir()
            scripts.mkdir()
            executable = b"\x7fELF\x00\r\n\xff"
            (binary / "skzconnect").write_bytes(executable)
            for name in ("install.sh", "uninstall.sh"):
                (scripts / name).write_bytes(b"\xef\xbb\xbf#!/usr/bin/env bash\r\nset -eu\r\necho ready\r\n")
            (scripts / "skzconnect.service").write_bytes(b"[Service]\r\nExecStart=/var/lib/skzconnect/skzconnect\r\n")
            (root / "LICENSE").write_bytes(b"license\r\n")
            (root / "THIRD-PARTY-NOTICES.md").write_bytes(b"notices\r\n")
            getinfo = tarfile.TarFile.gettarinfo

            def with_owner(archive, *args, **kwargs):
                info = getinfo(archive, *args, **kwargs)
                info.uid, info.gid = 123, 456
                info.uname, info.gname = "fixture-owner", "fixture-group"
                return info

            with patch.object(tarfile.TarFile, "gettarinfo", with_owner):
                path = PACKAGE.build_archive(root / "out", "0.0.1", binary, scripts, root)
            with tarfile.open(path) as archive:
                self.assertEqual(len(archive.getmembers()), 6)
                for info in archive.getmembers():
                    data = archive.extractfile(info).read()
                    self.assertEqual(info.size, len(data))
                    self.assertEqual((info.uid, info.gid, info.uname, info.gname), (0, 0, "", ""))
                    if info.name == "skzconnect":
                        self.assertEqual(data, executable)
                    else:
                        self.assertNotIn(b"\r", data)
                        self.assertFalse(data.startswith(b"\xef\xbb\xbf"))
                    if info.name.endswith(".sh"):
                        self.assertTrue(data.startswith(b"#!/usr/bin/env bash\n"))
                    self.assertEqual(info.mode, 0o755 if info.name in ("skzconnect", "install.sh", "uninstall.sh") else 0o644)


if __name__ == "__main__":
    unittest.main()
