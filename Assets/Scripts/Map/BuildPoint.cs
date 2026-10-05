using System.Collections;
using UnityEngine;

public class BuildPoint : MonoBehaviour
{
    [SerializeField] private BaseTurret _currentTurret;

    private TurretCombinationTable table;

    public Vector3 PlacementPosition => transform.TransformPoint(Vector3.zero);
    public Quaternion PlacementRotation => transform.rotation;

    private void Awake() => CacheComponent();

    // 플레이어는 조합 규칙을 몰라도 설치 가능 여부와 표시할 프리팹을 받을 수 있다.
    public bool TryGetResultTurret(BaseTurret selectedTurret, out BaseTurret resultTurret)
    {
        resultTurret = null;
        if (selectedTurret == null) return false;

        if (_currentTurret == null)
        {
            resultTurret = selectedTurret;
            return true;
        }

        return table.TryGetResult(_currentTurret.Type, selectedTurret.Type, out resultTurret);
    }

    // 선택한 프리팹으로 설치한다. 미리보기 오브젝트는 실제 타워로 사용하지 않는다.
    public bool TryBuildTurret(BaseTurret selectedTurret, out int resultCose)
    {
        if (!TryGetResultTurret(selectedTurret, out BaseTurret resultTurret))
        {
            resultCose = resultTurret.Cost;
            return false;
        }

        BaseTurret previousTurret = _currentTurret;

        BaseTurret newTurret = Instantiate(resultTurret, transform);
        newTurret.transform.localPosition = Vector3.zero;
        newTurret.transform.rotation = transform.rotation;
        _currentTurret = newTurret;

        resultCose = resultTurret.Cost;

        if (previousTurret != null)
            Destroy(previousTurret.gameObject);

        return true;
    }

    // 현재 설치된 터렛을 삭제하고 원래 가격의 50%만 돌려줌
    public bool TrySellTurret(PlayerWallet wallet)
    {
        if (_currentTurret == null) return false;

        wallet.AddGold(_currentTurret.Cost / 2);

        Destroy(_currentTurret.gameObject);

        _currentTurret = null;

        return true;
    }

    private void CacheComponent()
    {
        StartCoroutine(WaitForTable());
    }

    private IEnumerator WaitForTable()
    {
        yield return new WaitUntil(() => TurretCombinationTable.Instance != null);

        table = TurretCombinationTable.Instance;
    }
}
