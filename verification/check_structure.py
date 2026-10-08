"""Structural checks only; integration scenarios are in ../README.md."""
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
for entry in json.loads((root/'stable-hashes.json').read_text(encoding='utf-8-sig')):
    assert hashlib.sha256(Path(entry['Path']).read_bytes()).hexdigest().upper() == entry['Hash'], entry['Path']
print('Stable files: SHA256 unchanged')

sql = (root/'sql/JEDispachtInventory.sql').read_text()
for name in ('JEINDispachtMaterial', 'JEDispachtIssue'):
    dac = (root/'code'/f'{name}.cs').read_text()
    fields = re.findall(r'public virtual \S+ (\w+) \{ get; set; \}', dac)
    table = sql.split(f'CREATE TABLE dbo.{name} (', 1)[1].split('\n    );', 1)[0]
    for field in fields:
        if field == 'AvailableQty':
            continue
        assert re.search(r'\b'+field+r'\s+', table, re.I), (name, field)
    assert 'CompanyID INT NOT NULL' in table
    assert 'PXProjection' not in dac
print('Persistent DAC fields match new SQL tables')

aspx = (root/'screens/JE401003.aspx').read_text()
aspx = re.sub(r'<%.*?%>', '', aspx, flags=re.S)
tree = ET.fromstring('<root xmlns:asp="asp" xmlns:px="px">'+aspx+'</root>')
ids = [node.attrib['ID'] for node in tree.iter() if 'ID' in node.attrib]
assert len(ids) == len(set(ids)), 'Duplicate ASPX control ID'
assert len([n for n in tree.iter() if n.attrib.get('Text') == 'Issues']) == 1
print('ASPX markup: valid XML structure, unique controls, Issues tab exists')
