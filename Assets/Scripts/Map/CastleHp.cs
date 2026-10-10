using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class CastleHp : MonoBehaviour,ICastleInterface
{
    [SerializeField] private float _hp;
    [SerializeField] private float _maxHp;
    private void Awake()
    {
        _hp = _maxHp;
    }
    
    public event Action <float,float> OnCastleHealthChanged;
    public void Heal(float amount)
    {
        if (_hp <= 0 || amount <= 0) return;
        _hp = Mathf.Min(_maxHp, _hp + amount);
        OnCastleHealthChanged?.Invoke(_hp, _maxHp);
    }
    

    public void CastleTakeDamage(float damage)
    {
        if (_hp <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("Game Over");
#endif
            return;
        }
        _hp -= damage;

        OnCastleHealthChanged?.Invoke(_hp, _maxHp);
    }
    /*UI에서 
    private void Start()
    {
        _castleHp.OnCastleHealthChanged += UpdateCastleHp;
    }
    private void UpdateCastleHp(float currentHp, float maxHp)
    {
        Debug.Log($"UI HP 변경: {currentHp}/{maxHp}");
    }*/
    
}
