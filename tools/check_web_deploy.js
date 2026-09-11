// 배포된 WebGL이 브라우저에서 실제로 열리는지 확인한다.
// 브라우저가 하는 일을 그대로 흉내 낸다: br 로 받아서 딱 한 번 푼 뒤,
// 나온 것이 진짜 자바스크립트/wasm 인지 본다.
//
// 헤더만 보면 놓치는 것이 있다. 2026-09-11에 Content-Encoding 이 "br, br" 로 나가
// 브라우저가 두 번 풀려다 죽은 적이 있다. 그때 헤더 200 확인만으로는 못 잡았다.
//
// 쓰는 법: node tools/check_web_deploy.js [주소]

const https = require('https');
const zlib = require('zlib');

const base = process.argv[2] || 'https://planetracer-daz.pages.dev';

function get(url) {
  return new Promise((resolve, reject) => {
    https.get(url, { headers: { 'Accept-Encoding': 'br', 'User-Agent': 'deploy-check' } }, res => {
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => resolve({ status: res.statusCode, headers: res.headers, body: Buffer.concat(chunks) }));
    }).on('error', reject);
  });
}

const targets = [
  { path: '/Build/PlanetRacer.framework.js.br', kind: 'js' },
  { path: '/Build/PlanetRacer.wasm.br',         kind: 'wasm' },
  { path: '/Build/PlanetRacer.data.br',         kind: 'data' },
  { path: '/Build/PlanetRacer.loader.js',       kind: 'js' },
  { path: '/',                                  kind: 'html' },
];

(async () => {
  let bad = 0;
  for (const t of targets) {
    const url = base + t.path;
    let r;
    try { r = await get(url); } catch (e) { console.log(`X  ${t.path}  요청 실패: ${e.message}`); bad++; continue; }

    const enc = r.headers['content-encoding'] || '';
    if (r.status !== 200) { console.log(`X  ${t.path}  http ${r.status}`); bad++; continue; }

    // 브라우저가 하는 일: Content-Encoding 에 적힌 만큼만 푼다
    if (enc.split(',').length > 1) {
      console.log(`X  ${t.path}  Content-Encoding 이 "${enc}" 다. 하나여야 한다 — _headers 규칙이 겹쳤다`);
      bad++; continue;
    }

    let body = r.body;
    if (enc.trim() === 'br') {
      try { body = zlib.brotliDecompressSync(body); }
      catch (e) { console.log(`X  ${t.path}  br 로 못 푼다: ${e.message}`); bad++; continue; }
    }

    // 푼 결과가 제 모습인지 본다
    let ok = false, what = '';
    if (t.kind === 'wasm') {
      ok = body.length > 4 && body[0] === 0x00 && body[1] === 0x61 && body[2] === 0x73 && body[3] === 0x6d;
      what = ok ? 'wasm 매직 확인' : `wasm 이 아니다 (앞 4바이트 ${body.slice(0,4).toString('hex')})`;
    } else if (t.kind === 'js') {
      const head = body.slice(0, 200).toString('utf8');
      ok = /unityFramework|createUnityInstance|function|var /.test(head);
      what = ok ? '자바스크립트 확인' : `자바스크립트가 아니다 (${JSON.stringify(head.slice(0,60))})`;
    } else if (t.kind === 'html') {
      const head = body.slice(0, 400).toString('utf8');
      ok = /<canvas|unity|<!DOCTYPE/i.test(head);
      what = ok ? 'HTML 확인' : 'HTML 이 아니다';
    } else {
      ok = body.length > 1000;
      what = ok ? '데이터 확인' : '너무 작다';
    }

    console.log(`${ok ? 'O' : 'X'}  ${t.path}  enc=[${enc}]  받은 ${r.body.length} → 푼 ${body.length}  ${what}`);
    if (!ok) bad++;
  }

  console.log(bad === 0 ? '\n전부 통과. 브라우저에서 열린다.' : `\n${bad}개 실패.`);
  process.exit(bad === 0 ? 0 : 1);
})();
