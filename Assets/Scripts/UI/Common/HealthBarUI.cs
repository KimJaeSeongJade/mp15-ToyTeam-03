using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _fillImage;

    private IHealthSource _source;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _source = GetComponentInParent<IHealthSource>();
    }

    private void OnEnable()
    {
        _source.OnHealthChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        _source.OnHealthChanged -= Refresh;
    }

    private void Refresh()
    {
        _fillImage.fillAmount = Mathf.Clamp01(_source.CurrentHp / _source.MaxHp);
    }

    private void LateUpdate()
    {
        transform.rotation = _camera.transform.rotation;
    }
}
