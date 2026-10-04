using UnityEngine;

public class TurretPreview : MonoBehaviour
{
    private BaseTurret _previewTurret;
    private BaseTurret _previewSource;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _properties;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    // 터렛 담당: 프리팹 루트에 이 컴포넌트를 추가하고,
    // IBuildTargetReceiver.UpdateBuildTarget/ClearBuildTarget을 구현해 Show/Hide에 전달한다.
    // 조합과 설치 가능 여부는 BuildPoint에서 계산하므로 여기서는 다시 판단하지 않는다.
    public void Show(BaseTurret resultPrefab, Vector3 position, Quaternion rotation, bool canBuild)
    {
        if (resultPrefab == null)
        {
            Hide();
            return;
        }

        if (_previewTurret == null || _previewSource != resultPrefab)
        {
            Hide();
            CreatePreview(resultPrefab);
        }

        ViewUpdate(position, rotation, canBuild);
    }

    public void Hide()
    {
        if (_previewTurret != null)
        {
            _previewTurret.gameObject.SetActive(false);
            Destroy(_previewTurret.gameObject);
        }

        _previewTurret = null;
        _previewSource = null;
        _renderers = null;
        _properties = null;
    }

    private void CreatePreview(BaseTurret prefab)
    {
        // 비활성 부모 아래에서 복제해 터렛 스크립트가 켜지기 전에 프리뷰 상태로 만든다.
        GameObject staging = new GameObject("TurretPreviewStaging");
        staging.SetActive(false);
        _previewTurret = Instantiate(prefab, staging.transform);
        _previewSource = prefab;

        foreach (MonoBehaviour behaviour in _previewTurret.GetComponentsInChildren<MonoBehaviour>(true))
            behaviour.enabled = false;

        foreach (Collider collider in _previewTurret.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;

        foreach (Rigidbody body in _previewTurret.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        _renderers = _previewTurret.GetComponentsInChildren<Renderer>(true);
        _properties = new MaterialPropertyBlock();

        _previewTurret.transform.SetParent(null, true);
        _previewTurret.gameObject.SetActive(true);
        Destroy(staging);
    }

    private void ViewUpdate(Vector3 position, Quaternion rotation, bool canBuild)
    {
        _previewTurret.transform.SetPositionAndRotation(position, rotation);

        // TODO: 홀로그램 이펙트를 추가할 때 이 색상 처리만 교체한다.
        SetColor(_renderers, _properties, canBuild);
    }

    private static void SetColor(Renderer[] renderers, MaterialPropertyBlock properties, bool canBuild)
    {
        Color color = canBuild ? Color.green : Color.red;
        properties.SetColor(ColorId, color);
        properties.SetColor(BaseColorId, color);

        foreach (Renderer renderer in renderers)
            renderer.SetPropertyBlock(properties);
    }

}
