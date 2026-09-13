// W-06: 배포된 WebGL이 모바일 LTE에서 10초 안에 뜨는지 확인한다.
// Unity 에디터도 브라우저 프로파일러도 없는 클라우드 세션이라, "실제로 켜서 재는" 대신
// 이미 배포된 파일들의 실제 압축 크기(check_web_deploy.js와 같은 대상)를 재고,
// 대표적인 모바일 대역폭 몇 가지로 나눠 다운로드 시간만 계산한다.
// 파싱·초기화(wasm 인스턴스화 등) 시간은 포함하지 않으므로 실제 체감은 이보다 더 걸린다 —
// "최소 이만큼은 걸린다"는 하한선으로 보고, 초과하면 그 자체로 이미 예산을 넘긴 것이다.
//
// 쓰는 법: node tools/measure_web_load.js [주소]

const https = require('https');

const base = process.argv[2] || 'https://planetracer-daz.pages.dev';
console.log(`대상: ${base}`);

// 실제 브라우저가 처음 화면을 띄우기까지 받아야 하는 것들.
// loader.js는 압축 없이 내려간다(_headers가 .br 규칙만 걸어 뒀다).
const targets = [
  { path: '/Build/PlanetRacer.loader.js', label: 'loader.js' },
  { path: '/Build/PlanetRacer.framework.js.br', label: 'framework.js (br)' },
  { path: '/Build/PlanetRacer.wasm.br', label: 'wasm (br)' },
  { path: '/Build/PlanetRacer.data.br', label: 'data (br)' },
  { path: '/', label: 'index.html' },
];

// Mbps 단위. 국내 LTE 실측 기준으로 자주 인용되는 구간을 대충 세 단계로 잡았다 —
// 정확한 수치가 목적이 아니라 "이 정도면 위험하다"는 감을 보려는 것.
const bandwidths = [
  { label: 'LTE 약함(지하철 등)', mbps: 3 },
  { label: 'LTE 보통', mbps: 8 },
  { label: 'LTE/5G 좋음', mbps: 25 },
];

const BUDGET_SECONDS = 10; // CLAUDE.md/backlog W-06: 모바일 LTE 10초 넘으면 에셋을 줄인다

function headLength(url) {
  return new Promise((resolve, reject) => {
    https.get(url, { headers: { 'Accept-Encoding': 'br', 'User-Agent': 'measure-web-load' } }, res => {
      res.resume(); // 몸통은 안 받고 헤더만 본다 — 대역폭 계산엔 실제 전송 바이트 수(Content-Length)만 필요
      if (res.statusCode !== 200) { reject(new Error(`http ${res.statusCode}`)); return; }
      resolve(Number(res.headers['content-length'] || 0));
    }).on('error', reject);
  });
}

(async () => {
  let total = 0;
  let failed = 0;
  const rows = [];
  for (const t of targets) {
    const url = base + t.path;
    try {
      const bytes = await headLength(url);
      total += bytes;
      rows.push({ ...t, bytes });
      console.log(`  ${t.label.padEnd(20)} ${(bytes / 1024 / 1024).toFixed(2)} MB`);
    } catch (e) {
      failed++;
      console.log(`  ${t.label.padEnd(20)} 실패: ${e.message}`);
    }
  }

  // 하나라도 못 받았으면 합계가 실제보다 작게 나와서 "예산 안쪽"으로 잘못 보일 수 있다.
  // 이 클라우드 세션에서는 아웃바운드 네트워크 정책상 *.pages.dev 자체가 막혀 있어(403,
  // CONNECT 거부) 매번 이 경로를 탄다 — GitHub Actions나 Tifania PC에서 돌리면 정상 통과한다.
  if (failed > 0) {
    console.log(`\n${failed}개 요청이 실패해 정확한 합계를 못 냈다. 아래 대역폭별 시간은 계산하지 않는다.`);
    console.log('이 환경(클라우드 세션)은 임의 외부 사이트로 나가는 아웃바운드가 정책상 막혀 있을 수 있다 — ' +
      'GitHub Actions(check_web_deploy.js가 이미 매번 성공)나 실제 PC/폰 브라우저에서 다시 실행해 볼 것.');
    process.exit(2);
  }

  console.log(`\n합계: ${(total / 1024 / 1024).toFixed(2)} MB (파싱·초기화 시간 미포함, 다운로드만)\n`);

  let worstOverBudget = null;
  for (const bw of bandwidths) {
    const seconds = total / (bw.mbps / 8 * 1024 * 1024);
    const over = seconds > BUDGET_SECONDS;
    if (over) worstOverBudget = bw;
    console.log(`  ${bw.label.padEnd(20)} ${bw.mbps}Mbps → 다운로드 약 ${seconds.toFixed(1)}초` +
      (over ? `  ⚠ ${BUDGET_SECONDS}초 예산 초과` : ''));
  }

  if (worstOverBudget) {
    console.log(`\n${worstOverBudget.label} 기준으로도 예산을 넘긴다 — 에셋을 줄이거나(텍스처 압축, 코드 스트리핑)` +
      ' 로딩 화면에서 진행률이라도 보여줘야 이탈이 덜하다. backlog W-06 계속 열어 둠.');
    process.exit(1);
  } else {
    console.log(`\n측정한 대역폭 구간 전부 ${BUDGET_SECONDS}초 예산 안쪽. 다만 이건 다운로드 시간뿐이라 실제 체감(파싱·` +
      '초기화 포함)은 이보다 길 수 있다 — 폰으로 직접 열어서 체크섬 삼아 볼 것.');
    process.exit(0);
  }
})();
