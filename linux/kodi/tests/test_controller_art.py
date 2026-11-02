import pathlib
import struct
import unittest

import kodi_stubs
from resources.lib import common


class ControllerArtTests(unittest.TestCase):
    def test_all_model_variants_and_offline_icons_are_packaged(self):
        root = pathlib.Path(__file__).resolve().parents[3]
        source = root / 'src/Switch2Pro.Bridge/Assets/Controllers'
        target = root / 'linux/kodi/service.skzconnect/resources/icons'
        self.assertEqual(len(list(source.glob('*.png'))), 28)
        for image in source.glob('*.png'):
            for name in (image.name, image.stem + '_off.png'):
                data = (target / name).read_bytes()
                self.assertEqual(data[:8], b'\x89PNG\r\n\x1a\n', name)
                self.assertEqual(struct.unpack('>IIBB', data[16:26]), (128, 128, 8, 6), name)

    def test_models_and_generations_have_distinct_images(self):
        for a, b in [('Pro1', 'Pro2'), ('JoyCon1Left', 'JoyCon2Left'), ('DualShock4', 'DualSense')]:
            self.assertNotEqual(common.kind_icon(a), common.kind_icon(b))
        self.assertTrue(common.kind_icon('N64Controller').endswith('n64.png'))
        self.assertTrue(common.player_icon({'devices': [{'kind': 'JoyCon1Left'}, {'kind': 'JoyCon1Right'}]}).endswith('jc1_paar.png'))
        self.assertTrue(common.player_icon({'devices': [{'kind': 'JoyCon2Left', 'inGrip': True}, {'kind': 'JoyCon2Right', 'inGrip': True}]}).endswith('jc2_grip.png'))
