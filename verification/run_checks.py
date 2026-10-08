"""Run local checks with .NET SDK 9.0.300; does not need Acumatica DLLs."""
from pathlib import Path
import json
import os
import shutil
import subprocess
import sys

here = Path(__file__).resolve().parent
dotnet_root = Path(os.environ.get('ProgramFiles', 'C:/Program Files')) / 'dotnet'
roslyn = dotnet_root/'sdk/9.0.300/Roslyn/bincore'
refs = dotnet_root/'packs/Microsoft.NETCore.App.Ref/9.0.5/ref/net9.0'
subprocess.run(['dotnet', str(roslyn/'csc.dll'), '/nologo', '/target:exe',
    '/out:'+str(here/'CheckSyntax.dll'), *['/r:'+str(p) for p in refs.glob('*.dll')],
    '/r:'+str(roslyn/'Microsoft.CodeAnalysis.dll'),
    '/r:'+str(roslyn/'Microsoft.CodeAnalysis.CSharp.dll'), str(here/'CheckSyntax.cs')], check=True)
for name in ('Microsoft.CodeAnalysis.dll', 'Microsoft.CodeAnalysis.CSharp.dll'):
    shutil.copy2(roslyn/name, here/name)
(here/'CheckSyntax.runtimeconfig.json').write_text(json.dumps({'runtimeOptions': {
    'tfm':'net9.0', 'framework': {'name':'Microsoft.NETCore.App', 'version':'9.0.5'}}}))
subprocess.run(['dotnet', str(here/'CheckSyntax.dll'), str(here.parent/'code')], check=True)
subprocess.run([sys.executable, str(here/'check_structure.py')], check=True)
