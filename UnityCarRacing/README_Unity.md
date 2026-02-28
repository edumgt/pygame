# Unity 기반 고품질 카레이싱 게임 제작/사용 가이드

이 문서는 현재 저장소의 `UnityCarRacing/Assets/Scripts` 코드를 기반으로,
**"바로 실행 가능한 2D 카레이싱 게임"**을 Unity에서 구성하고,
시각/사운드/카메라 연출을 더해 **고품질 느낌**으로 확장하는 방법까지 단계별로 안내합니다.

---

## 1) 구현된 핵심 시스템 개요

현재 스크립트는 아래 게임 루프를 이미 포함합니다.

- 플레이어 좌우 이동 + 부스트
- 장애물 지속 생성, 시간 경과에 따라 난이도 상승
- 실드 아이템 획득(1회 충돌 무효)
- 점수/최고 점수(`PlayerPrefs`) 저장
- 게임오버 + `R` 키 재시작

### 스크립트 역할 맵

- `GameManager.cs`
  - 점수, 최고점수, 게임 상태(시작/게임오버/재시작), 실드 상태 관리
- `PlayerCarController.cs`
  - 좌우 입력 처리, 부스트 지속시간/쿨다운, 플레이어 충돌 감지
- `ObstacleSpawner.cs`
  - 장애물/실드 생성 주기 관리, 장애물 속도 점진 증가
- `ObstacleMover.cs`
  - 장애물 하강 이동, 화면 이탈 시 점수 추가 후 제거
- `ShieldPickup.cs`
  - 실드 아이템 하강, 플레이어 충돌 시 실드 획득
- `UIController.cs`
  - 점수/최고점수/실드 텍스트 및 게임오버 패널 표시

---

## 2) Unity 프로젝트 생성 및 기본 세팅

권장 버전: **Unity 2022.3 LTS 이상**

1. Unity Hub → **New Project** → **2D Core**
2. 프로젝트 생성 후 `Assets/Scripts` 폴더 생성
3. 이 저장소의 `UnityCarRacing/Assets/Scripts/*.cs`를 프로젝트 `Assets/Scripts`로 복사
4. 패키지 매니저에서 **TextMeshPro**가 설치되어 있는지 확인

> 팁: 해상도는 `1920 x 1080` 기준으로 맞추면 UI 배치가 안정적입니다.

---

## 3) 씬 구성 (실행 가능 상태까지)

아래 순서대로 하면 바로 플레이 가능한 상태가 됩니다.

### A. Player 오브젝트

1. 빈 오브젝트 생성 후 이름 `Player`
2. `SpriteRenderer` 추가 (차량 스프라이트 지정)
3. `Rigidbody2D` 추가
   - Body Type: **Kinematic**
   - Gravity Scale: `0`
4. `BoxCollider2D` 추가
   - `Is Trigger` 체크
5. Tag를 `Player`로 설정
6. `PlayerCarController` 컴포넌트 부착
7. Inspector 파라미터 예시
   - Normal Speed: `6`
   - Boost Speed: `10`
   - Boost Duration: `0.75`
   - Boost Cooldown: `4`
   - Min X: `-2.2`, Max X: `2.2`

### B. Obstacle 프리팹

1. `Obstacle` 오브젝트 생성
2. `SpriteRenderer` + `BoxCollider2D (Is Trigger)` 추가
3. Tag를 `Obstacle`로 설정
4. `ObstacleMover` 부착
5. 프리팹으로 저장 (`Assets/Prefabs/Obstacle.prefab` 권장)

### C. Shield 프리팹

1. `Shield` 오브젝트 생성
2. `SpriteRenderer` + `CircleCollider2D (Is Trigger)` 추가
3. `ShieldPickup` 부착
4. 프리팹으로 저장 (`Assets/Prefabs/Shield.prefab` 권장)

### D. GameManager 오브젝트

1. 빈 오브젝트 생성 후 이름 `GameManager`
2. `GameManager`, `ObstacleSpawner`, `UIController` 컴포넌트 부착
3. `ObstacleSpawner` 슬롯 연결
   - Obstacle Prefab → `Obstacle` 프리팹
   - Shield Prefab → `Shield` 프리팹
4. `GameManager` 슬롯 연결
   - Player → `Player`
   - Obstacle Spawner → 같은 오브젝트의 `ObstacleSpawner`
   - UI Controller → 같은 오브젝트의 `UIController`

### E. UI(Canvas)

1. Canvas 생성 (Screen Space - Overlay)
2. TextMeshPro 텍스트 3개 생성
   - `ScoreText`
   - `HighScoreText`
   - `ShieldText`
3. 게임오버 패널 생성 (`GameOverPanel`, 기본 비활성)
4. `UIController` 슬롯에 각각 연결

---

## 4) 플레이 방법 (사용법)

- **좌우 이동**: `←` / `→`
- **부스트**: `Left Shift`
  - 짧게 고속 이동 가능
  - 사용 후 쿨다운이 돌아야 재사용 가능
- **실드 획득**: 떨어지는 실드 아이템과 충돌
  - 다음 장애물 충돌 1회 무효화
- **게임오버 후 재시작**: `R`

점수는 시간 경과 + 장애물 회피(화면 아래로 통과)로 증가합니다.
최고점수는 로컬에 자동 저장됩니다.

---

## 5) 고품질(High-Quality) 느낌으로 끌어올리는 설정 가이드

현재 코어 로직은 가볍고 안정적이므로, 아래 요소를 더하면 품질이 크게 올라갑니다.

### 5-1. 비주얼 품질

- **배경 레이어 파랄랙스**
  - 도로/가드레일/도시 배경을 2~3 레이어로 분리
  - 레이어별 스크롤 속도를 다르게 하여 속도감 강화
- **포스트 프로세싱(2D URP)**
  - Bloom(헤드라이트, 네온)
  - Color Adjustments(대비/채도)
  - Vignette(집중감)
- **파티클 효과**
  - 부스트 시 배기 불꽃/스피드 라인
  - 충돌 시 스파크
  - 실드 획득 시 링/글로우

### 5-2. 조작감 품질

- **카메라 흔들림(Cinemachine Impulse)**
  - 충돌/근접 회피 순간 짧은 흔들림
- **입력 반응 개선**
  - 이동 시 아주 짧은 가속/감속 커브 적용
  - 차체 좌우 기울기(롤) 시각 피드백 추가
- **부스트 피드백**
  - FOV 확대, 엔진 피치 상승, UI 글로우

### 5-3. 오디오 품질

- BGM 1트랙 + 상황별 SFX 분리
  - 엔진 루프
  - 부스트 발동
  - 충돌
  - 실드 획득
  - 게임오버
- AudioMixer 그룹 분리
  - Master / BGM / SFX 볼륨 옵션 제공

### 5-4. UX 품질

- 메인 메뉴 / 일시정지 / 결과 화면 분리
- 해상도/볼륨/입력 키 설정 메뉴
- 최고 점수 외 추가 지표
  - 생존 시간
  - 근접 회피 보너스
  - 완벽 주행(무충돌) 보상

---

## 6) 밸런싱 추천값 (초기 튜닝 프리셋)

초반은 쉽고, 30초 이후부터 긴장감이 오르는 값입니다.

- `ObstacleSpawner`
  - Obstacle Spawn Interval: `1.3`
  - Shield Spawn Interval: `6.0`
  - Initial Obstacle Speed: `4.0`
  - Speed Increase: `0.15`
- `PlayerCarController`
  - Normal Speed: `6.0`
  - Boost Speed: `10.0`
  - Boost Duration: `0.75`
  - Boost Cooldown: `4.0`

어렵다면
- Spawn Interval을 `1.4~1.6`으로 증가,
- Speed Increase를 `0.10` 근처로 감소시키세요.

---

## 7) 빌드 및 배포 체크리스트

1. `File > Build Settings`에서 현재 씬을 `Scenes In Build`에 추가
2. 플랫폼 선택 (Windows/Mac/Linux/WebGL)
3. `Project Settings > Player`에서
   - Company/Product Name 지정
   - 아이콘/해상도 정책 설정
4. 릴리즈 전 확인
   - 게임오버/재시작 정상 동작
   - 최고점수 저장/로드 확인
   - 30분 이상 플레이 시 성능 저하 여부 확인

---

## 8) 문제 해결 (FAQ)

### Q1. 충돌이 안 나요.
- Player/Obstacle 콜라이더의 `Is Trigger` 설정 확인
- Player Tag가 `Player`, 장애물 Tag가 `Obstacle`인지 확인

### Q2. 점수가 갱신되지 않아요.
- `GameManager`의 `UIController` 레퍼런스 연결 확인
- `UIController`의 TMP 텍스트 슬롯 연결 확인

### Q3. 실드가 안 먹어요.
- 실드 프리팹에 `ShieldPickup` 부착 여부 확인
- 실드 오브젝트의 Collider2D가 Trigger인지 확인

### Q4. 최고점수가 저장 안 돼요.
- 에디터 Play 모드 종료 전에 강제 종료하지 않았는지 확인
- `PlayerPrefs` 저장은 플랫폼별 로컬 저장소를 사용

---

## 9) Python 버전에서 Unity 버전으로의 전환 포인트

- 루프/상태 전환: `core/game.py` → `GameManager.cs`
- 플레이어 이동/부스트: Python 입력 처리 → `PlayerCarController.cs`
- 장애물/아이템 스폰: Python 객체 생성 → `ObstacleSpawner.cs`
- UI 렌더링: pygame draw → Unity Canvas + `UIController.cs`

Unity에서는 프레임 루프를 직접 작성하기보다,
컴포넌트 단위(`Update`, `OnTriggerEnter2D`)로 책임을 분리하는 것이 핵심입니다.

---

## 10) 다음 확장 로드맵 (권장)

1. **차량 선택 시스템** (가속/최고속/핸들링 스탯 차별화)
2. **트랙 테마 3종** (도심/사막/야간 네온)
3. **적 차량 AI** (차선 변경, 블로킹 패턴)
4. **미션 시스템** (n초 생존, 노부스트 클리어 등)
5. **모바일 대응** (터치 스와이프/버튼 입력)

이 순서대로 확장하면 적은 리스크로 완성도를 빠르게 높일 수 있습니다.
