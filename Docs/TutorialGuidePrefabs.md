# 튜토리얼 가이드 프리팹

`TutorialManager → 공통 참조 → Build Guide Style / Enemy Guide Style`에서 각각 지정합니다.
비어 있는 프리팹 항목은 기존 코드로 만든 표시를 그대로 사용합니다.

- **World Arrow Prefab**: 대상 위 화살표. 모델, 월드 스페이스 Canvas, 이펙트를 사용할 수 있습니다.
- **World Ring Prefab**: 빌드포인트 링. 몬스터 가이드에서는 사용하지 않습니다.
- **Screen Guide Prefab**: 이름·거리와 화면 밖 방향 안내.
- **World Arrow Offset / World Ring Offset / Screen Anchor Offset**: 대상 위치 기준 오프셋.
- **World Arrow Scale / World Ring Scale**: 원래 프리팹 크기에 곱할 배율.
- **Arrow Faces Camera**: 카메라 방향으로 정렬. 회전이 고정된 3D 모델은 해제합니다.
- **Bob Amount / Bob Speed**: 위아래 움직임. Amount를 0으로 설정하면 움직이지 않습니다.
- **Default Color**: 프리팹을 지정하지 않은 기본 표시의 색상. 커스텀 프리팹의 색상은 유지합니다.

화면 안내 프리팹의 루트는 RectTransform으로 만들고 `TutorialGuideScreenUI`를 붙입니다.
자식 TMP 텍스트를 **Label**에, 자식 화살표 이미지를 **Direction Arrow**에 연결합니다.
선택적으로 별도 TMP 텍스트를 **Distance**에 연결할 수 있습니다.
화살표는 루트나 텍스트의 부모 대신 별도 자식으로 두어야 이름·거리를 숨기지 않습니다.
프리팹 내부에 Canvas를 넣지 않고, 실행 시 기존 튜토리얼 Canvas 아래에 생성하도록 구성합니다.

`Label Format`에는 `{이름}`, `{거리}`를 사용할 수 있습니다.
거리 칸을 따로 쓰면 Label Format을 `{이름}`으로 바꾸세요.
화살표 이미지가 위쪽을 향하면 `TutorialGuideScreenUI → Arrow Angle Offset`을 -90으로 설정합니다.

가이드는 처음 필요할 때 생성하고 대상이 바뀌면 재사용합니다.
프리팹을 바꾼 설정은 다음 플레이부터 적용됩니다. 이번 변경에서는 교체용 프리팹 자체를 생성하지 않습니다.
