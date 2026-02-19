# Python Car Racing Game

간단한 **2D 레이싱 게임(Pygame)** 과 **3D 차량 뷰어(Ursina + Panda3D)** 를 함께 실험하는 프로젝트입니다.

## 프로젝트 구성
- `car_race.py`: 2D 자동차 레이싱 게임(장애물 회피, 점수, 재시작).
- `car_race_3d.py`: GLB 차량 모델을 불러와 오빗 카메라로 확인하는 3D 뷰어.
- `car.png`, `obstacle.png`, `crash.wav` 등: 2D 게임 에셋.
- `car.svg`, `obstacle_cone.svg`: 벡터 소스 에셋.
- `test.mp4`: 플레이/결과 샘플 영상.

## 기술 스택 (상세)

### 1) Language / Runtime
- **Python 3.x**
  - 게임 루프, 충돌 처리, 입력 처리, 렌더링 제어를 Python 코드로 작성.

### 2) 2D 게임 스택 (`car_race.py`)
- **Pygame**
  - `pygame.display`: 게임 윈도우 생성 및 화면 업데이트.
  - `pygame.image`: 차량/장애물 이미지 로드 및 스케일 조정.
  - `pygame.mixer`: 배경음악(BGM) 및 충돌 효과음 재생.
  - `pygame.event` / `pygame.key`: 키 입력 및 이벤트 루프 처리.
  - `pygame.Rect`: 충돌 판정(자동차와 장애물 교차 여부).
- **Python 표준 라이브러리**
  - `random`: 장애물 랜덤 위치 생성.
  - `os`: 리소스 파일 존재 여부 체크.
  - `sys`: 종료 처리.

### 3) 3D 뷰어 스택 (`car_race_3d.py`)
- **Ursina Engine**
  - 빠른 3D 장면 구성(`Entity`, `Text`, `camera`, `update`, `input` 등).
- **Panda3D (Ursina 내부 렌더 기반 활용)**
  - `loadPrcFileData`: OpenGL/AA/sRGB 등 렌더 옵션 설정.
  - `Loader`, `NodePath`, `Filename`: `.glb` 모델 로드 및 씬 그래프 배치.
  - `DirectionalLight`, `AmbientLight`: 조명 세팅.
- **GLB 3D Asset Pipeline**
  - `models/car.glb`를 불러와 차량 스케일 자동 보정 및 카메라 오빗 뷰 제공.

### 4) 에셋/미디어
- **PNG/SVG**: 2D 스프라이트 및 원본 디자인 자산.
- **WAV/MP3(선택)**: 효과음 및 BGM.
- **MP4**: 데모 영상.

## 실행 방법

### 2D 게임 실행
```bash
python car_race.py
```

### 3D 뷰어 실행
```bash
python car_race_3d.py
```
> `car_race_3d.py`는 `models/car.glb` 파일이 필요합니다.

## 권장 개발 환경
- Python 3.10+
- 가상환경(venv) 사용 권장

예시:
```bash
python -m venv .venv
source .venv/bin/activate
pip install pygame ursina panda3d
```

---

## To Do (버전업 고도화 목표)

### v0.x → v1.0 (게임성/구조 안정화)
- [ ] 설정 분리: 해상도, FPS, 속도, 난이도 계수를 `config` 파일로 분리.
- [ ] 코드 구조화: `core/`, `systems/`, `assets/` 폴더 구조로 리팩터링.
- [ ] 씬 관리 도입: 시작 화면 / 플레이 / 일시정지 / 게임오버 상태 머신 적용.
- [ ] 리소스 로더 도입: 경로 관리와 누락 에셋 처리 일원화.

### v1.0 → v1.5 (콘텐츠 확장)
- [ ] 차량/장애물 종류 확장(속성 차등: 크기, 속도, 충돌 판정).
- [ ] 아이템 시스템(부스트, 쉴드, 슬로우 등) 추가.
- [ ] 난이도 곡선 고도화(시간 기반 + 점수 기반 혼합 스케일링).
- [ ] HUD 개선(콤보, 최고기록, 실시간 속도 표시).

### v1.5 → v2.0 (품질/배포)
- [ ] 입력 확장: 게임패드 및 키 리맵 지원.
- [ ] 사운드 시스템 고도화: 채널 분리(BGM/SFX), 볼륨 옵션 UI.
- [ ] 저장 시스템: 로컬 최고 점수/설정 저장(JSON).
- [ ] 테스트 추가: 핵심 로직(충돌/스코어/난이도) 단위 테스트.
- [ ] CI 구성: lint + test 자동 실행.
- [ ] 패키징: PyInstaller 기반 배포본(Windows/macOS/Linux).

### 3D 모드 고도화 (병행 트랙)
- [ ] 3D 주행 프로토타입(차량 이동 + 코스 + 충돌체) 구현.
- [ ] 카메라 프리셋(추적/탑뷰/콕핏) 전환 기능.
- [ ] PBR 머티리얼/환경맵 적용으로 비주얼 품질 향상.
- [ ] 성능 프로파일링(FPS/드로우콜) 및 옵션 프리셋(LOW/MID/HIGH) 제공.

원하면 다음 단계로 위 To Do 중 **우선순위(빠른 체감 개선 기준)** 를 정해서 이슈 템플릿까지 같이 정리할 수 있습니다.
