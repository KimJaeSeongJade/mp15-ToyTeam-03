using UnityEngine;
using System.Collections.Generic;

// 현재 안내하는 빌드포인트만 건설 레이에 감지되게 한다. 본게임에는 추가하지 않는다.
public class TutorialBuildAreaFilter : MonoBehaviour
{
    private BuildPoint[] _points;
    private readonly Dictionary<Transform, int> _layers = new Dictionary<Transform, int>();
    public void Initialize(BuildPoint[] points)
    {
        _points = points;
        foreach (BuildPoint point in _points)
        {
            Remember(point.transform);
        }
    }
    public void Allow(BuildPoint allowed)
    {
        for (int i = 0; i < _points.Length; i++)
        {
            Remember(_points[i].transform);
            foreach (Transform child in _points[i].GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = _points[i] == allowed ? _layers[child] : 2;
        }
    }
    private void Remember(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (!_layers.ContainsKey(child)) _layers.Add(child, child.gameObject.layer);
        }
    }
    private void OnDestroy()
    {
        foreach (KeyValuePair<Transform, int> item in _layers)
        {
            if (item.Key != null) item.Key.gameObject.layer = item.Value;
        }
    }
}
