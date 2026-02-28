# Unity 버전 전환 가이드

이 폴더는 기존 Python(Pygame) 자동차 게임을 Unity 2D로 옮기기 위한 기본 구현입니다.

## 포함 기능
- 좌우 이동 (`←`,`→`)
- 부스트 (`Left Shift`)
- 장애물 스폰 + 시간 경과 난이도 증가
- 실드 아이템 획득 및 1회 충돌 무효화
- 점수/최고점수 표시 (`PlayerPrefs` 저장)
- 게임 오버 + `R` 재시작

## Unity 설정 절차 (권장: Unity 2022.3 LTS 이상)

1. Unity Hub에서 **New project → 2D Core** 생성
2. 생성된 프로젝트의 `Assets/Scripts` 폴더에 이 저장소의 `UnityCarRacing/Assets/Scripts/*.cs` 복사
3. 씬 구성
   - `Player` 오브젝트 생성 (SpriteRenderer + Rigidbody2D(Kinematic) + BoxCollider2D(Is Trigger))
   - Tag를 `Player`로 지정
   - `PlayerCarController` 부착
4. 장애물 프리팹
   - `Obstacle` 프리팹 생성 (SpriteRenderer + BoxCollider2D(Is Trigger))
   - Tag를 `Obstacle`로 지정
   - `ObstacleMover` 부착
5. 실드 프리팹
   - `Shield` 프리팹 생성 (SpriteRenderer + CircleCollider2D(Is Trigger))
   - `ShieldPickup` 부착
6. GameObject `GameManager` 생성
   - `GameManager`, `ObstacleSpawner`, `UIController` 부착
   - `ObstacleSpawner`의 프리팹 슬롯에 `Obstacle`, `Shield` 연결
   - `GameManager`의 player/spawner/ui 연결
7. UI 구성 (Canvas)
   - TMP Text 3개: Score, High Score, Shield
   - 게임오버 패널 1개 (기본 비활성)
   - `UIController` 슬롯 연결
8. Play 실행

## 매핑 참고 (Python → Unity)
- `core/game.py`의 루프/상태 관리 → `GameManager.cs`
- 플레이어 이동/부스트 → `PlayerCarController.cs`
- 장애물/실드 생성 → `ObstacleSpawner.cs`
- 충돌 후 처리 → `GameManager.HandleObstacleCollision()`

## 참고
- 본 폴더는 Unity 메타파일(`.meta`) 없이 스크립트 중심으로 제공됩니다.
- 실제 프로젝트에서 프리팹/씬 구성은 Unity Editor에서 완료해야 합니다.
