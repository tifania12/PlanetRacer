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
console.log(`대상: ${base}`);

function get(url) {
  return new Promise((resolve, reject) => {
    https.get(url, { headers: { 'Accept-Encoding': 'br', 'User-Agent': 'deploy-check' } }, res => {
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => resolve({ status: res.statusCode, headers: res.headers, body: Buffer.concat(chunks) }));
    }).on('error', reject);
  });
}

// 2026-09-14: 파일 이름을 하드코딩하지 않는다. nameFilesAsHashes 를 켠 뒤로
// Build/ 안의 이름이 매 빌드마다 바뀌기 때문에, index.html 을 먼저 받아서
// 거기 적힌 실제 파일 이름을 뽑아 쓴다. (이름을 고정해 두면 캐시에 남은 옛날 파일을
// 확인하고 "성공"이라고 말하게 된다 — 오늘 그것 때문에 세 번을 헛돌았다.)
async function resolveTargets() {
  const r = await get(base + '/');
  if (r.status !== 200) throw new Error(`index.html http ${r.status}`);
  // index.html은 Build/*.br 처럼 우리가 직접 Content-Encoding을 붙인 게 아니라,
  // Cloudflare가 요청 헤더(Accept-Encoding: br)를 보고 즉석에서 압축해서 내려준다.
  // 여기서도 그걸 안 풀고 그대로 문자열로 바꾸면 바이너리가 되어 정규식이 하나도 안 걸린다
  // (2026-09-14에 이것 때문에 빌드는 성공했는데 배포 확인만 계속 떨어졌다).
  let htmlBuf = r.body;
  const enc = (r.headers['content-encoding'] || '').trim();
  if (enc === 'br') htmlBuf = zlib.brotliDecompressSync(htmlBuf);
  else if (enc === 'gzip') htmlBuf = zlib.gunzipSync(htmlBuf);
  const html = htmlBuf.toString('utf8');
  const names = [...html.matchAll(/buildUrl \+ "\/([^"]+)"/g)].map(m => m[1]);
  if (names.length === 0) throw new Error('index.html 에서 Build 파일 이름을 못 찾았다');

  const kindOf = n =>
    n.endsWith('.wasm.br') ? 'wasm' :
    n.endsWith('.data.br') ? 'data' : 'js';

  const list = names.map(n => ({ path: '/Build/' + n, kind: kindOf(n) }));
  list.push({ path: '/', kind: 'html' });
  console.log('index.html 이 가리키는 파일: ' + names.join(', '));
  return list;
}

(async () => {
  let bad = 0;
  let targets;
  try { targets = await resolveTargets(); }
  catch (e) { console.log('X  대상 목록을 못 만들었다: ' + e.message); process.exit(1); }
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
