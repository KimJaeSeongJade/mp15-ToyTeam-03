using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerBuildMode : MonoBehaviour
{
    // TODO: 터렛 선택 추후 연결시 터렛 클래스로 변경
    [SerializeField] private BaseTurret _selectedTurret;

    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private float _rayDistance;

    private PlayerInputReader _inputReader;
    private PlayerAttackMode _attackMode;
    private BaseTurret _previewTurret;
    private TowerTest _towerTest;

    private Ray ray;
    private bool isBuildMode = false;

    private void Awake() => CacheComponenet();
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
        if (_selectedTurret == null) return;

        if (_previewTurret == null)
        {
            _previewTurret = Instantiate(_selectedTurret);
        }

        Vector3 centerPoint = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        ray = Camera.main.ScreenPointToRay(centerPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _targetMask))
        {
            // TODO: 디버그용 임시 코드. 추후 연결시 삭제
            if (hit.transform.TryGetComponent(out _towerTest))
            {
                _towerTest.CallbackDebugLog();
                //_previewTurret.CanBuild(true);

#if UNITY_EDITOR
                Debug.Log("설치 영역 감지됨: " + hit.collider.gameObject.name);
                Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.green);
#endif
            }
            else
            {
                _towerTest = null;
                //_previewTurret.CanBuild(false);

#if UNITY_EDITOR
                Debug.Log("감지된 설치 영역 없음");
                Debug.DrawRay(ray.origin, ray.direction * _rayDistance, Color.red);
#endif
            }

            _previewTurret.transform.position = hit.point;

        }
        // TODO: 선택한 터렛 미리보기 홀로그램 출력
        // 탐지 영역 결과를 보고 결과 나온걸로 출력
    }

    // TODO: 추후 변경 가능성 있음. 키 입력 구독 처리로 refac 예정
    public void SetSelectedTurret()
    {
        if (!_inputReader.isPressedPrimary
            && !_inputReader.isPressedSub
            && !_inputReader.isPressedThird
            && !_inputReader.isPressedForth) return;


        if (_inputReader.isPressedPrimary)
        {
            SetSelectedTurret(0);
        }
        else if (_inputReader.isPressedSub)
        {
            SetSelectedTurret(1);

        }
        else if (_inputReader.isPressedThird)
        {
            SetSelectedTurret(2);

        }
        else if (_inputReader.isPressedForth)
        {
            SetSelectedTurret(3);

        }
    }

    public void SetSelectedTurret(int selectedNum)
    {
        _selectedTurret = TurretCombinationTable.Instance.GetSelectedTurret(selectedNum);

        if (_previewTurret == null) return;

        Destroy(_previewTurret.gameObject);
        _previewTurret = Instantiate(_selectedTurret);
    }

    private void TurretBuild()
    {
        if (_towerTest == null || !_inputReader.isPressedAttackDown) return;

        _towerTest.Build(_previewTurret);
        //_previewTurret.TryBuild();
        _previewTurret = null;

    }

    private void BuildModeEnd()
    {
        if (!_inputReader.isPressedExitBuild) return;

        isBuildMode = false;

        Destroy(_previewTurret.gameObject);
        _selectedTurret = null;
        _previewTurret = null;

        enabled = isBuildMode;
        _attackMode.enabled = !isBuildMode;

    }

    private void CacheComponenet()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _attackMode = GetComponent<PlayerAttackMode>();
    }

    private void Init()
    {
        enabled = isBuildMode;
    }
}
