# UTF-8 파일 끝을 읽어 준다 (2026-09-29). PowerShell/cmd는 한글 출력에서 코드페이지 때문에
# 줄을 깨먹거나 findstr이 0건처럼 보이게 만든다 — 파이썬으로 읽는 쪽이 유일하게 믿을 만하다.
#
#   python tools/tail_utf8.py <파일> [글자수]
import io
import sys

path = sys.argv[1]
n = int(sys.argv[2]) if len(sys.argv) > 2 else 800
text = io.open(path, encoding="utf-8", errors="replace").read()[-n:]
# 콘솔이 cp949라 '—' 같은 글자에서 죽는다. 표준출력을 UTF-8로 갈아 끼운다.
out = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
out.write(text + "\n")
out.flush()
