# 이미지 요청 대기열 — GPT로 만들 것

Tifania가 ChatGPT Plus를 쓰고, **이미지 생성은 거기서 한다**(2026-09-15 결정).
밤 20~08시에 도는 이미지 세션이 PC의 ChatGPT 데스크탑 앱을 조작해 여기 쌓인 것을 뽑는다.
그러니 **이미지가 필요해지면 직접 만들려 하지 말고 여기에 프롬프트를 적어 둔다.**

받은 이미지는 `PlanetRacer/Assets/Resources/Art/` 아래로 들어간다 — `Assets/Art/`가 아니다.
`Resources`에 있어야 아트 확인 화면(`?art=1`)이 런타임에 긁어서 보여줄 수 있다.
배경을 빼야 하는 그림은 **처음부터 투명 PNG로 요청한다**(마젠타 키잉은 더 쓰지 않는다).
GPT는 프롬프트에 `real alpha channel` / `no background color`를 넣으면 진짜 RGBA PNG를 준다 — 2026-09-16 확인.
이미지 세션은 받은 파일이 정말 RGBA이고 네 모서리가 투명한지 검사한 뒤 넣는다.

## 적는 법

한 항목에 이만큼만 있으면 된다. 길게 쓰지 않는다.

    ### (파일명) 무엇에 쓰는 이미지인지
    - 크기: 512x512 (또는 필요한 비율)
    - 용도: 어디에 들어가는지 (예: 업그레이드 화면 도구 아이콘)
    - 프롬프트: (영문. GPT에 그대로 붙여 넣을 수 있게)
    - 참고: 색·톤에서 지켜야 할 것 (예: 쿼츠 행성 팔레트에 맞출 것)

만들어져서 프로젝트에 들어가면 `- [x]`로 바꾸고 파일 경로를 적는다.

## 톤 기준

행성 색은 `GemColors`가 정해 둔 값을 따른다(쿼츠는 흰빛, 루비는 붉은빛).

**톤이 두 갈래다. 2026-09-18에 Tifania가 컷신 7장을 보고 확정했다.**

- **작은 것 — 아이콘·펫·행성 구체.** UI 아이콘은 `BootstrapHudUgui.cs` 위쪽의 색 상수와
  어울리게(남색 바탕에 밝은 회백색 선). **사실적인 렌더링보다 단순한 형태가 낫다.**
  64x64에서도 실루엣이 읽혀야 하기 때문이다.
- **큰 것 — 컷신.** 전체 화면에 깔리는 자리라 **사실적으로 가도 된다.**
  기존 7장(오프닝·행성 도착 5·첫 우승)이 그 기준이다 —
  저폴리 결정 지형 + 실사에 가까운 차량·조명 + 하늘에 행성 실루엣.
  **컷신을 새로 뽑을 때는 그 7장과 나란히 놓고 튀지 않는지 본다.**

두 갈래가 섞여 있는 게 의도다. 작은 건 알아보는 게 목적이고, 큰 건 분위기가 목적이다.

## 대기 중

<!-- 여기에 추가 -->

**펫 아트 76장 (2026-09-19에 채움).** `docs/design/pet-gacha.md` 7절의 순서 그대로다.
위에서부터 뽑으면 된다 — 7등급 10장이 먼저 나오게 해 뒀다(뽑기 화면에 제일 크게 나온다).

**초월 10종 중 5장이 마젠타 잔상으로 막혀 있다 (2026-09-19 밤 세션).**
`drill-sovereign`(3946px) · `comet-racer`(136px) · `burst-phoenix`(60px) ·
`shard-weaver`(547px) · `ember-heart`(269px). 파일은 받아서 자리에 그대로 두었지만
`check_alpha.py`가 실패시켜서 커밋하지 않았다(`drill-sovereign`은 다른 세션의 `git add -A`에
휩쓸려 들어갔다가 e0120fa에서 추적을 뺐다).

**키잉 때문이 아니다.** 프롬프트의 `iridescent prismatic material`이 분홍·보라 픽셀을 만드는데,
`check_alpha.py`의 마젠타 검사가 그걸 `r>150 and b>150 and g<100`로 잡는다. 같은 프롬프트의
나머지 5장은 우연히 50px 문턱을 넘지 않아 통과했다 — 그림의 좋고 나쁨과는 무관하다.
**Tifania가 정할 일:** (1) 초월 프롬프트에서 `iridescent prismatic`을 덜 분홍인 말로 바꾸거나,
(2) 마젠타 검사를 순수 #FF00FF에 가깝게 좁히거나(지금은 보라 전체를 잡는다),
(3) 이 5장은 눈으로 보고 통과시키거나. 정해지기 전에는 밤 세션이 다시 뽑아도 같은 자리에서 막힌다.

**같은 일이 신화 날개족에서도 났다 (2026-09-20 새벽 세션).** `wing-03`(마젠타 123px) ·
`wing-04`(89px)가 같은 검사에서 막혔다. 초월이 아닌데도 막힌 이유는 날개족 프롬프트의
`rich jewel tones`가 보라·남색 픽셀을 만들고, 그걸 마젠타 검사(`r>150 and b>150 and g<100`)가
잡기 때문이다. 같은 프롬프트로 뽑은 `wing-02` · `wing-05~08` 다섯 장은 통과했다 —
**그림의 좋고 나쁨이 아니라 우연히 50px 문턱을 넘었는지의 문제다.** 두 파일은 자리에 그대로
두었고 커밋하지 않았다.

**광석족(ore)은 거의 전멸했다 (2026-09-20 새벽 세션).** 일곱 장 중 통과는 `ore-01` 하나뿐이다.
`ore-02`(4209px) · `ore-03`(2203px) · `ore-04`(574px) · `ore-05`(178px) · `ore-07`(1742px)이
마젠타 검사에 걸렸고, `ore-06`은 ChatGPT 앱이 "미리 보기" 상태로 멈춰 아예 내려받지 못했다.
광석족 프롬프트는 `a faceted crystal cluster` + `rich jewel tones`라서 모델이 거의 항상
보라·자수정 결정을 그린다 — 날개족처럼 운에 맡길 문제가 아니라 **이 계열은 구조적으로 막힌다.**
반면 **짐꾼족(haul)은 일곱 장 전부 통과했다**(따뜻한 갈색·남색 위주라 마젠타 검사에 안 걸린다).

**한줄로: 위 (2)번으로 정하면 한꺼번에 풀린다.** 마젠타 검사를 순수 #FF00FF 쪽으로 좁히면
초월 5장 · 날개 2장 · 광석 5장, 모두 열두 장이 같이 풀린다. 정해지기 전까지는 다시 뽑아도 같은 자리에서 막힌다.

**2026-09-20 새벽 세션이 끝난 시점에서, "대기 중"에 남은 열세 장은 전부 이 문제다.**
초월 5 · 날개 2 · 광석 6(`ore-06`은 앱이 "미리 보기"에서 멈춰 아직 한 번도 못 받은 것).
나머지는 **다 들어갔다** — 신화 바퀴 8 / 날개 8 / 광석 1 / 짐꾼 7, 전설 20장(4계열 x 5색),
골격 12장(2·3·4등급 x 4계열). 즉 **이 마젠타 검사 하나만 정하면 펫 아트는 끝난다.**
1~4등급 색 변종 48장은 골격이 다 찼으니 이제 `tools/recolor_pet.py`로 만들면 된다(P-18).

**2026-09-20 아침 세션에서 셋이 풀렸다.** 같은 프롬프트를 그대로 다시 뽑았더니
`comet-racer`(투명 67%) · `burst-phoenix`(63%) · `shard-weaver`(71%) 세 장이 마젠타 검사를
통과해 들어갔다. **초월 계열은 구조적으로 막힌 게 아니라 뽑기 나름이라는 뜻이다** —
`drill-sovereign`은 같은 자리에서 또 막혔다(이번엔 3247픽셀). 광석족은 아직 확인 못 했다 —
`ember-heart`부터는 ChatGPT 이미지 생성 한도에 걸려("14시간 후에 다시 시도하세요") 더 못 뽑았다.
남은 열 장: 초월 2(`drill-sovereign` · `ember-heart`) · 날개 2 · 광석 6.
**(2)번 결정은 여전히 유효하다** — 검사를 좁히면 남은 것도 한꺼번에 풀린다.


**신화 30종은 계열별로 프롬프트가 똑같다.** 바퀴 8장이 같은 문장 하나, 날개 8장이 같은 문장 하나다.
종 이름(톱니 순례자·궤도 방랑자…)이 프롬프트에 들어가 있지 않아서, 어느 파일이 어느 종이 되는지는
뽑는 순서가 정할 뿐이다. 그림 자체는 매번 다르게 나오니 못 쓸 것은 아니지만,
이름과 그림을 맞추려면 종마다 한 줄씩 더 적어야 한다.

1~4등급의 **색 변종 48장은 여기 없다.** 그림을 다시 뽑지 않고 아래 "골격" 항목을
`tools/recolor_pet.py`로 색만 바꿔 만든다(backlog P-18). 골격만 뽑으면 된다.

**2026-09-20 20시 세션 — "광석족은 구조적으로 막힌다"는 진단도 틀렸다.**
같은 프롬프트를 한 글자도 안 고치고 다시 뽑았더니 `ore-03`(투명 52%) · `ore-05`(51%)가
마젠타 검사를 통과해 들어갔고, 날개족 `wing-04`(55%)도 통과했다.
같은 자리에서 막힌 것은 `ore-02`(8542px) · `ore-04`(288px) · `ore-06`(3114px) ·
`wing-03`(14105px) · `drill-sovereign`(1294px) · `ember-heart`(58px)이다.
**계열 문제가 아니라 매번 다른 뽑기 운이다** — 같은 프롬프트로 광석족 네 장 중 두 장이 통과했고,
새벽에 4209px이던 `ore-02`가 8542px로 더 나빌지는 동안 2203px이던 `ore-03`은 통과했다.
`ore-06`은 이번에 처음으로 내려받기에 성공했다(앱의 "미리 보기" 멈춤은 재현되지 않았다).
**그래도 (2)번 결정이 가장 싸다.** `ember-heart`는 58픽셀 — 문턱(50)을 여덟 픽셀 넘겨 막혔다.
검사를 순수 #FF00FF 쪽으로 좁히면 이런 것부터 바로 풀린다.
남은 일곱 장: 초월 2(`drill-sovereign` · `ember-heart`) · 날개 1(`wing-03`) · 광석 4(`ore-02` · `ore-04` · `ore-06` · `ore-07`).

**2026-09-20 22시 세션 — 열두 장 뽑아 한 장만 통과했다.**
남은 일곱 장을 위에서부터 한 번씩 돌리고(7장) 다시 위에서부터 다섯 장을 더 돌렸다.
첫 바퀴 일곱 장은 **전부 마젠타 검사에 막혔다** — `drill-sovereign`(10259px) · `ember-heart`(673px) ·
`wing-03`(500px) · `ore-02`(233px) · `ore-04`(259px) · `ore-06`(8365px) · `ore-07`(8408px).
두 번째 바퀴에서 `wing-03`이 통과했고(투명 67%), 나머지 네 장은 또 막혔다
(`drill-sovereign` 118px · `ember-heart` 1684px · `ore-02` 367px · `ore-04` 24262px).
**12에 1 — 이제까지 중 가장 나쁜 비율이다.** 20시 세션은 12에 6이었다.
같은 프롬프트를 다시 돌리는 것만으로는 남은 여섯 장을 정리하기 어렵다는 뜻이다.
`drill-sovereign`은 118픽셀까지 내려갔다 — 문턱(50)을 68픽셀 넘겼을 뿐이다.
**(2)번 결정이 여전히 가장 싼 길이다.**
남은 여섯 장: 초월 2(`drill-sovereign` · `ember-heart`) · 광석 4(`ore-02` · `ore-04` · `ore-06` · `ore-07`).


**2026-09-21 00시 세션 — 열두 장 뽑아 두 장 통과.** 대기 중 여섯 장을 위에서부터 한 바퀴 돌리고(6장),
다시 위에서부터 여섯 장을 더 돌렸다. 1차에서 `ember-heart`(투명 51%), 2차 마지막에 `drill-sovereign`(53%)이 통과했다.
막힌 것: `ore-02`(5237px -> 7817px) · `ore-04`(137 -> 147) · `ore-06`(449 -> 201) · `ore-07`(4877 -> 209).
**초월 두 장이 다 풀려서 남은 것은 광석족 네 장뿐이다.** 광석족은 네 장 모두 두 번씩 막혔고,
수치가 오르내리기만 할 뿐 문턱(50) 아래로는 한 번도 안 내려갔다 — 가장 가까웠던 게 `ore-04` 137px이다.
**(2)번 결정이 여전히 남은 네 장을 푸는 가장 싼 길이다.**

**2026-09-21 02시 세션 — 열두 장 뽑아 0장 통과. 처음 있는 전멸이다.**
남은 네 장(`ore-02` · `ore-04` · `ore-06` · `ore-07`)을 위에서부터 세 바퀴 돌렸다.
마젠타 픽셀 수(1차 → 2차 → 3차): `ore-02` 2136 → 10072 → 1656 · `ore-04` 34258 → 2381 → 16341 ·
`ore-06` 13780 → 4788 → 27609 · `ore-07` 41702 → 32304 → 3536.
**열두 번 중 문턱(50)에 가까이 간 것이 한 번도 없다** — 가장 낮았던 게 1656픽셀로, 00시 세션의
최저치(137)보다도 열 배 이상 높다. 프롬프트는 장부 그대로, 한 글자도 안 고쳤다.
**광석족 누적 26연속 실패다**(20시 4뽑아 2통과 이후 22시 6·00시 8·이번 12 = 26번 전패).
**재생성 운으로 푸는 길은 사실상 닫혔다고 봐야 한다.** (2)번 결정(마젠타 검사를 순수 #FF00FF
쪽으로 좁히기)이 남은 네 장을 푸는 유일하게 현실적인 길이다 — 밤 세션을 더 돌려도
같은 자리에서 같은 이유로 막힌다.

**2026-09-21 04시 세션 — 여덟 장 뽑아 0장. 그리고 막힌 이유를 처음으로 측정했다.**
네 장(`ore-02` · `ore-04` · `ore-06` · `ore-07`)을 위에서부터 두 바퀴 돌렸다.
마젠타 픽셀 수(1차 → 2차): `ore-02` 9764 → 26241 · `ore-04` 29560 → 30418 ·
`ore-06` 9471 → 43466 · `ore-07` 15723 → 53363. **광석족 누적 34연속 실패다.**

**새로 안 것 — 이건 키잉 잔상이 아니다. 측정해서 확인했다.**
`tools/check_magenta_kind.py`(이번에 만듦)로 걸린 픽셀이 **어떤 색인지** 세어 봤다.

| 파일 | 걸린 픽셀 | 그중 #FF00FF 근처 | 그중 반투명(테두리) |
|---|---|---|---|
| `ore-02` | 26241 | **448 (1.7%)** | 168 (0.6%) |
| `ore-04` | 30418 | **99 (0.3%)** | 1707 (5.6%) |
| `ore-06` | 43466 | **43 (0.1%)** | 2100 (4.8%) |
| `ore-07` | 53363 | **53 (0.1%)** | 2406 (4.5%) |

걸린 픽셀에서 가장 많은 색은 네 장 모두 **rgb(170,65,240) 언저리 — 자수정 보라**다.
`#FF00FF`는 1.7% 이하이고, **반투명 픽셀(=알파 경계, 진짜 키잉 잔상이 있다면 거기 모인다)도 6% 미만**이다.
즉 **걸린 것은 테두리가 아니라 몸통이고, 색은 마젠타가 아니라 보라다.**

대조군이 이걸 못 박는다. 같은 폴더에서
- **이미 통과해 들어간 `ore-03`은 걸린 픽셀이 3개**다(보라가 거의 없는 그림이 우연히 나왔다).
- **짐꾼족 `haul-01`은 0개**다(따뜻한 갈색이라 아예 안 걸린다).

**검사는 "키잉했는가"가 아니라 "보라색 생물인가"를 가르고 있다.** 광석족 프롬프트가
`faceted crystal cluster` + `rich jewel tones`인 이상, 모델은 거의 항상 자수정 보라를 그린다.
재생성으로 푸는 길이 34번 막힌 이유가 이것이다 — 뽑기 운이 아니라 **검사 기준의 문제다.**

**(2)번 결정에 필요한 숫자는 이제 다 있다. 다만 "순수 #FF00FF로 좁히기"만으로는 부족하다.**
색만 `r>240 and b>240 and g<40`으로 좁히면 `ore-06`(43)만 문턱(50) 아래로 내려가고
`ore-07`(53) · `ore-04`(99)는 아슬아슬하게, `ore-02`(448)는 넉넉히 남는다.

**색과 자리를 같이 보면 네 장이 한꺼번에 0이 된다.** 진짜 키잉 잔상은 **알파 경계(반투명 픽셀)에**
남는다 — 몸통 한가운데에 남을 수가 없다. 그래서 `순수 #FF00FF` **그리고** `알파<250`을 같이 걸면:

| 파일 | 순수 #FF00FF | 순수 #FF00FF **이면서** 반투명 |
|---|---|---|
| `ore-02` | 448 | **0** |
| `ore-04` | 99 | **1** |
| `ore-06` | 43 | **0** |
| `ore-07` | 53 | **0** |

**네 장 다 사실상 0이다.** 이게 "이 파일들에는 키잉 잔상이 없다"는 말의 숫자 형태다.

**기준을 바꾸는 것은 Tifania가 정할 일이라 이 세션은 검사도 프롬프트도 건드리지 않았다.**
네 장은 자리에 그대로 두고 커밋하지 않았다.

### 대조군을 실제로 돌려 봤다 (2026-09-21 06시 세션) — 위 제안은 그대로 쓰면 안 된다

04시 세션이 "옛 키잉 파일을 git 이력에서 꺼내 한 번 돌려 보면 확실해진다"고 남긴 걸 했다.
`icon-key.png`의 **키잉하던 시절 원본**은 `72daba5`에 있다(`f171407`이 "열쇠 재생성"으로 갈아끼운 것).

    git show 72daba5:PlanetRacer/Assets/Resources/Art/Icons/icon-key.png > 옛키.png
    python tools/check_magenta_kind.py 옛키.png

**결과가 제안을 뒤집는다.**

| 파일 | 걸린 픽셀 | 순수 #FF00FF | 반투명(알파<250) | **반투명 비율** |
|---|---|---|---|---|
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | **0** | 2103 | **74.8%** |
| `ore-02` (09-21 06시) | 38548 | 0 | 670 | 1.7% |
| `ore-04` (〃) | 31615 | 2 | 2155 | 6.8% |
| `ore-06` (〃) | 19082 | 0 | 1141 | 6.0% |
| `ore-07` (〃) | 71953 | 13 | 1571 | 2.2% |
| `ore-03` (통과한 것) | 3 | 0 | 0 | — |
| `haul-01` (통과한 것) | 0 | — | — | — |

1. **진짜 키잉 파일에도 순수 #FF00FF가 0개다.** 키잉 잔상은 배경과 섞여 나오기 때문에
   실제 색이 rgb(224,32,208)·rgb(208,32,192) 언저리다 — `r>240 and b>240 and g<40`에 안 걸린다.
   그러니 **"순수 #FF00FF로 좁히기"는 검사를 고치는 게 아니라 꺼 버리는 것이다.**
   04시 세션이 제안한 `순수 #FF00FF 그리고 반투명`은 옛 키잉 파일에서도 **0**이 나온다 —
   영원히 아무것도 안 잡는 조건이다. 그대로 넣으면 안 된다.
2. **실제로 둘을 가르는 건 "걸린 픽셀 중 몇 %가 알파 경계에 있는가"다.**
   진짜 키잉은 **74.8%**, 보라색 생물 넷은 **1.7~6.8%**다. 열 배 이상 벌어진다.
   절대 개수로는 못 가른다 — 옛 키잉(2103)과 `ore-04`(2155)가 거의 같다. **비율이라야 갈린다.**

**그래서 실제로 고른다면 이런 모양이 된다(제안일 뿐, 이 세션은 손대지 않았다).**
기존처럼 보라 전체(`r>150,b>150,g<100`)를 세되, 실패는 그중
**반투명 비율이 절반을 넘을 때만** 내는 것. 위 표의 일곱 개 파일이 전부 옳게 갈린다.

**이 절이 (2)번 결정에 필요한 마지막 조각이다.** 결정은 여전히 Tifania 몫이다.

### 다른 네 장으로 독립 재현했다 (2026-09-21 20시 세션) — 06시 제안이 맞는다

20시 세션이 네 장(`ore-02` · `ore-04` · `ore-06` · `ore-07`)을 **세 바퀴 돌려 12장을 새로 뽑고**,
그 결과물에 같은 측정을 다시 돌렸다. 06시 표와 **겹치지 않는 새 파일 넷**이다.

| 파일 | 걸린 픽셀 | 순수 #FF00FF | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `ore-02` (09-21 20시) | 1630 | 0 | 49 | **3.0%** |
| `ore-04` (〃) | 726 | 0 | 4 | **0.6%** |
| `ore-06` (〃) | 16087 | 8 | 1332 | **8.3%** |
| `ore-07` (〃) | 4183 | 0 | 372 | **8.9%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- **0.6~8.9%** — 06시가 다른 네 장에서 잰 1.7~6.8%와 같은 자리다. 진짜 키잉 74.8%와는 여전히 열 배 가까이 벌어진다.
- 걸린 픽셀의 최빈색도 네 장 다 `rgb(144~192, 32~96, 240)` — **자수정 보라 그대로**다.
- **서로 다른 두 세션의 여덟 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이 전부 옳게 갈린다.**
  바꾸면 자리에 있는 네 장이 **다시 뽑을 필요 없이 그대로 통과한다.**

**뽑기 결과 자체는 12장 0통과였다(광석족 누적 50연속 실패).** 다만 이번 세션 수치는
1차 41879/4989/6007/3721 · 2차 **467**/11325/2878/6826 · 3차 1630/726/16087/4183으로,
**02·04·06시보다 자릿수 하나가 낮다**(최저 467픽셀 — 00시의 137 이후 가장 낮다).
그림이 자수정 보라에서 하늘·남색 쪽으로 옮겨 오고 있는 것이 눈에 보이지만,
**문턱(50)을 넘은 적은 한 번도 없다.** 검사도 프롬프트도 이 세션은 건드리지 않았다.

### 세 번째 세션에서도 같은 결과가 나왔다 (2026-09-21 22시 세션) — 06시 기준이 또 맞는다

22시 세션이 네 장(`ore-02` · `ore-04` · `ore-06` · `ore-07`)을 **세 바퀴 돌려 12장을 새로 뽑았다.**
20시·06시 표와 겹치지 않는 **세 번째 독립 표본**이다.

| 파일 | 걸린 픽셀 | 순수 #FF00FF | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `ore-02` (09-21 22시) | 2659 | 0 | 53 | **2.0%** |
| `ore-04` (〃) | 3394 | 0 | 26 | **0.8%** |
| `ore-06` (〃) | 10604 | 0 | 1003 | **9.5%** |
| `ore-07` (〃) | 11988 | 0 | 608 | **5.1%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- **0.8~9.5%** — 06시(1.7~6.8%)·20시(0.6~8.9%)와 같은 자리다. 진짜 키잉 74.8%와는 여전히 열 배 가까이 벌어진다.
- **순수 #FF00FF가 네 장 모두 0개다.** 20시 표에서는 `ore-06` 8개 · `ore-07` 13개가 있었는데 이번엔 아예 없다.
  "순수 #FF00FF로 좁히기"가 검사를 꺼 버리는 것이라는 06시 결론이 더 분명해졌다.
- 걸린 픽셀의 최빈색도 네 장 다 `rgb(144~240, 48~96, 240)` — **자수정 보라 그대로**다.
- **이제 서로 다른 세 세션의 열두 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이 전부 옳게 갈린다.**
  바꾸면 자리에 있는 네 장이 다시 뽑을 필요 없이 그대로 통과한다.

**뽑기 결과 자체는 12장 0통과다(광석족 누적 62연속 실패).** 수치는
1차 4668/15019/4933/3394 · 2차 **689**/8515/1798/20768 · 3차 2659/3394/10604/11988.
최저치 689픽셀로 20시(467)보다는 높지만 02·04·06시보다는 낮다 — **문턱(50)을 넘은 적은 한 번도 없다.**
검사도 프롬프트도 이 세션은 건드리지 않았다.

### 2026-09-22 00시 세션 — 열두 장 뽑아 한 장 통과. 「재생성으로는 못 푼다」는 진단이 또 틀렸다

네 장(`ore-02` · `ore-04` · `ore-06` · `ore-07`)을 위에서부터 세 바퀴 돌리고 `ore-02`를 한 번 더 돌렸다(12장).
**`ore-06`이 2차에서 통과했다**(투명 56%, 마젠타 0픽셀) — 09-20 20시 이후 **66연속 실패 끝에 나온 첫 광석족 통과**다.
프롬프트는 장부 그대로, 한 글자도 안 고쳤다.

| 항목 | 1차 | 2차 | 3차 | 4차 |
|---|---|---|---|---|
| `ore-02` | 6876 | **660** | 31652 | 1798 |
| `ore-04` | 3561 | 12597 | 23016 | — |
| `ore-06` | 1163 | **통과(0)** | — | — |
| `ore-07` | 11374 | **857** | 3008 | — |

- **02시 세션이 남긴 "재생성 운으로 푸는 길은 사실상 닫혔다"는 결론은 이번 표본이 뒤집는다.**
  같은 프롬프트로 열두 번 중 한 번은 문턱 아래로 내려온다. 다만 **한 장 통과에 열두 장이 든다** —
  남은 세 장을 이 방식으로 마저 채우려면 세션이 서넛 더 필요하다는 뜻이기도 하다.
- 660 · 857처럼 **세 자리까지 내려온 표본이 두 번** 나왔다. 20시(467) · 22시(689)와 같은 자리다.
- **(2)번 결정은 여전히 가장 싼 길이다.** 검사를 06시 제안(반투명 비율 50% 초과일 때만 실패)으로 바꾸면
  자리에 있는 세 장이 다시 뽑을 필요 없이 그대로 통과한다. 다만 **"검사를 안 고치면 영영 못 채운다"는 말은 이제 틀렸다** —
  고치지 않아도 채워지기는 한다, 느릴 뿐이다.
- 검사도 프롬프트도 이 세션은 건드리지 않았다.

남은 세 장: 광석 3(`ore-02` · `ore-04` · `ore-07`).

### 2026-09-22 02시 세션 — 열두 장 뽑아 0장 통과. 세 장 모두 문턱 근처에 못 갔다

남은 세 장(`ore-02` · `ore-04` · `ore-07`)을 위에서부터 네 바퀴 돌렸다(12장).
프롬프트는 장부 그대로, 한 글자도 안 고쳤다. 검사도 안 건드렸다.

| 항목 | 1차 | 2차 | 3차 | 4차 |
|---|---|---|---|---|
| `ore-02` | 5603 | 15409 | 8820 | 13637 |
| `ore-04` | 8247 | 7804 | **4220** | 35359 |
| `ore-07` | 15640 | 43364 | 10538 | 5705 |

- **열두 번 중 가장 낮았던 게 4220픽셀이다** — 문턱(50)과는 두 자릿수 배수 차이다.
  00시 세션이 660 · 857 같은 세 자리를 두 번 봤던 것과 달리, 이번엔 네 자리 아래로 한 번도 안 내려갔다.
- 파일 세 장은 자리에 둔 채 커밋하지 않았다(추적 중이라 `M` 상태).
- **누적으로 보면 광석족 재생성은 12에 0~1 사이다.** 남은 세 장을 이 방식으로 채우려면
  세션이 더 들고, 그나마도 보장이 없다. **(2)번 결정이 여전히 가장 싼 길이다.**

### 2026-09-22 04시 세션 — 열두 장 뽑아 0장 통과. 「미리 보기」가 열두 번 중 여섯 번

세 장(`ore-02` · `ore-04` · `ore-07`)을 위에서부터 네 바퀴 돌렸다(12장).
프롬프트는 장부 그대로, 한 글자도 안 고쳤다. 검사도 안 건드렸다.

| 항목 | 1차 | 2차 | 3차 | 4차 |
|---|---|---|---|---|
| `ore-02` | 12757 | 4974 | **329** | 6748 |
| `ore-04` | 5414 | **283** | 20993 | 32099 |
| `ore-07` | 8493 | 13616 | 14654 | 16311 |

- **최저가 283픽셀이다** — 00시 세션의 660·857보다 낮고, 02시 세션의 최저(4220)보다 열다섯 배 낮다.
  그래도 문턱(50)까지는 다섯 배가 남았다. 세 세션 누적 36장에 통과 1장이다.
- **3·4차는 기존 파일보다 나쁠 때 덮어쓰지 않았다**(4차 두 장은 후보 파일로 받아 검사만 하고 버렸다).
  1·2차를 돌릴 때는 그 규칙이 없어서 `ore-02`의 329와 `ore-04`의 283이 뒤 회차에 덮여 사라졌다 —
  **다음 세션은 처음부터 후보 파일로 받아 검사한 뒤 더 좋을 때만 자리에 넣을 것.**
- 자리에 남은 세 장: `ore-02` 6748 · `ore-04` 20993 · `ore-07` 14654 (전부 미커밋 `M`).

### 절차 버그 하나를 찾았다 — 보낸 뒤 화면이 위로 남아 있으면 **앞 그림을 받는다**

첫 장에서 `사본 다운로드`로 받은 파일이 **02시 세션이 받아 둔 `ore-07`과 바이트 단위로 같았다**(MD5 동일).
편집기 탭은 열지도 않았다. 원인은 단순했다 — **프롬프트를 보낸 뒤 대화가 바닥까지 내려가지 않아서**
화면 맨 위에 보이던 그림이 새 그림이 아니라 **앞 세션이 남긴 그림**이었고, 거기에 오른쪽 클릭을 했다.
「맨 아래로 스크롤」 버튼이 떠 있었다는 게 단서였는데 그걸 안 봤다.
**받은 뒤 MD5를 앞 파일과 비교하지 않았으면 못 잡았을 종류다.**
`image-session.md` 2-2절에 "보낸 뒤 반드시 「맨 아래로 스크롤」부터 누른다"를 넣었다.

### 절차에서 배운 것 두 가지 (다음 세션이 그대로 쓰면 된다)

1. **「미리 보기」 딱지가 붙어 오른쪽 클릭 메뉴가 안 뜨는 경우가 이번엔 절반을 넘었다**(12장 중 7장).
   `image-session.md` 2-2절의 라이트박스 우회로가 **일곱 번 다 통했다** — 그림을 왼쪽 클릭해
   확대 보기를 연 뒤 거기서 오른쪽 클릭하면 Chromium 메뉴의 `Save Image As...`가 나온다.
   메뉴 항목 위치는 오른쪽 클릭 지점 **+(63, 51)**(`사본 다운로드`의 +(58,78)과 다르다).
   라이트박스는 받은 뒤 `Esc`로 닫는다.
2. **열두 장의 MD5가 전부 달랐다** — 09-21 20시에 나온 「편집기 탭이 앞 썸네일을 준다」는 버그는
   오른쪽 클릭·라이트박스 두 경로 모두에서 한 번도 재현되지 않았다.

### 2026-09-22 06시 세션 — 열두 장 뽑아 0장 통과. 다만 세 장 모두 자리 값이 크게 내려갔다

세 장(`ore-02` · `ore-04` · `ore-07`)을 위에서부터 네 바퀴 돌렸다(12장).
프롬프트는 장부 그대로, 한 글자도 안 고쳤다. 검사도 안 건드렸다.
**처음부터 끝까지 후보 파일(`cand01`~`cand12.png`)로 받아 검사한 뒤 앞 회차보다 좋을 때만 자리에 옮겼다**
(04시 세션이 문서에 넣은 규칙대로 — 덕분에 이번엔 좋은 값을 덮어써서 잃은 게 없다).

| 항목 | 1차 | 2차 | 3차 | 4차 | 자리에 남은 값 |
|---|---|---|---|---|---|
| `ore-02` | 27068 | 25993 | **1919** | 5770 | **1919** (앞 6748) |
| `ore-04` | **869** | 13397 | 9879 | 18800 | **869** (앞 20993) |
| `ore-07` | 4618 | **1762** | **398** | 6857 | **398** (앞 14654) |

- **통과는 0장이지만 세 자리가 다 좋아졌다.** `ore-07` 398픽셀은 09-21 00시의 137 이후 가장 낮은 값이고,
  세 장이 한 세션에서 동시에 내려간 것은 처음이다. 그래도 문턱(50)은 여덟 배 남았다.
- 열두 장의 MD5가 전부 달랐다. 「맨 아래로 스크롤」을 매번 먼저 눌렀고, 앞 그림을 받은 사고는 없었다.
- 세 파일은 자리에 둔 채 커밋하지 않았다(추적 중이라 `M` 상태).

#### 네 번째 독립 표본에서도 06시 기준이 또 맞는다

자리에 남은 세 장을 `check_magenta_kind.py`로 다시 쟀다. 06시·20시·22시 표와 겹치지 않는 **네 번째 표본**이다.

| 파일 | 걸린 픽셀 | #FF00FF 근처 + 반투명 | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `ore-02` (09-22 06시) | 1919 | 0 | 211 | **11.0%** |
| `ore-04` (〃) | 869 | 0 | 17 | **2.0%** |
| `ore-07` (〃) | 398 | 0 | 105 | **26.4%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- **2.0~26.4%** — 앞 세 표본(1.7~6.8% · 0.6~8.9% · 0.8~9.5%)보다 위쪽이지만 진짜 키잉 74.8%와는 여전히 세 배 가까이 벌어진다.
- 걸린 픽셀의 최빈색은 세 장 다 `rgb(144~192, 48~96, 240)` — **자수정 보라 그대로**다.
- **순수 #FF00FF이면서 반투명인 픽셀은 세 장 모두 0개다.**
- **이제 서로 다른 네 세션의 열다섯 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이 전부 옳게 갈린다.**
  바꾸면 자리에 있는 세 장이 다시 뽑을 필요 없이 그대로 통과한다.

#### 절차 메모 — 「미리 보기」가 여섯 번, 라이트박스 우회로가 여섯 번 다 통했다

12장 중 **6장**이 `미리 보기` 딱지가 붙은 채로 오른쪽 클릭 메뉴가 안 떴다.
그림을 왼쪽 클릭해 확대 보기를 연 뒤 거기서 오른쪽 클릭하면 `Save Image As...`가 나온다 — **여섯 번 다 통했다.**
메뉴 위치는 기록대로 라이트박스 **+(63, 51)** · `사본 다운로드` **+(58, 78)**, 열두 번 다 맞았다.
라이트박스 저장 대화상자는 `다운로드` 폴더에서 열리므로 전체 경로를 붙여 넣는 것이 특히 중요하다.
`Clipboard set` 뒤 `clip_check.ps1`로 앞 20자를 확인하는 절차는 열두 번 다 정상이었다(샌 적 없음).

### 2026-09-22 20시 세션 — 열두 장 뽑아 0장 통과. 다만 `ore-04`가 222픽셀까지 내려갔다

세 장(`ore-02` · `ore-04` · `ore-07`)을 위에서부터 네 바퀴 돌렸다(12장).
프롬프트는 장부 그대로, 한 글자도 안 고쳤다. 검사도 안 건드렸다.
후보 파일(`cand01`~`cand12.png`)로 받아 검사한 뒤 **앞 회차보다 좋을 때만** 자리에 옮겼다.

| 항목 | 1차 | 2차 | 3차 | 4차 | 자리에 남은 값 |
|---|---|---|---|---|---|
| `ore-02` | **1087** | 8211 | 1524 | 14374 | **1087** (앞 1919) |
| `ore-04` | 2679 | 9127 | 4206 | **222** | **222** (앞 869) |
| `ore-07` | 7025 | 12572 | 9913 | 3577 | **398** (그대로) |

- **통과는 0장이지만 두 자리가 또 좋아졌다.** `ore-04` 222픽셀은 광석족 누적 두 번째로 낮은 값이고
  (최저는 09-21 00시의 137), **자리에 남은 세 값이 처음으로 전부 1100 아래**다(1087 · 222 · 398).
  그래도 문턱(50)은 네 배 넘게 남았다.
- 열두 장의 MD5가 전부 달랐다. 「맨 아래로 스크롤」이 필요한 적은 없었고(보낸 뒤 매번 바닥이었다), 앞 그림을 받은 사고도 없었다.
- **「미리 보기」로 오른쪽 클릭 메뉴가 안 뜬 적이 한 번도 없었다(12/12 바로 떴다).** 06시 세션은 6/12였다.
  이번엔 시작할 때 ChatGPT 창을 보조 모니터 전체(1890x1030)로 키워 놓고 했다 — 인과인지는 한 세션으로 단정 못 하니
  다음 세션도 창을 키워 놓고 해 보면 확인된다.
- 세 파일은 자리에 둔 채 커밋하지 않았다(추적 중이라 `M` 상태).

#### 다섯 번째 독립 표본에서도 06시 기준이 또 맞는다

자리에 남은 세 장을 `check_magenta_kind.py`로 쟀다. 앞 네 표본과 겹치지 않는 **다섯 번째 표본**이다.

| 파일 | 걸린 픽셀 | #FF00FF 근처 + 반투명 | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `ore-02` (09-22 20시) | 1087 | 0 | 12 | **1.1%** |
| `ore-04` (〃) | 222 | 0 | 0 | **0.0%** |
| `ore-07` (〃) | 398 | 0 | 105 | **26.4%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- **0.0~26.4%** — 진짜 키잉 74.8%와는 여전히 세 배 가까이 벌어진다. `ore-04`는 **반투명 픽셀이 아예 0개**다.
- 걸린 픽셀의 최빈색은 세 장 다 `rgb(144~176, 48~96, 240)` — **자수정 보라 그대로**다.
- 순수 #FF00FF이면서 반투명인 픽셀은 세 장 모두 0개.
- **이제 서로 다른 다섯 세션의 열여덟 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이 전부 옳게 갈린다.**
  바꾸면 자리에 있는 세 장이 다시 뽑을 필요 없이 그대로 통과한다.

### 2026-09-22 22시 세션 — 열두 장 뽑아 0장 통과. 자리 값 셋 다 그대로

세 장(`ore-02` · `ore-04` · `ore-07`)을 위에서부터 돌렸다(12장, 세션 상한).
프롬프트는 장부 그대로, 한 글자도 안 고쳤다. 검사도 안 건드렸다.
후보 파일(`cand01`~`cand12.png`)로 받아 검사한 뒤 **앞 회차보다 좋을 때만** 옮기는 규칙을 그대로 지켰는데,
**열두 장 전부 자리 값보다 나빠서 한 장도 안 옮겼다.**

| 항목 | 회차별 걸린 픽셀 | 이번 최저 | 자리에 남은 값 |
|---|---|---|---|
| `ore-02` | 17633 · 14175 · 21510 · 10027 · 21947 (5회) | 10027 | **1087** (그대로) |
| `ore-04` | 15579 · 8851 · 9256 (3회) | 8851 | **222** (그대로) |
| `ore-07` | 2403 · 11421 · 7688 · 14580 (4회) | **2403** | **398** (그대로) |

- **열두 장 중 최저가 2403픽셀**이다. 문턱은 50이고 자리 값은 이미 222~1087이라 **한 장도 근처에 못 갔다.**
  09-22 밤(00·02·04·06·20시) 다섯 세션에 이어 여섯 번째 세션인데, 자리 값이 처음으로 **전혀 안 내려갔다.**
- 누적: **여섯 세션 72장에 통과 1장**(00시의 `ore-06`).
- 열두 장의 MD5가 전부 달랐다. 앞 그림을 받은 사고 없음.

#### GPT가 한 요청에 그림을 두 장 준 적이 두 번 있었다 (새로 관찰)

`ore-02` 2회차와 `ore-07` 3회차에서, 응답이 아직 스트리밍 중인 상태에서 **같은 응답이 그림을 한 장 더 그렸다.**
둘 다 받아서 검사에 넣었다(`cand05` 21510 · `cand11` 14580) — 위 표의 회차 수가 세 항목이 5·3·4로 안 맞는 이유다.
한도는 어차피 그림 수로 세므로 **12장에서 끊는 규칙은 그대로 지켰다.**
다음 세션도 보내고 나서 그림이 한 장 더 붙는지 보고, 붙으면 그것도 세어서 12장을 넘기지 말 것.

#### 「미리 보기」와 편집기 탭 — 한 번 걸렸고 우회로가 통했다

- 12장 중 **2장**이 `미리 보기` 딱지가 붙은 채로 남았다(06시 6/12, 20시 0/12 사이).
- 그중 한 장은 오른쪽 클릭에 메뉴가 아예 안 떴다. **장부에 적힌 라이트박스 우회로(그림 왼쪽 클릭)를 썼더니
  라이트박스가 아니라 「이미지 편집기 탭」이 열렸다** — 09-21 20시가 경고한 그 탭이다.
  거기서 받지 않고 **탭을 닫고 대화로 돌아가니 `미리 보기` 딱지가 사라져 있었고, 오른쪽 클릭이 정상으로 떴다.**
  받은 파일은 정상이었다(MD5 고유).
  **다음 세션이 쓸 순서: 메뉴가 안 뜨면 → 그림 왼쪽 클릭 → (라이트박스가 아니라 편집기 탭이 열리면) 탭 닫기 →
  대화로 돌아와 오른쪽 클릭 다시.** 편집기 탭에서 내려받지 않으니 09-21의 「앞 썸네일을 준다」 버그에도 안 걸린다.
- 메뉴 위치는 기록대로 `사본 다운로드` = 오른쪽 클릭 지점 **+(58, 78)**, 열두 번 다 맞았다.
- `Clipboard set` 뒤 `clip_check.ps1`로 앞 20자를 확인하는 절차는 열두 번 다 정상이었다(샌 적 없음).
- 도중에 사람이 PC를 만진 흔적은 없었다.

#### 여섯 번째 독립 표본에서도 06시 기준이 또 맞는다

이번 세션 후보 중 낮은 셋을 `check_magenta_kind.py`로 쟀다. 앞 다섯 표본과 겹치지 않는 **여섯 번째 표본**이다.

| 파일 | 걸린 픽셀 | #FF00FF 근처 + 반투명 | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `cand03` (`ore-07` 1회차) | 2403 | 0 | 322 | **13.4%** |
| `cand10` (`ore-07` 3회차) | 7688 | 0 | 507 | **6.6%** |
| `cand06` (`ore-04` 2회차) | 8851 | 0 | 299 | **3.4%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- 최빈색은 셋 다 `rgb(144~192, 64~96, 240)` — **자수정 보라 그대로**다. 순수 #FF00FF이면서 반투명인 픽셀은 셋 모두 0개.
- **이제 서로 다른 여섯 세션의 스물한 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이 전부 옳게 갈린다.**
  바꾸면 자리에 있는 세 장이 다시 뽑을 필요 없이 그대로 통과한다.

### 2026-09-23 00시 세션 — 열두 장 뽑아 0장 통과. `ore-02`만 1087 → 852로 내려갔다

세 장(`ore-02` · `ore-04` · `ore-07`)은 프롬프트가 **바이트 단위로 같다**(종 이름이 프롬프트에 안 들어간다).
그래서 이번 세션은 같은 프롬프트로 열두 장을 뽑으면서, **회차마다 그 그림이 세 자리 중 어느 것이든 개선하면 그 자리에 넣는** 식으로 돌렸다.
장부 규칙("후보 파일로 받아 앞 회차보다 좋을 때만 옮긴다")은 그대로고, 어느 자리에 대느냐만 고정하지 않은 것이다 —
이 문서가 이미 "어느 파일이 어느 종이 되는지는 뽑는 순서가 정할 뿐"이라고 적어 둔 대로다.

| 후보 | 걸린 픽셀 | 후보 | 걸린 픽셀 |
|---|---|---|---|
| `cand01` | 3417 | `cand07` | 6408 |
| `cand02` | 7134 | `cand08` | 7948 |
| `cand03` | 18540 | `cand09` | 3755 |
| `cand04` | **861** | `cand10` | 2595 |
| `cand05` | 14070 | `cand11` | **852** |
| `cand06` | 4537 | `cand12` | 1272 |

- **열두 장 중 최저가 852픽셀**(`cand11`)이다. 문턱은 50이다.
  `cand04`(861)로 `ore-02`를 한 번 갈아 끼운 뒤, 뒤에 나온 `cand11`(852)로 다시 갈아 끼웠다.
- 자리에 남은 값: **`ore-02` 852**(앞 1087) · **`ore-04` 222**(그대로) · **`ore-07` 398**(그대로).
  열두 장 중 222·398보다 낮은 것이 하나도 없어 그 두 자리는 한 장도 안 옮겼다.
- 누적: **일곱 세션 84장에 통과 1장**(09-22 00시의 `ore-06`).
- 프롬프트는 장부 그대로, 한 글자도 안 고쳤다. `check_alpha.py`도 안 건드렸다.
- 열두 장의 MD5가 전부 달랐다. 앞 그림을 받은 사고 없음.

#### 일곱 번째 독립 표본에서도 06시 기준이 또 맞는다

| 파일 | 걸린 픽셀 | #FF00FF 근처 + 반투명 | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `ore-02` (09-23 00시, 새로 들어감) | 852 | 0 | 17 | **2.0%** |
| `ore-04` (그대로) | 222 | 0 | 0 | **0.0%** |
| `ore-07` (그대로) | 398 | 0 | 105 | **26.4%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- 최빈색은 세 장 다 `rgb(144~176, 48~96, 240)` — **자수정 보라 그대로**다. 순수 #FF00FF는 세 장 모두 0개.
- **이제 서로 다른 일곱 세션의 스물네 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이 전부 옳게 갈린다.**
  바꾸면 자리에 있는 세 장이 다시 뽑을 필요 없이 그대로 통과한다.

#### 절차 메모

- **`미리 보기`로 오른쪽 클릭 메뉴가 안 뜬 적이 한 번도 없었다(12/12).** 09-22 20시(0/12)와 같고 22시(2/12)·06시(6/12)보다 좋다.
  ChatGPT 창을 보조 모니터 전체(1890x1030)로 둔 채 시작한 것이 20시·이번 두 세션의 공통점이다.
- `사본 다운로드` 위치 = 오른쪽 클릭 지점 **+(58, 78)**, 열두 번 다 맞았다. 라이트박스 우회로는 쓸 일이 없었다.
- **GPT가 한 요청에 그림을 두 장 준 적이 한 번 있었다**(`cand04` 다음에 `cand05`). 09-22 22시 관찰과 같다.
  첫 장을 받은 뒤 25초쯤 더 기다렸다가 화면을 다시 보면 두 번째가 붙었는지 알 수 있다. 12장 상한은 그림 수로 세서 그대로 지켰다.
- 클립보드 확인은 열두 번 다 정상이었다(샌 적 없음). 이번엔 프롬프트를 `Assets/Screenshots/_prompt.txt`(gitignore)에
  한 번 써 두고 `_setclip.ps1`로 **설정과 확인을 PowerShell 한 번에** 처리했다 — 매 회차 호출이 하나 줄어든다.
  임시 파일들은 세션 끝에 지웠다.
- **함정 하나(다음 세션이 겪지 말 것):** 이 세션의 컨테이너 쪽 `/tmp`는 쓰기가 막혀 있는데,
  **앞 세션이 같은 경로에 남겨 둔 파일이 그대로 읽혔다.** heredoc이 `Permission denied`로 실패했는데도
  뒤이은 python이 09-22 20시 세션의 낡은 `/tmp/sec.md`를 읽어 **그 세션 기록을 이 문서에 한 번 더 붙였다.**
  `git show HEAD:...`로 되돌리고 다시 썼다. 긴 문서 조각은 `/tmp` 말고 **연결 폴더 안**(gitignore된 곳)에 쓸 것.

### 2026-09-23 02시 세션 — 열두 장 뽑아 한 장 통과. `ore-02`가 852 → 0으로 끝났다

전제는 다 맞았다. KST 02:09 시작, 유휴 5059초, ChatGPT 앱 떠 있음, 대기 중 세 장,
오늘 daily에 `이미지 한도` 기록 없음(이번 세션도 한도는 안 만났다).
`prepare_capture.ps1` → `CAPTURE-OK`(입력 데스크톱 `Default`, 화면 보호기 `scrnsave.scr` pid 11792 종료).

**결과: 세션 상한 12장을 다 썼고 통과는 한 장이다.**

| 후보 | 걸린 픽셀 | 후보 | 걸린 픽셀 |
|---|---|---|---|
| `cand01` | 14117 | `cand07` | 12173 |
| `cand02` | 3716 | `cand08` | 8501 |
| `cand03` | 13122 | `cand09` | 5289 |
| `cand04` | 6338 | `cand10` | 10480 |
| `cand05` | 5127 | `cand11` | 4771 |
| `cand06` | **0 (통과)** | `cand12` | 5679 |

- **`cand06`이 마젠타 0픽셀로 통과**해 `ore-02` 자리에 들어갔다(커밋 36aec0c). 투명 영역 66%.
  `check_magenta_kind.py`로도 0이다 — 문턱을 어떻게 잡든 통과하는 그림이다.
- 자리에 남은 값: **`ore-02` 0(통과, 커밋 완료)** · **`ore-04` 222**(그대로) · **`ore-07` 398**(그대로).
  누적 **여덟 세션 96장에 통과 2장.**
- 세 항목의 프롬프트가 바이트 단위로 같아서 00시 세션과 같이 **뽑힌 그림이 세 자리 중 어느 것이든
  개선하면 그 자리에 넣는** 식으로 돌렸다. `cand02`(3716)부터 `cand12`(5679)까지 열한 장은
  모두 `ore-04`(222)·`ore-07`(398)보다 나빠서 자리에 손대지 않았다.
- 프롬프트는 장부 그대로, 한 글자도 안 고쳤다. `check_alpha.py`도 안 건드렸다.
- 열두 장 MD5 전부 다름. 앞 그림을 받은 사고 없음. 클립보드 확인 12/12 정상.
- 「미리 보기」로 오른쪽 클릭 메뉴가 안 뜬 것은 **12번째 한 번**뿐이었다. 라이트박스 우회로로 받았고
  (`Save Image As...`) 파일은 똑같이 1254x1254 RGBA였다. 우회로는 지금도 잘 동작한다.
- `cand09`를 받을 때 **이미지 편집기 탭이 저절로 열렸다**(오른쪽 클릭 → 사본 다운로드 경로였는데도).
  받은 파일 자체는 MD5가 새것이라 문제없었지만, 장부 경고대로 **바로 탭을 닫고** 이어 갔다.
  다음 세션도 편집기 탭이 보이면 그 자리에서 닫을 것.
- 후보 파일 `cand01`~`cand12.png`와 임시 스크립트는 세션 끝에 지웠다.

### 이어서 할 것

- **Tifania: (2)번 결정은 여전히 남아 있다.** `ore-04`(222) · `ore-07`(398) 두 장은 여덟 세션 동안
  문턱(50) 아래로 내려간 적이 없다. 다만 이번 `cand06`이 0픽셀로 나온 것은
  **재생성으로도 뚫린다**는 증거다 — 09-22 06시의 「재생성으로는 못 푼다」 진단은 여전히 틀렸다.
  기준을 바꾸지 않아도 언젠가는 두 장 다 들어온다. 다만 기댓값이 12장에 한 장꼴이라 네 세션쯤 더 걸린다.
- 다음 이미지 세션(04시): 대기 중은 `ore-04` · `ore-07` 두 장. 자리 값이 **222 · 398**이므로
  그보다 나쁜 회차는 옮기지 말 것.
- `ore-04`·`ore-07`의 png는 여전히 미커밋 수정 상태다.

### 2026-09-23 04시 세션 — 열두 장 뽑아 0장 통과. `ore-04`가 222 → 76으로 내려갔다

전제는 다 맞았다. KST 04:09 시작, 유휴 4366초, ChatGPT 앱 떠 있음, 대기 중 두 장,
오늘 daily에 `이미지 한도` 기록 없음(이번 세션도 한도는 안 만났다).
`prepare_capture.ps1` → `CAPTURE-OK`(입력 데스크톱 `Screen-saver` → 화면 보호기 `scrnsave.scr` pid 24252 종료).

**결과: 세션 상한 12장을 다 썼고 통과는 0장이다. 자리 값은 하나가 내려갔다.**

| 후보 | 걸린 픽셀 | 후보 | 걸린 픽셀 |
|---|---|---|---|
| `cand01` | 1955 | 7번째 | **못 받음**(아래 참고) |
| `cand02` | **76** | `cand08` | 3009 |
| `cand03` | 7502 | `cand09` | 1752 |
| `cand04` | 1842 | `cand10` | 3042 |
| `cand05` | 1212 | `cand11` | 1628 |
| `cand06` | 21598 | `cand12` | 1065 |

- **`cand02`(76)로 `ore-04`를 갈아 끼웠다**(앞 222). 문턱은 50이라 여전히 실패지만
  **여덟 세션 동안 나온 `ore-04` 값 중 가장 낮고, 문턱까지 26픽셀 남았다.**
- 자리에 남은 값: **`ore-04` 76**(앞 222) · **`ore-07` 398**(그대로).
  나머지 열 장은 전부 398보다 나빠서 `ore-07`은 한 장도 안 옮겼다.
- 누적 **아홉 세션 108장에 통과 2장.**
- 두 항목의 프롬프트가 바이트 단위로 같아서 00시·02시 세션처럼 **뽑힌 그림이 두 자리 중
  어느 것이든 개선하면 그 자리에 넣는** 식으로 돌렸다.
- 프롬프트는 장부 그대로, 한 글자도 안 고쳤다. `check_alpha.py`도 안 건드렸다.
- 받은 열한 장 MD5 전부 다름. 앞 그림을 받은 사고 없음. 클립보드 확인 12/12 정상.

#### 여덟 번째 독립 표본에서도 06시 기준이 또 맞는다

| 파일 | 걸린 픽셀 | #FF00FF 근처 + 반투명 | 반투명(alpha<250) | **반투명 비율** |
|---|---|---|---|---|
| `ore-04` (09-23 04시, 새로 들어감) | 76 | 0 | 0 | **0.0%** |
| `ore-07` (그대로) | 398 | 0 | 105 | **26.4%** |
| **옛 `icon-key.png` (진짜 키잉)** | 2813 | 0 | 2103 | **74.8%** |

- 최빈색은 둘 다 `rgb(144~160, 48~96, 240)` — **자수정 보라 그대로**다. 순수 #FF00FF는 둘 다 0개.
- **이제 서로 다른 여덟 세션의 스물여섯 장에서 06시 제안 기준(반투명 비율 50% 초과일 때만 실패)이
  전부 옳게 갈린다.** 바꾸면 자리에 있는 두 장이 다시 뽑을 필요 없이 그대로 통과하고 펫 아트가 끝난다.

#### 절차 메모 — 「미리 보기」가 이번엔 심했고, 라이트박스에서 앱이 터지는 새 버그를 봤다

- **오른쪽 클릭 메뉴가 뜬 것은 처음 다섯 장뿐이고(5/12), 여섯 번째부터는 한 번도 안 떴다.**
  09-23 00시(12/12)·02시(11/12)와 정반대다. 세션 도중에 바뀌었다 — 창 크기는 그대로
  보조 모니터 전체(1890x1030)였으니 **창 크기 가설로는 설명되지 않는다.**
- 여섯 번째부터는 전부 **라이트박스 우회로(`Save Image As...`)로 받았고** 파일은 똑같이 1254x1254 RGBA였다.
- **새 버그: 라이트박스에서 `Save Image As...`를 누르면 ChatGPT 앱이 JavaScript 오류 창을 띄우는 경우가 있다.**
  `A JavaScript error occurred in the main process / TypeError: Cannot read properties of null
  (reading 'getOwnerBrowserWindow')`. 저장 대화상자가 아예 안 뜬다.
  **7번째 그림에서 두 번 연속 이 오류가 나서 그 장은 못 받고 건너뛰었다**(장부 규칙: 두 번 시도하고 넘어간다).
  `확인`을 눌러 오류 창을 닫고 `Esc`로 라이트박스를 닫은 뒤 **다음 프롬프트로 넘어가면 그 다음 장부터는 다시 된다** —
  8~12번째 다섯 장은 같은 경로로 전부 정상이었다. 앱을 다시 띄울 필요는 없다.
- `사본 다운로드` 위치 = 오른쪽 클릭 지점 **+(58, 78)**, 라이트박스 `Save Image As...` = **+(66, 51)**.
- 라이트박스 저장 대화상자는 `다운로드` 폴더에서 열리고 파일 이름이 `download.png`다.
  (오른쪽 클릭 → `사본 다운로드`는 지난번 저장 위치를 기억한다.) 어느 쪽이든 `Ctrl+A` 후 전체 경로를 붙여 넣으면 된다.
- 도중에 사람이 PC를 만진 흔적 없음. 후보 파일 `cand01`~`cand12.png`와 임시 파일은 세션 끝에 지웠다.

### 이어서 할 것

- **Tifania: (2)번 결정은 여전히 남아 있다.** `ore-04`가 76까지 내려왔다(문턱 50). 기준을 안 바꿔도
  `ore-04`는 곧 들어올 것 같지만 `ore-07`(398)은 아홉 세션째 제자리다.
  `python tools/check_magenta_kind.py <파일>` 한 줄이면 두 장 다 확인된다.
- 다음 이미지 세션(06시): 대기 중은 `ore-04`(76) · `ore-07`(398) 두 장. 그보다 나쁜 회차는 옮기지 말 것.
- `ore-04`·`ore-07`의 png는 여전히 미커밋 수정 상태다.

### Resources/Art/Pets/6-myth/ore-04.png — 신화 — 서릿결 현자 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-07.png — 신화 — 공명하는 정동 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

## 들어온 것

### [x] Resources/Art/Pets/6-myth/ore-02.png — 신화 — 정맥 탐색자 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/ore-02.png (커밋 36aec0c)

### [x] Resources/Art/Pets/6-myth/ore-06.png — 신화 — 심층 광부 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: Resources/Art/Pets/6-myth/ore-06.png (커밋 d6a2a06)



### [x] Resources/Art/Pets/7-transcend/drill-sovereign.png — 초월 1/10 — 굴착의 군주 (채굴 산출)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a colossal crowned wheel ringed with rotating drill bits, molten gold light in the gaps, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: Resources/Art/Pets/7-transcend/drill-sovereign.png (커밋 38ead85)


### [x] Resources/Art/Pets/7-transcend/ember-heart.png — 초월 10/10 — 불씨의 심장 (연료 회복)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a crystal creature with an open chest cavity holding a burning ember core, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: Resources/Art/Pets/7-transcend/ember-heart.png (커밋 af1a462)

### [x] Resources/Art/Pets/6-myth/wing-03.png — 신화 — 안개 사냥꾼 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: Resources/Art/Pets/6-myth/wing-03.png (커밋 d9ef09c)


### [x] Resources/Art/Icons/icon-refinery.png — 아이콘 — 제련소 (2026-09-20 추가)

A-16 아트 배선에서 업그레이드 화면 제련소 줄에 넣을 그림이 없어서 `icon-blueprint`를
임시로 쓰기로 했다. 전용 그림이 들어오면 갈아 끼운다.

    <스타일 고정문>
    A small refinery furnace icon: a squat crucible with a glowing molten pour spout,
    a faint heat shimmer above it, seen three-quarters from the front,
    on a fully transparent background — real alpha channel, no background color,
    no checkerboard, no shadow, no gradient, centered, readable at 64x64 pixels.
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-refinery.png (커밋 f647c1c)

### [x] Resources/Art/Icons/icon-part-body.png — 아이콘 — 부품: 차체 (2026-09-20 추가)

제작 화면 다섯 줄 중 `body` 줄에 아이콘이 없다. 나머지 셋(engine·tire·suspension)은 있다.

    <스타일 고정문>
    A car chassis/body shell part icon: a rounded racing body panel seen three-quarters,
    clean metal with one accent stripe,
    on a fully transparent background — real alpha channel, no background color,
    no checkerboard, no shadow, no gradient, centered, readable at 64x64 pixels.
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-body.png (커밋 f647c1c)

### [x] Resources/Art/Icons/icon-part-booster.png — 아이콘 — 부품: 부스터 (2026-09-20 추가)

제작 화면 `booster` 줄. 위와 같은 이유다.

    <스타일 고정문>
    A booster thruster part icon: a short cylindrical rear thruster with a flared nozzle
    and a small blue flame at the tip, seen three-quarters,
    on a fully transparent background — real alpha channel, no background color,
    no checkerboard, no shadow, no gradient, centered, readable at 64x64 pixels.
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-booster.png (커밋 f647c1c)

### [x] Resources/Art/Pets/6-myth/wing-04.png — 신화 — 쌍익 도굴꾼 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-04.png (커밋 32f04fa)

### [x] Resources/Art/Pets/6-myth/ore-03.png — 신화 — 용암 조각가 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/ore-03.png (커밋 32f04fa)

### [x] Resources/Art/Pets/6-myth/ore-05.png — 신화 — 원석 수도사 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/ore-05.png (커밋 32f04fa)


<!-- 프로젝트에 반영된 것 -->

### [x] Resources/Art/Pets/7-transcend/comet-racer.png — 초월 4/10 — 혜성 질주자 (레이스 속도)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a streamlined twin-wheeled creature trailing a comet tail of white fire, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/comet-racer.png (커밋 8966ded)

### [x] Resources/Art/Pets/7-transcend/burst-phoenix.png — 초월 5/10 — 폭발의 불새 (부스트)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a bird-like creature with thruster wings, exhaust blooming into feathers of blue flame, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/burst-phoenix.png (커밋 90cd00b)

### [x] Resources/Art/Pets/7-transcend/shard-weaver.png — 초월 9/10 — 조각의 직조자 (조각 획득)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a many-armed crystal weaver spinning floating shards into a lattice, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/shard-weaver.png (커밋 6c960a1)

### [x] Resources/Art/Pets/6-myth/wheel-04.png — 신화 — 폭주 기수 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-04.png (커밋 fc22cab)

### [x] Resources/Art/Pets/6-myth/wheel-05.png — 신화 — 황혼 바퀴 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-05.png (커밋 fc22cab)

### [x] Resources/Art/Pets/6-myth/wheel-06.png — 신화 — 이중륜 술사 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-06.png (커밋 fc22cab)

### [x] Resources/Art/Pets/6-myth/wheel-07.png — 신화 — 먼지 폭군 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-07.png (커밋 e2f6b4f)

### [x] Resources/Art/Pets/6-myth/wheel-08.png — 신화 — 광륜 기사 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-08.png (커밋 e2f6b4f)

### [x] Resources/Art/Pets/6-myth/wing-01.png — 신화 — 성층권 파수꾼 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-01.png (커밋 47583b2)

### [x] Resources/Art/Pets/7-transcend/refinery-sage.png — 초월 2/10 — 제련의 현자 (정제 속도)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a tall crystal sage with a furnace glowing inside its chest, rings of molten light orbiting it, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/refinery-sage.png (커밋 408a59c)

### [x] Resources/Art/Pets/7-transcend/vault-titan.png — 초월 3/10 — 화물의 거인 (화물칸)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a broad armored beast carrying a vast glowing container that folds open like petals, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/vault-titan.png (커밋 def81a2)

### [x] Resources/Art/Pets/7-transcend/fortune-key.png — 초월 6/10 — 행운의 열쇠 (상자 등급)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a floating creature shaped like an ornate key with wings, keyholes glowing across its body, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/fortune-key.png (커밋 46c0099)

### [x] Resources/Art/Pets/7-transcend/beacon-herald.png — 초월 7/10 — 신호의 전령 (광고 보상)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a lantern-bodied herald with a broadcasting horn, concentric light rings pulsing outward, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/beacon-herald.png (커밋 46c0099)

### [x] Resources/Art/Pets/7-transcend/dream-keeper.png — 초월 8/10 — 잠의 수호자 (오프라인 상한)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a sleepy moon-faced guardian curled around a glowing hourglass, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/dream-keeper.png (커밋 46c0099)

### [x] Resources/Art/Pets/1-common/wheel-quartz.png — 일반 — 바퀴족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/wheel-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/1-common/wing-quartz.png — 일반 — 날개족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/wing-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/1-common/ore-quartz.png — 일반 — 광석족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/ore-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/1-common/haul-quartz.png — 일반 — 짐꾼족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/haul-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/6-myth/wheel-01.png — 신화 — 톱니 순례자 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-01.png (커밋 4e8caa8)

### [x] Resources/Art/Pets/6-myth/wheel-02.png — 신화 — 궤도 방랑자 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-02.png (커밋 4e8caa8)

> **2·3·4등급 골격 12장이다.** `pet-gacha.md` 7절 "1~4등급 52종"의 나머지 —
> 계열 4 × 등급 4 = 16장 중 1등급 4장은 들어왔고 이 12장이 남은 것이다.
> 1등급과 같은 이유로 **행성 색을 안 쓰고 중립 회청색(#8C94B8)**으로 받는다.
> 계열 문장은 초월 10종·1등급 4장과 똑같고, 등급 문장만 갈아 끼웠다 —
> 7절 예시(1등급 "plain rubber tread" / 4등급 "twin wheels with a metal rim" /
> 7등급 "glowing ring, ornate plating") 사이를 메우는 방향이다.
>
> **올리는 순서 3·4번(6등급 30종·5등급 18종)을 건너뛰고 5번을 먼저 집었다.**
> 3·4번은 종 이름과 고유 효과 48개를 새로 정해야 하는데, 그건 밤 세션이 혼자
> 정할 일이 아니라 Tifania에게 물을 일이다(초월 10종은 2절이 "서로 다른 축 10개"를
> 이미 고정해 둬서 나눠 붙이기만 하면 됐다). 골격 12장은 7절이 이미 정해 둔 것이라 바로 뽑을 수 있다.

> **2026-09-20 — 아래 `Resources/Art/Pets/pet-t*.png` 경로의 파일들은 지웠다.**
> T-12에서 구조가 4계열 x N색으로 정해지면서 등급 폴더(`Pets/1-common` … `Pets/7-transcend`)
> 쪽이 살아 있는 이름이 됐고, 옛 경로 26장은 빌드 용량(26.2MB)만 먹고 있었다.
> 항목 자체는 **기록이라 남겨 둔다.** 파일은 `git rm`으로 지웠으니 커밋 이력에서 꺼낼 수 있다.
> 자세한 것은 `docs/design/art-wiring.md` 4절.

### [x] Resources/Art/Pets/pet-t3-carrier.png — 희귀 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a wooden crate strapped to its back, no plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-carrier.png (커밋 33f5471)

### [x] Resources/Art/Pets/pet-t4-carrier.png — 영웅 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a reinforced metal crate with a latch on its back, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-carrier.png (커밋 33f5471)

### [x] Resources/Art/Pets/pet-t2-wheel.png — 고급 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a slightly larger wheel with a studded tread and a thin metal band, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-wheel.png (커밋 4f89a35)

### [x] Resources/Art/Pets/pet-t3-wheel.png — 희귀 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a reinforced wheel with a patterned tread and small bolted plates, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-wheel.png (커밋 4f89a35)

### [x] Resources/Art/Pets/pet-t4-wheel.png — 영웅 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  twin wheels with a metal rim and reinforced plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-wheel.png (커밋 4f89a35)

### [x] Resources/Art/Pets/pet-t2-wing.png — 고급 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  slightly longer wings with soft feather tips, no plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-wing.png (커밋 6fd71f7)

### [x] Resources/Art/Pets/pet-t3-wing.png — 희귀 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  wings with small metal cuffs at the joints, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-wing.png (커밋 6fd71f7)

### [x] Resources/Art/Pets/pet-t4-wing.png — 영웅 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  layered wings with reinforced plating along the leading edge, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-wing.png (커밋 6fd71f7)

### [x] Resources/Art/Pets/pet-t2-ore.png — 고급 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a slightly larger lump with a few cleanly cut facets, dull surface, no glowing core,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere.
- 참고: 투명 PNG로 받는다. 결정은 보랏빛으로 새기 쉬워서 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-ore.png (커밋 14f4d2f)

### [x] Resources/Art/Pets/pet-t3-ore.png — 희귀 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a clustered body with sharper faceted shards, no glowing core,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere.
- 참고: 투명 PNG로 받는다. 결정은 보랏빛으로 새기 쉬워서 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-ore.png (커밋 14f4d2f)

### [x] Resources/Art/Pets/pet-t4-ore.png — 영웅 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a layered crystal body with a metal band around its middle, no glowing core,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere.
- 참고: 투명 PNG로 받는다. 결정은 보랏빛으로 새기 쉬워서 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-ore.png (커밋 14f4d2f)

### [x] Resources/Art/Pets/pet-t2-carrier.png — 고급 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a slightly larger cloth saddlebag with buckled straps, no plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-carrier.png (커밋 0062fee)

> **계열 넷의 1등급(일반) 골격 4장이다.** `pet-gacha.md` 7절 "올리는 순서" 2번 —
> 무료 뽑기에서 제일 자주 보인다. 7절 "1~4등급 52종"대로 **이 4장은 색 변종의 바탕**이라
> 행성 색을 안 쓰고 중립 회청색(#8C94B8)으로 받는다. 나중에 행성 색으로 갈아 끼운다.
> 계열 문장은 초월 10종과 **똑같은 문장**을 쓰고 등급 문장만 1등급용으로 바꿨다.

### [x] Resources/Art/Pets/pet-t1-wheel.png — 일반 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a small plain wheel with a rubber tread, no decoration, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 초월 바퀴족 셋과 나란히 놓았을 때 같은 생물의 어린 모습으로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-wheel.png (커밋 bbe60ee)

### [x] Resources/Art/Pets/pet-t1-wing.png — 일반 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  plain bare wings, no plating, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 초월 날개족 셋과 같은 생물의 어린 모습으로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-wing.png (커밋 bbe60ee)

### [x] Resources/Art/Pets/pet-t1-ore.png — 일반 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a small rough uncut lump, dull surface, no glowing core, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere, including highlights and edges.
- 참고: 투명 PNG로 받는다. 지오드코어처럼 결정은 보랏빛으로 새기 쉬워서 금지 줄을 처음부터 붙였다
- 참고 추가: 첫 판은 응답이 아예 안 왔다(그림이 나쁜 게 아니라 빈 응답). 같은 프롬프트로 한 번 더 보내니 바로 나왔다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-ore.png (커밋 bbe60ee)

### [x] Resources/Art/Pets/pet-t1-carrier.png — 일반 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a simple empty cloth saddlebag, no plating, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 초월 짐꾼족 둘과 같은 생물의 어린 모습으로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-carrier.png (커밋 bbe60ee)

> **7등급(초월) 10종이다.** `docs/design/pet-gacha.md` 7절 "올리는 순서" 1번.
> 종 이름과 계열·맡는 축은 같은 문서 2절(10마리가 서로 다른 축을 하나씩 맡는다)과
> 7절(계열 넷)에서 갈라 놓은 것이다. 계열 문장은 고정하고 등급 문장만 7등급용으로 썼다.
> 들어가는 곳은 새 하위 폴더 `Resources/Art/Pets/`다.

### [x] Resources/Art/Pets/pet-t7-wheel-aurora.png — 초월 바퀴족: 오로라휠 (레이스 속도)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a large wheel with a glowing ring spinning around it, ornate plating, trailing light ribbons,
  rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(바퀴 몸통 + 허브의 큰 눈)은 바퀴족 넷이 똑같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wheel-aurora.png (커밋 deb2fe6)

### [x] Resources/Art/Pets/pet-t7-wheel-blaze.png — 초월 바퀴족: 블레이즈휠 (부스트)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a large wheel with a glowing ring spinning around it, ornate plating, twin exhaust vents flaring at the sides,
  rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep every warm tone strictly in the orange/vermilion range — no pink, no magenta, no purple tints anywhere, including highlights and edges.
- 참고: 투명 PNG로 받는다. 위 마지막 줄이 붉은 계열용 금지 줄이다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wheel-blaze.png (커밋 deb2fe6)

### [x] Resources/Art/Pets/pet-t7-wheel-ember.png — 초월 바퀴족: 엠버휠 (연료 회복)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a large wheel with a glowing ring spinning around it, ornate plating, a small fuel cell glowing inside the hub,
  rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wheel-ember.png (커밋 deb2fe6)

### [x] Resources/Art/Pets/pet-t7-wing-lantern.png — 초월 날개족: 랜턴윙 (상자 등급)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  ornate plated wings with a glowing ring behind it, carrying a small lantern that lights its face,
  rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(짧은 날개 + 떠 있는 자세)은 날개족 넷이 똑같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wing-lantern.png (커밋 0116d3f)

### [x] Resources/Art/Pets/pet-t7-wing-prism.png — 초월 날개족: 프리즘윙 (조각 획득)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  ornate plated wings with a glowing ring behind it, a faceted prism shard orbiting its body,
  rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wing-prism.png (커밋 0116d3f)

### [x] Resources/Art/Pets/pet-t7-wing-herald.png — 초월 날개족: 헤럴드윙 (광고 보상)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  ornate plated wings with a glowing ring behind it, a long ribbon banner streaming from its tail,
  rendered with #1238A8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wing-herald.png (커밋 0116d3f)

### [x] Resources/Art/Pets/pet-t7-ore-geode.png — 초월 광석족: 지오드코어 (채굴 산출)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a tall ornate geode split open to show a glowing core, small shards orbiting it,
  rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(결정 덩어리에 눈)은 광석족 셋이 똑같이 쓴다
- 참고 추가: 첫 판이 마젠타 잔상 886픽셀로 걸렸다(결정이 보랏빛으로 나왔다). 프롬프트 끝에
  `Keep the crystal strictly pale white and cool grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere, including the inner glow, highlights and edges.`
  를 붙여 다시 뽑으니 통과했다. 흰빛 결정도 붉은 계열과 똑같이 이 줄이 필요하다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-ore-geode.png (커밋 caa8562)

### [x] Resources/Art/Pets/pet-t7-ore-crucible.png — 초월 광석족: 크루시블코어 (정제 속도)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a tall ornate crystal body with a molten refining chamber glowing in its chest,
  rendered with #BF0F29 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep every red strictly in the crimson/scarlet range — no pink, no magenta, no purple tints anywhere, including highlights and edges.
- 참고: 투명 PNG로 받는다. 붉은 계열이라 planet-ruby에서 쓴 분홍·마젠타 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-ore-crucible.png (커밋 268dbaa)

### [x] Resources/Art/Pets/pet-t7-carrier-vault.png — 초월 짐꾼족: 볼트캐리어 (화물칸)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  an ornate reinforced vault container on its back with a glowing seal, heavy plated legs,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(통통한 몸 + 등의 짐칸)은 짐꾼족 셋이 똑같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-carrier-vault.png (커밋 268dbaa)

### [x] Resources/Art/Pets/pet-t7-carrier-hearth.png — 초월 짐꾼족: 하스캐리어 (오프라인 상한)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  an ornate domed shelter on its back with a warm glow inside, curled up as if resting,
  rendered with #1C298C as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-carrier-hearth.png (커밋 268dbaa)

### [x] Resources/Art/Cutscenes/opening.png — 오프닝 — 쿼츠 행성 착륙
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A small mining ship descending toward a pale crystalline planet surface at dawn, landing struts extended, dust kicked up below. Dominant color #E6E6F0.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/opening.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-ruby.png — 도착 — 루비
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A lone mining rig cresting a ridge above glowing lava-veined red crystal cliffs. Dominant color #BF0F29.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-ruby.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-sapphire.png — 도착 — 사파이어
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig on a vast frozen plain of blue crystal spires, frost drifting low across the ground. Dominant color #1238A8.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-sapphire.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-aquamarine.png — 도착 — 아쿠아마린
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig at the edge of a wide teal liquid lake reflecting rounded crystal reefs. Dominant color #59D9CC.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-aquamarine.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-cinnabar.png — 도착 — 주사
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig pushing through drifting orange toxic mist over rough vermilion terrain. Dominant color #D94D0F.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-cinnabar.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-lapis.png — 도착 — 라피스 라줄리
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig anchored to deep indigo rock while broken fragments drift upward in low gravity. Dominant color #1C298C.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-lapis.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/first-race-win.png — 첫 레이스 우승
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A racing buggy crossing a finish marker on a crystal plain, dust trail behind, other racers distant. Dominant color #E6E6F0.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/first-race-win.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Icons/icon-key.png — 재화: 상자 열쇠
- 크기: 512x512 정사각
- 용도: 상자 개봉 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a small ornate key with a crystal bow, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-key.png (커밋 72daba5, 재생성 2032cbb)

### [x] Resources/Art/Icons/icon-blueprint.png — 재화: 청사진
- 크기: 512x512 정사각
- 용도: 제작 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a rolled technical blueprint scroll with faint grid lines, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-blueprint.png (커밋 72daba5)

### [x] Resources/Art/Icons/icon-fuel.png — 재화: 연료
- 크기: 512x512 정사각
- 용도: 레이스 출전 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a sealed fuel canister with a glowing level window, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-fuel.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-box-rusty.png — 공구 상자: 녹슨
- 크기: 512x512 정사각
- 용도: 상자 개봉·레이스 보상
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a battered rusty metal toolbox, closed, worn edges, rendered with #8C6A4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-box-rusty.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-box-steel.png — 공구 상자: 강철
- 크기: 512x512 정사각
- 용도: 상자 개봉·레이스 보상
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a clean steel toolbox, closed, riveted panels, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-box-steel.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-box-titanium.png — 공구 상자: 티타늄
- 크기: 512x512 정사각
- 용도: 상자 개봉·레이스 보상
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a sleek titanium case, closed, subtle blue sheen, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-box-titanium.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-grade-c.png — 등급 배지: C
- 크기: 512x512 정사각
- 용도: 부품 등급 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a simple shield badge with one notch, plain finish, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-c.png (커밋 a5d9ee4)

### [x] Resources/Art/Icons/icon-grade-b.png — 등급 배지: B
- 크기: 512x512 정사각
- 용도: 부품 등급 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a shield badge with two notches, polished finish, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-b.png (커밋 a5d9ee4)

### [x] Resources/Art/Icons/icon-grade-a.png — 등급 배지: A
- 크기: 512x512 정사각
- 용도: 부품 등급 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a shield badge with three notches and a small gem inset, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-a.png (커밋 a5d9ee4)

### [x] Resources/Art/Icons/icon-grade-s.png — 등급 배지: S
- 크기: 512x512 정사각
- 용도: 부품 등급 표시. 네 개 중 가장 화려하게
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of an ornate shield badge with a radiant gem centerpiece, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-s.png (커밋 a5d9ee4)

### [x] Resources/Art/Planets/planet-quartz.png — 쿼츠 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 쿼츠 crystal formations,
  dominant color #E6E6F0, smooth pale surface with scattered clear prismatic shards,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-quartz.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-sapphire.png — 사파이어 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 사파이어 crystal formations,
  dominant color #1238A8, frozen blue crystal plains with frost haze,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-sapphire.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-aquamarine.png — 아쿠아마린 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 아쿠아마린 crystal formations,
  dominant color #59D9CC, shallow teal liquid pools between rounded crystal reefs,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-aquamarine.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-cinnabar.png — 주사 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 주사 crystal formations,
  dominant color #D94D0F, rough vermilion terrain with drifting toxic orange mist,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-cinnabar.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-lapis.png — 라피스 라줄리 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 라피스 라줄리 crystal formations,
  dominant color #1C298C, deep indigo rock with fragments floating off the surface in low gravity,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-lapis.png (커밋 7fddc92)

### [x] Resources/Art/Rigs/rig-tiers-sheet.png — 채굴차 3티어 (한 장에 세 대)
- 크기: 1536x1024 가로
- 용도: 채굴차 티어 외형. 받은 뒤 셋으로 잘라 쓴다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Three versions of the same small six-wheeled mining rover shown side by side in a row,
  left to right: (1) basic, with a simple pickaxe arm, worn grey panels;
  (2) upgraded, with a rotary drill arm and reinforced plating;
  (3) advanced, with a sleek laser cutter arm and glowing #708CFF accents.
  Same scale, same angle, same lighting for all three. Fully transparent background — real alpha channel, no background color, no checkerboard.
- 참고: 한 장에 세 대를 그려야 통일감이 산다. 따로 뽑으면 셋이 다른 차가 된다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Rigs/rig-tiers-sheet.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-part-engine.png — 부품: 엔진
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a compact futuristic vehicle engine block with glowing intake vents, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-engine.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-part-tire.png — 부품: 타이어
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a chunky off-road vehicle tire seen at a three-quarter angle, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-tire.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-part-suspension.png — 부품: 서스펜션
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a coil-over suspension strut with a spring, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-suspension.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-gear-tool.png — 채굴 장비: 도구
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a mining drill bit with a faceted crystal tip, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-gear-tool.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-gear-cargo.png — 채굴 장비: 화물칸
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비 + 화물칸 게이지 옆
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of an open cargo container half filled with rough crystal ore, rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-gear-cargo.png (커밋 94625d6)

### [x] Resources/Art/Icons/icon-gear-engine.png — 채굴 장비: 엔진
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a rugged tracked-vehicle drive unit with a single piston, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-gear-engine.png (커밋 94625d6)

### [x] Resources/Art/Icons/icon-raw-mineral.png — 재화: 원석
- 크기: 512x512 정사각
- 용도: HUD 상단 원석 숫자 옆, 모든 보상 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a cluster of three rough uncut crystal shards, rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-raw-mineral.png (커밋 94625d6)

### [x] Resources/Art/Icons/icon-refined-mineral.png — 재화: 정제 광물
- 크기: 512x512 정사각
- 용도: 제련소·상점. 원석과 한눈에 구분되어야 한다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a single polished faceted gem cut into a clean hexagon, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-refined-mineral.png (커밋 94625d6)

### [x] Resources/Art/Planets/planet-ruby.png — 루비 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 루비 crystal formations,
  dominant color #BF0F29, cracked lava veins glowing faintly between jagged red crystals,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 참고 추가: 첫 판이 마젠타 잔상 59픽셀로 검사에 걸렸다. 프롬프트 끝에
  `Keep every red strictly in the crimson/scarlet range — no pink, no magenta, no purple tints anywhere, including highlights and edges.`
  를 붙여 다시 뽑으니 통과했다. 붉은 계열 행성을 다시 뽑을 땐 이 줄을 같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-ruby.png (커밋 246dacc)

### 다시 뽑아 달라고 하는 법

마음에 안 드는 그림이 있으면, 여기 그 항목 아래에 이렇게 한 줄만 붙이면 된다.

    - [!] 다시: 색이 너무 어둡다. 더 밝게, 그리고 드릴 날을 더 크게

다음 이미지 세션이 **대기 중인 새 요청보다 이걸 먼저** 처리한다.
원래 프롬프트 뒤에 그 지시를 덧붙여 새로 뽑고 파일을 덮어쓴 뒤, `- [!]` 줄을 지운다.
여러 번 반복해도 된다. 마음에 들 때까지 붙이면 된다.

### [x] Resources/Art/Pets/6-myth/ore-01.png — 신화 — 결정 대장장이 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/ore-01.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-01.png — 신화 — 강철 등짐꾼 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-01.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-02.png — 신화 — 심해 운반자 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-02.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-03.png — 신화 — 중력 포터 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-03.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-04.png — 신화 — 보급의 어머니 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-04.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-05.png — 신화 — 이동 창고 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-05.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-06.png — 신화 — 마지막 짐꾼 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-06.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/haul-07.png — 신화 — 느린 거인 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-07.png (커밋 41224a4)

### [x] Resources/Art/Pets/6-myth/wheel-03.png — 신화 — 분쇄의 무희 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-03.png (커밋 94ad7d6)

### [x] Resources/Art/Pets/6-myth/wing-02.png — 신화 — 유성 전령 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-02.png (커밋 94ad7d6)

### [x] Resources/Art/Pets/6-myth/wing-05.png — 신화 — 월광 활공자 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-05.png (커밋 94ad7d6)

### [x] Resources/Art/Pets/6-myth/wing-06.png — 신화 — 전파 나그네 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-06.png (커밋 94ad7d6)

### [x] Resources/Art/Pets/6-myth/wing-07.png — 신화 — 섬광 매 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-07.png (커밋 94ad7d6)

### [x] Resources/Art/Pets/6-myth/wing-08.png — 신화 — 고요의 감시자 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-08.png (커밋 94ad7d6)

### [x] Resources/Art/Pets/5-legend/wheel-quartz.png — 전설 — 바퀴족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-quartz.png (커밋 14f3628)

### [x] Resources/Art/Pets/5-legend/wheel-ruby.png — 전설 — 바퀴족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-ruby.png (커밋 14f3628)

### [x] Resources/Art/Pets/5-legend/wheel-sapphire.png — 전설 — 바퀴족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-sapphire.png (커밋 14f3628)

### [x] Resources/Art/Pets/5-legend/wheel-aquamarine.png — 전설 — 바퀴족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-aquamarine.png (커밋 14f3628)

### [x] Resources/Art/Pets/5-legend/wheel-cinnabar.png — 전설 — 바퀴족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-cinnabar.png (커밋 14f3628)

### [x] Resources/Art/Pets/5-legend/wing-quartz.png — 전설 — 날개족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-quartz.png (커밋 8303c46)

### [x] Resources/Art/Pets/5-legend/wing-ruby.png — 전설 — 날개족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-ruby.png (커밋 8303c46)

### [x] Resources/Art/Pets/5-legend/wing-sapphire.png — 전설 — 날개족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-sapphire.png (커밋 8303c46)

### [x] Resources/Art/Pets/5-legend/wing-aquamarine.png — 전설 — 날개족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-aquamarine.png (커밋 8303c46)

### [x] Resources/Art/Pets/5-legend/wing-cinnabar.png — 전설 — 날개족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-cinnabar.png (커밋 8303c46)

### [x] Resources/Art/Pets/5-legend/ore-quartz.png — 전설 — 광석족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-quartz.png (커밋 656e9d4)

### [x] Resources/Art/Pets/5-legend/ore-ruby.png — 전설 — 광석족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-ruby.png (커밋 656e9d4)

### [x] Resources/Art/Pets/5-legend/ore-sapphire.png — 전설 — 광석족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-sapphire.png (커밋 656e9d4)

### [x] Resources/Art/Pets/5-legend/ore-aquamarine.png — 전설 — 광석족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-aquamarine.png (커밋 de03aad)

### [x] Resources/Art/Pets/5-legend/ore-cinnabar.png — 전설 — 광석족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-cinnabar.png (커밋 de03aad)

### [x] Resources/Art/Pets/5-legend/haul-quartz.png — 전설 — 짐꾼족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/haul-quartz.png (커밋 8e8e528)

### [x] Resources/Art/Pets/5-legend/haul-ruby.png — 전설 — 짐꾼족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/haul-ruby.png (커밋 8e8e528)

### [x] Resources/Art/Pets/5-legend/haul-sapphire.png — 전설 — 짐꾼족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/haul-sapphire.png (커밋 8e8e528)

### [x] Resources/Art/Pets/5-legend/haul-aquamarine.png — 전설 — 짐꾼족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/haul-aquamarine.png (커밋 8e8e528)

### [x] Resources/Art/Pets/5-legend/haul-cinnabar.png — 전설 — 짐꾼족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/haul-cinnabar.png (커밋 8e8e528)

### [x] Resources/Art/Pets/2-base/wheel.png — 고급 골격 — 바퀴족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/2-base/wheel.png (커밋 9feef12)

### [x] Resources/Art/Pets/2-base/wing.png — 고급 골격 — 날개족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/2-base/wing.png (커밋 9feef12)

### [x] Resources/Art/Pets/2-base/ore.png — 고급 골격 — 광석족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/2-base/ore.png (커밋 9feef12)

### [x] Resources/Art/Pets/2-base/haul.png — 고급 골격 — 짐꾼족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/2-base/haul.png (커밋 5f17677)

### [x] Resources/Art/Pets/3-base/wheel.png — 희귀 골격 — 바퀴족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/3-base/wheel.png (커밋 5f17677)

### [x] Resources/Art/Pets/3-base/wing.png — 희귀 골격 — 날개족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/3-base/wing.png (커밋 5f17677)

### [x] Resources/Art/Pets/3-base/ore.png — 희귀 골격 — 광석족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/3-base/ore.png (커밋 5f17677)

### [x] Resources/Art/Pets/3-base/haul.png — 희귀 골격 — 짐꾼족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/3-base/haul.png (커밋 5f17677)

### [x] Resources/Art/Pets/4-base/wheel.png — 영웅 골격 — 바퀴족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/4-base/wheel.png (커밋 6f3c564)

### [x] Resources/Art/Pets/4-base/wing.png — 영웅 골격 — 날개족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/4-base/wing.png (커밋 6f3c564)

### [x] Resources/Art/Pets/4-base/ore.png — 영웅 골격 — 광석족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/4-base/ore.png (커밋 6f3c564)

### [x] Resources/Art/Pets/4-base/haul.png — 영웅 골격 — 짐꾼족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/4-base/haul.png (커밋 6f3c564)
