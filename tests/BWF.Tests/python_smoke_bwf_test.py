import unittest
import requests
import os
import json


class TestBWF(unittest.TestCase):

    def test_bwf_api_works(self):
        
        self.assertEqual(requests.get('http://localhost:5800/index.html').status_code, 200, 'BWF is not healthy')


if __name__ == '__main__':
    unittest.main()