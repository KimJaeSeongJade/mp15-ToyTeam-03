using UnityEngine;

public class PlayerBuildMode : MonoBehaviour
{
    [SerializeField] private BaseTurret _selectedTurret;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private float _rayDistance;

    private Camera mainCamera;
    private BuildPoint _buildPoint;
    private PlayerInputReader _inputReader;
    private PlayerAttackMode _attackMode;

    private void Awake() => CacheComponent();
    private void Start() => Init();

    private void Update()
    {
        SetSelectedTurret();
        RayToBuildPoint();
        TurretBuild();
        BuildModeEnd();
    }

    private void RayToBuildPoint()
    {
        if (_selectedTurret == null)
        {
            _buildPoint = null;
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        _buildPoint = null;
        BaseTurret resultPrefab = _selectedTurret;
        bool canBuild = false;

        // 허공을 바라보면 최대 거리의 위치를 플레이어 발높이에 투영한다.
        Vector3 previewPosition = ray.GetPoint(_rayDistance);
        previewPosition.y = transform.position.y;
        Quaternion previewRotation = transform.rotation;

        if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _targetMask))
        {
            previewPosition = hit.point;

            // 일단 설치 가능 스팟은 한종류 뿐이니 직접 찾기로 하고
            // 기능 확장이 필요할때 인터페이스로 변경하는걸로...
            BuildPoint point = hit.collider.GetComponent<BuildPoint>();
            if (point != null)
            {
                _buildPoint = point;
                canBuild = point.TryGetResultTurret(resultPrefab, out BaseTurret buildResult);
                if (canBuild) resultPrefab = buildResult;
                previewPosition = point.PlacementPosition;
                previewRotation = point.PlacementRotation;
            }
        }

        // BuildPoint가 계산한 결과를 터렛에 전달한다.
        // BaseTurret에 IBuildTargetReceiver 추가 필요
        if (_selectedTurret is IBuildTargetReceiver receiver)
            receiver.PreviewShow(resultPrefab, canBuild, previewPosition, previewRotation);
    }

    // 일반 공격 모드에서는 PlayerAttackMode가 선택 번호를 넘겨준다.
    public void SetSelectedTurret()
    {
        if (_inputReader.isPressedPrimary)
            SetSelectedTurret(0);
        else if (_inputReader.isPressedSub)
            SetSelectedTurret(1);
        else if (_inputReader.isPressedThird)
            SetSelectedTurret(2);
        else if (_inputReader.isPressedForth)
            SetSelectedTurret(3);
    }

    public void SetSelectedTurret(int selectedNum)
    {
        BaseTurret selected = TurretCombinationTable.Instance.GetSelectedTurret(selectedNum);

        if (selected == _selectedTurret) return;

        ClearSelectedBuildTarget();

        _selectedTurret = selected;
    }

    private void TurretBuild()
    {
        if (!_inputReader.isPressedAttackDown || _buildPoint == null || _selectedTurret == null) return;

        // 설치 시에도 BuildPoint가 같은 조합 규칙으로 결과를 결정하고 true시 원래 색으로 되돌린다.
        if (_buildPoint.TryBuildTurret(_selectedTurret))
            ClearSelectedBuildTarget();
    }

    private void BuildModeEnd()
    {
        if (!_inputReader.isPressedExitBuild) return;

        ClearSelectedBuildTarget();

        _selectedTurret = null;
        _buildPoint = null;
        enabled = false;

        _attackMode.enabled = true;
    }

    private void CacheComponent()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _attackMode = GetComponent<PlayerAttackMode>();
        mainCamera = Camera.main;
    }

    private void ClearSelectedBuildTarget()
    {
        if(_selectedTurret is IBuildTargetReceiver receiver)
            receiver.PreviewHide();
    }

    private void Init()
    {
        enabled = false;
    }
}
