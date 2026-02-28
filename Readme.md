# Python Car Racing Game (Advanced)

Pygame 기반 2D 자동차 레이싱 게임입니다.  
이번 버전에서는 차량 스탯/트랙 테마/적 AI/미션/모바일 입력까지 포함된 고도화 버전으로 업데이트했습니다.

---

## 1) 고도화 기능

- **차량 선택 시스템 (1/2/3 키)**
  - Sprint: 높은 가속, 안정적 핸들링
  - Interceptor: 최고속 특화
  - Drift: 핸들링 특화
  - 각 차량은 **가속/최고속/핸들링** 스탯이 다릅니다.

- **트랙 테마 3종**
  - 도심
  - 사막
  - 야간 네온
  - 라운드 시작 시 랜덤으로 테마가 적용됩니다.

- **적 차량 AI**
  - 다수 적 차량이 등장
  - AI가 주기적으로 **차선 변경**
  - 플레이어 근접 시 **블로킹(플레이어 차선 추적)** 행동 수행

- **미션 시스템**
  - 예: 20초 생존 / 25초 노부스트
  - 성공 시 보너스 점수 획득
  - 노부스트 미션 도중 부스트 사용 시 미션 실패 처리

- **모바일 대응 입력**
  - 화면 하단 **좌/우/부스트 버튼**
  - **스와이프 입력**으로 차선 이동 보조
  - PC에서도 마우스로 동일 UI 테스트 가능

- 기존 기능 유지
  - 실드 아이템(1회 충돌 무효)
  - 부스트 쿨다운
  - 최고 점수 저장(`highscore.json`)

---

## 2) 설치

### 요구사항
- Python 3.10+
- pygame

### 가상환경(권장)

#### macOS / Linux
```bash
python3 -m venv .venv
source .venv/bin/activate
```

#### Windows (PowerShell)
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

### 의존성 설치
```bash
pip install pygame
```

---

## 3) 실행 방법

```bash
python car_race.py
```

### 조작법
- `SPACE`: 게임 시작
- `1 / 2 / 3`: 차량 선택 (시작 화면)
- `← / →`: 이동
- `Left Shift`: 부스트
- `P`: 일시정지
- `Y / N`: 게임오버 후 재시작/종료
- 모바일/터치: 하단 버튼 + 스와이프

---

## 4) 빌드(실행 파일 만들기)

배포용 단일 실행 파일은 `pyinstaller`로 만들 수 있습니다.

```bash
pip install pyinstaller
pyinstaller --onefile --noconsole car_race.py
```

빌드 완료 후:
- `dist/car_race` (Linux/macOS)
- `dist/car_race.exe` (Windows)

> 사운드/이미지 리소스를 함께 배포하려면 PyInstaller spec 또는 `--add-data` 옵션을 추가해 주세요.

예시(Windows):
```powershell
pyinstaller --onefile --noconsole car_race.py --add-data "car.png;." --add-data "obstacle.png;." --add-data "crash.wav;."
```

---

## 5) 프로젝트 구조

```text
.
├── car_race.py
├── config.py
├── core/
│   └── game.py
├── systems/
│   ├── resource_loader.py
│   └── save_system.py
├── car.png
├── obstacle.png
├── crash.wav
└── highscore.json
```

