using UnityEngine;

// 현재 안내하는 빌드포인트만 건설 레이에 감지되게 한다. 본게임에는 추가하지 않는다.
public class TutorialBuildAreaFilter : MonoBehaviour
{
    private BuildPoint[] _points;
    private readonly System.Collections.Generic.Dictionary<Transform, int> _layers = new System.Collections.Generic.Dictionary<Transform, int>();
    public void Initialize()
    {
        var points = new System.Collections.Generic.List<BuildPoint>();
        foreach (BuildPoint point in FindObjectsOfType<BuildPoint>())
            if (point.gameObject.scene == gameObject.scene) points.Add(point);
        _points = points.ToArray();
        foreach (BuildPoint point in _points) Remember(point.transform);
    }
    public void Allow(BuildPoint allowed)
    {
        if (_points == null) return;
        for (int i = 0; i < _points.Length; i++)
        {
            if (_points[i] == null) continue;
            Remember(_points[i].transform);
            foreach (Transform child in _points[i].GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = _points[i] == allowed ? _layers[child] : 2;
        }
    }
    private void Remember(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (!_layers.ContainsKey(child)) _layers.Add(child, child.gameObject.layer);
    }
    private void OnDestroy()
    {
        foreach (var item in _layers)
            if (item.Key != null) item.Key.gameObject.layer = item.Value;
    }
}
