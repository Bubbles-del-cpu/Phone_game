import argparse,json,os,struct,subprocess,tempfile,zipfile
from pathlib import Path
TARGETS={'macOS':('StandaloneOSX',2),'Windows':('StandaloneWindows64',19),'Linux':('StandaloneLinux64',24),'Android':('Android',13)}
def verify(path,label):
 target,target_id=TARGETS[label]
 extractor=os.environ.get('UNITY_WEBEXTRACT','/Applications/Unity/Hub/Editor/6000.2.8f1/Unity.app/Contents/Tools/WebExtract')
 with zipfile.ZipFile(path) as z:
  names=z.namelist()
  settings=[n for n in names if n.endswith('/aa/settings.json')]
  assert len(settings)==1, 'Missing or duplicate Addressables settings'
  root=settings[0][:-len('settings.json')]
  data=json.loads(z.read(settings[0]))
  assert data['m_buildTarget']==target, f"{label} contains {data['m_buildTarget']} Addressables, expected {target}"
  cat=z.read(root+'catalog.bin')
  assert target.encode() in cat or target.encode('utf-16-le') in cat, 'Catalog does not reference expected platform'
  for other,_ in TARGETS.values():
   if other!=target:assert other.encode() not in cat and other.encode('utf-16-le') not in cat, 'Foreign platform in catalog: '+other
  bundles=[n for n in names if n.startswith(root) and n.endswith('.bundle')]
  assert bundles and any(n.endswith('/localization-locales_assets_all.bundle') for n in bundles)
  count=0
  with tempfile.TemporaryDirectory(prefix='nts-bundle-audit-') as tmp:
   for index,name in enumerate(bundles):
    assert name.startswith(root+target+'/'), 'Foreign bundle directory: '+name
    f=Path(tmp)/f'{index}.bundle';f.write_bytes(z.read(name))
    subprocess.run([extractor,str(f)],check=True,stdout=subprocess.DEVNULL)
    serialized=[p for p in Path(str(f)+'_data').iterdir() if p.name.startswith('CAB-') and not p.name.endswith(('.resS','.resource'))]
    assert serialized, 'No serialized data in '+name
    for p in serialized:
     header=p.read_bytes()[:256]
     version=struct.unpack('>I',header[8:12])[0]
     assert version==22, 'Unsupported serialized format: '+str(version)
     assert header[16] in (0,1), 'Invalid endian marker'
     end=header.index(0,48)
     actual=struct.unpack(('>' if header[16] else '<')+'i',header[end+1:end+5])[0]
     assert actual==target_id, f'{name}: embedded target {actual}, expected {target_id}'
     count+=1
  return {'platform':label,'addressables_target':target,'bundles':len(bundles),'serialized_files':count,'status':'PASS'}
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('archive',type=Path);p.add_argument('platform',choices=TARGETS);a=p.parse_args()
 print(json.dumps(verify(a.archive,a.platform)))
