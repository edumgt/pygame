# Python Car Racing Game

Pygame 기반의 2D 자동차 레이싱 게임과 Ursina 기반 3D 차량 뷰어를 함께 포함한 프로젝트입니다.

---

## 1) 이번 고도화 내용

2D 게임(`car_race.py`) 기준으로 아래 기능을 추가했습니다.

- **부스트 시스템**
  - `Left Shift`로 순간 가속 사용
  - 지속 시간/쿨다운을 설정값으로 관리
- **실드 아이템 시스템**
  - 일정 주기마다 실드 아이템 생성
  - 획득 시 1회 충돌 무효화
- **HUD 강화**
  - 현재 점수, 최고 점수, 부스트 상태(READY/쿨다운) 표시
- **최고 점수 저장**
  - 게임 오버 시 `highscore.json`에 최고 점수 자동 저장
  - 재실행 후 최고 점수 유지
- **시간 기반 점수 증가**
  - 장애물 회피 점수 외에도 생존 시간 기반 점수 누적

---

## 2) 프로젝트 구조

```text
.
├── car_race.py              # 2D 게임 실행 진입점
├── car_race_3d.py           # 3D 차량 뷰어
├── config.py                # 화면/밸런스/아이템/저장 설정
├── core/
│   └── game.py              # 씬/게임 루프/입력/충돌/HUD 처리
├── systems/
│   ├── resource_loader.py   # 이미지/사운드 로딩
│   └── save_system.py       # 최고 점수 저장 시스템
├── car.png
├── obstacle.png
├── crash.wav
└── Readme.md
```

---

## 3) 설치 가이드 (권장)

### 3-1. Python 버전
- **Python 3.10 이상 권장**

### 3-2. 가상환경 생성 및 활성화

#### macOS / Linux
```bash
python -m venv .venv
source .venv/bin/activate
```

#### Windows (PowerShell)
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

### 3-3. 의존성 설치
```bash
pip install pygame ursina panda3d
```

> 2D 게임만 실행할 경우 최소 `pygame`만 있어도 됩니다.

---

## 4) 실행 매뉴얼

## 4-1. 2D 게임 실행
```bash
python car_race.py
```

### 조작 방법
- `← / →`: 좌우 이동
- `Left Shift`: 부스트 사용
- `P`: 일시정지 / 해제
- `ESC`: 종료
- `SPACE`: 시작 화면에서 게임 시작
- `Y / N`: 게임 오버 후 재시작/종료

### 게임 규칙
1. 장애물을 피하면 점수가 증가합니다.
2. 게임이 진행될수록 장애물 속도가 빨라집니다.
3. 실드 아이템(파란색 타원)을 획득하면 1회 충돌이 무효화됩니다.
4. 충돌 시 게임 오버이며, 최고 점수는 자동 저장됩니다.

---

## 4-2. 3D 뷰어 실행
```bash
python car_race_3d.py
```

> `models/car.glb` 파일이 필요합니다. 파일이 없으면 뷰어가 정상 동작하지 않을 수 있습니다.

---

## 5) 설정 커스터마이징

`config.py`에서 주요 값을 조정할 수 있습니다.

- `DisplayConfig`: 해상도, FPS, 타이틀
- `BalanceConfig`: 기본 속도, 부스트, 난이도 증가율, 시간 점수 주기
- `ItemConfig`: 실드 생성 주기/크기/낙하 속도
- `SaveConfig`: 최고 점수 저장 파일 경로

예시 조정 항목:
- 더 빠른 게임: `initial_obstacle_speed`, `obstacle_speed_increase` 상승
- 부스트 남용 방지: `boost_cooldown_frames` 증가
- 실드 드랍 희소화: `shield_spawn_interval_frames` 증가

---

## 6) 트러블슈팅

### Q1. 사운드가 나오지 않아요
- 시스템 오디오 장치 상태 확인
- `crash.wav` 파일 존재 여부 확인
- 일부 환경에서는 믹서 초기화 실패 가능 → 오디오 드라이버 점검

### Q2. 최고 점수가 저장되지 않아요
- 실행 디렉터리에 `highscore.json` 생성 권한이 있는지 확인
- 파일이 깨졌다면 삭제 후 재실행

### Q3. 이미지가 보이지 않아요
- `car.png`, `obstacle.png` 누락 여부 확인
- 누락 시 대체 단색 스프라이트가 표시됩니다

---

## 7) 빠른 시작 요약

```bash
python -m venv .venv
source .venv/bin/activate  # Windows는 .\.venv\Scripts\Activate.ps1
pip install pygame
python car_race.py
```

---

원하면 다음 단계로 **아이템 다양화(슬로우/더블스코어)**, **설정 UI**, **랭킹 다중 저장(JSON 배열)** 까지 확장할 수 있습니다.
