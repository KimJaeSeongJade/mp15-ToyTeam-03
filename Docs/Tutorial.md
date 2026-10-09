# 튜토리얼 씬

`Assets/Scenes/Main Scenes/TutorialScene.unity`는 BigIsland 1의 별도 복제본이다.
본게임 씬/원본 프리팹에는 튜토리얼 컴포넌트를 추가하지 않는다.

## 진행

이동 → 일반 공격 → 스킬 사용/처치 → 첫 웨이브 골드 배달 → 기본 공격 터렛 건설 →
공격 + 공격 조합 → E로 건설 모드 종료 → 두 번째 웨이브 터렛 전투 → 캐슬 컷씬 → 완료.

각 단계 시작 시 관련 HUD를 강조하고 1.2초 후 행동을 허용한다. 다음 버튼과 준비 시간 제한은 없다.
1~4번 슬롯을 모두 표시하되 1번만 허용한다. 건설 단계에서 판매 입력은 막는다.

튜토리얼 전용 터렛 복제본은 공격 터렛 100G, 속사 터렛 200G로 설정했다.
원본 속사 터렛의 0G 가격과 누락된 총구 참조는 본게임 에셋을 수정하지 않고 복제본에서만 보완했다.
첫 웨이브 골드는 두 가격의 합을 기준으로 계산한다. 골드 배달이 끝난 후 건설로 넘어간다.

## 컴포넌트

- TutorialManager: 순서와 단계 완료 조건, ESC 확인, 완료 저장.
- TutorialInputFilter: 튜토리얼 플레이어에만 부착하는 입력 제한.
- TutorialGoldDelivery: WaveManager 없이 기존 GoldDrop의 배달 시작/완료 확인.
- TutorialUI: 기존 GameManager HUD의 공격/스킬/건설/골드를 강조하고 골드/진행 표시를 갱신한다. 추가 UI는 설명/진행/강조/스킵 확인만 사용한다.
- TutorialBuildPointGuide: 건설/조합 단계에만 바닥 링과 건설 지점 위의 움직이는 노란 화살표를 표시한다. 화면 밖에서는 가장자리 방향 안내와 거리를 표시한다. TutorialManager가 튜토리얼에서만 생성한다.
- TutorialCastleCutscene: Cinemachine 컷씬, 근처 몬스터 이동/공격 연출, 실제 HP 10 감소.
- TutorialProjectileTargeting: 튜토리얼 총알 복제본에서만 타겟 지정. 기존 터렛/총알 코드는 수정하지 않는다.

ESC로 확인창을 열면 게임과 입력이 정지한다. 취소는 원래 단계로 돌아온다.
스킵도 완료로 저장한다. 저장 키는 `Tutorial.Completed.v1`, 조회는 `TutorialManager.HasCompleted`.
타이틀/본게임 자동 진입은 보류했으므로 `OnCompleted` UnityEvent만 제공한다.
직접 튜토리얼 씬을 실행하는 개발 검증은 저장 여부와 무관하게 가능하다.
향후 진입 버튼에서 HasCompleted를 검사해야 첫 실행만 안내하는 동작이 완성된다.

## 기존 파일 변경

- PlayerInputReader: 공통 IPlayerInputFilter 연결 및 키 정보 조회만 추가. 필터 없으면 기존 입력 그대로.
- GoldDrop: WaveManager가 없는 씬의 이벤트 구독/해제 null 검사, 파괴 시 구독 해제.

GameManager, WaveManager, BuildPoint, BaseTurret, 개별 터렛/공격 스크립트는 변경하지 않았다.
씬 안의 GameManager는 자동 Start/Update를 끄고 플레이어 참조와 기존 UI 콜백만 재사용한다.
튜토리얼 씬에서 WaveManager 컴포넌트를 제거하고 기존 MonsterSpawner는 비활성화했다.
조합표는 원본 프리팹을 유지하며 복제 씬의 인스펙터 오버라이드로 4슬롯/1조합을 설정했다.

## 확인

씬 생성 도구를 삭제한 상태에서는 기존 TutorialScene을 직접 편집한다.

몬스터는 TutorialManager의 OutsideSpawnPosition에서 등장한다. 첫 웨이브는 CombatSpawn까지 접근한 뒤 대기하며, 두 번째 웨이브는 기존 TurretPath를 따라 이동한다. NavMesh 샘플 및 접근 경로가 완전한지 생성 전에 검사하고, 실패하면 로그와 안내를 표시한다. 플레이어용 TutorialBoundary 콜라이더는 해당 튜토리얼 몬스터와의 충돌만 제외한다. 본게임 충돌 설정은 변경하지 않는다.
경계/지형/NavMesh를 변경했다면 OutsideSpawnPosition을 벽 밖의 이동 가능한 도로에 맞춘다. 현재 좌표는 기존 Wave1_WayPointPath의 WayPoint_1 (7)의 월드 좌표(-16.365416, 7.09, 11.89)를 사용한다.

실제 플레이에서는 전체 단계, 골드 300G 배달, 건설/조합 후 잔액, 잠긴 2~4번 입력,
스킵 취소/확정, 캐슬 카메라 복구를 확인한다. 준비 중 오래 기다려도 웨이브가 자동 시작되면 안 된다.
일반 웨이브 몬스터는 새 인스턴스를 사용하므로 기존 몬스터 풀의 초기화 문제를 수정하지 않는다.
기존에 편집한 경계벽/안개 배치는 유지한다. 실제 플레이에서 등장 위치의 높이와 NavMesh 경로, 화면 밖 방향 안내, 건설/조합 후 안내 종료를 확인한다.
