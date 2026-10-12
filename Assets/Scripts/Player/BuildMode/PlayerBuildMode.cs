using UnityEngine;

public class PlayerBuildMode : MonoBehaviour
{
    [SerializeField] private BaseTurret _selectedTurret;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private float _rayDistance;
    [Header("터렛 정보 UI (World Space 프리팹을 한 번 생성하여 재사용)")]
    [SerializeField] private BuildPointTurretUI _turretInfoPrefab;
    private BuildPointTurretUI _turretInfoUI;

    private Camera mainCamera;
    private BuildPoint _buildPoint;
    private PlayerInputReader _inputReader;
    private PlayerStatus _status;
    private PlayerAttackMode _attackMode;
    private PlayerWallet _wallet;
    private PlayerSound _sound;
    private int _selectedSlot = -1;

    private void Awake()
    {
        CacheComponent();
        if (_turretInfoPrefab != null)
        {
            _turretInfoUI = Instantiate(_turretInfoPrefab, transform);
            _turretInfoUI.Hide();
        }
    }
    private void Start() => Init();
    private void OnDisable() => ClearSelectedBuildTarget();
    private void OnDestroy()
    {
        if (_turretInfoUI != null) Destroy(_turretInfoUI.gameObject);
    }

    private void Update()
    {
        SetSelectedTurret();
        RayToBuildPoint();
        TurretBuild();
        TurretSell();
        BuildModeEnd();
    }

    private void RayToBuildPoint()
    {
        if (_selectedTurret == null || _selectedSlot < 0 || mainCamera == null)
        {
            _buildPoint = null;
            ClearSelectedBuildTarget();
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        _buildPoint = null;
        BaseTurret resultPrefab = _selectedTurret;
        bool canBuild = false;
        BaseTurret infoResult = null;

        // 허공을 바라보면 최대 거리의 위치를 플레이어 발높이에 투영한다.
        Vector3 previewPosition = ray.GetPoint(_rayDistance);
        previewPosition.y = transform.position.y;
        Quaternion previewRotation = transform.rotation;

        if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _targetMask))
        {
            previewPosition = hit.point;

            // 일단 설치 가능 스팟은 한종류 뿐이니 직접 찾기로 하고
            // 기능 확장이 필요할때 인터페이스로 변경하는걸로...
            BuildPoint point = hit.collider.GetComponentInParent<BuildPoint>();
            if (point != null)
            {
                _buildPoint = point;
                if (point.TryGetResultTurret(resultPrefab, out BaseTurret buildResult))
                {
                    resultPrefab = buildResult;
                    infoResult = buildResult;
                    canBuild = _wallet.TryGetBoolSpendGold(buildResult.Cost)
                        && TurretCombinationTable.Instance.IsBuildReady(_selectedSlot);
                }
                else
                {
                    // 조합 불가 시 선택한 터렛의 빨간 미리보기는 유지한다.
                    // UI용 infoResult는 null로 남겨 조합 불가를 표시한다.
                    resultPrefab = _selectedTurret;
                }
                previewPosition = point.PlacementPosition;
                previewRotation = point.PlacementRotation;
            }
        }

        // BuildPoint가 계산한 결과를 터렛에 전달한다.
        // BaseTurret에 IBuildTargetReceiver 추가 필요
        if (_selectedTurret is IBuildTargetReceiver receiver)
        {
            if (resultPrefab != null)
                receiver.PreviewShow(resultPrefab, canBuild, previewPosition, previewRotation);
            else
                receiver.PreviewHide();
        }

        if (_turretInfoUI != null)
        {
            if (_buildPoint != null)
                _turretInfoUI.Show(infoResult, canBuild, _buildPoint.HasTurret, _buildPoint.SellGold,
                    _buildPoint.PlacementPosition, mainCamera);
            else
                _turretInfoUI.Hide();
        }
    }

    // 일반 공격 모드에서는 PlayerAttackMode가 선택 번호를 넘겨준다.
    public bool SetSelectedTurret()
    {
        int selectedNum = _inputReader.GetSelectedTurretIndex();
        if (selectedNum < 0) return false;

        BaseTurret selected = TurretCombinationTable.Instance.GetSelectedTurret(selectedNum);
        _selectedSlot = selectedNum;

        if (selected == _selectedTurret) return true;
        SoundManager.Instance.Play(_sound.SelectTurret);

        ClearSelectedBuildTarget();

        _selectedTurret = selected;
        return true;
    }

    private void TurretBuild()
    {
        if (!_inputReader.isPressedAttackDown || _buildPoint == null || _selectedTurret == null || _selectedSlot < 0) return;
        if (!TurretCombinationTable.Instance.IsBuildReady(_selectedSlot)) return;

        if (!_buildPoint.TryGetResultTurret(_selectedTurret, out BaseTurret resultTurret)
            || !_wallet.TryGetBoolSpendGold(resultTurret.Cost)) return;

        // 설치 시에도 BuildPoint가 같은 조합 규칙으로 결과를 결정하고 true시 원래 색으로 되돌린다.
        // int resultCost 로 조합 결과 터렛의 가격으로 계산한다.
        if (_buildPoint.TryBuildTurret(_selectedTurret, out int resultCost)
            && _wallet.TrySpendGold(resultCost))
        {
            float cooldown = TurretCombinationTable.Instance.StartBuildCooldown(_selectedSlot);
            _status.NotifyBuildCooldownStarted(_selectedSlot, cooldown);
            SoundManager.Instance.Play(_sound.Build);
            ClearSelectedBuildTarget();
            RayToBuildPoint();
        }
        return;
    }

    private void TurretSell()
    {
        if (!_inputReader.isPressedSkillDown || _buildPoint == null) return;

        // 판매 성공시 프리뷰 색을 다시 정한다
        if (_buildPoint.TrySellTurret(_wallet))
        {
            SoundManager.Instance.Play(_sound.Sell);
            ClearSelectedBuildTarget();
            RayToBuildPoint();
        }
    }

    private void BuildModeEnd()
    {
        if (!_inputReader.isPressedExitBuild) return;
        SoundManager.Instance.Play(_sound.ExitBuild);

        ClearSelectedBuildTarget();

        _status.PlayerModeChange(true);

        _selectedTurret = null;
        _selectedSlot = -1;
        _buildPoint = null;
        enabled = false;

        _attackMode.enabled = true;
    }

    private void CacheComponent()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _attackMode = GetComponent<PlayerAttackMode>();
        _status = GetComponent<PlayerStatus>();
        _wallet = GetComponent<PlayerWallet>();
        _sound = GetComponent<PlayerSound>();
        mainCamera = Camera.main;
    }

    private void ClearSelectedBuildTarget()
    {
        if (_turretInfoUI != null) _turretInfoUI.Hide();
        if(_selectedTurret is IBuildTargetReceiver receiver)
            receiver.PreviewHide();
    }

    private void Init()
    {
        enabled = false;
    }
}
