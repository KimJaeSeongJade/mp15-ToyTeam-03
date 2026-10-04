using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : SingletonBehaviour<UITest>
{
    [SerializeField] private Image turret1fillImage;
    [SerializeField] private Image turret2fillImage;
    private float _remainingTime = 3;
    private const float TURRET1_COOLDOWN = 5;
    private void Update()
    {
        // 1, 2번 눌렀을 때 UI 상호작용되는지 처리
        // 나중에는 설치된 기준으로 쿨타임 돌아가게 구현 해야함.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1번 터렛 쿨타임 감소 중");
            turret1fillImage.fillAmount = _remainingTime / TURRET1_COOLDOWN;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("2번 터렛 쿨타임 감소 중");
            turret2fillImage.fillAmount = _remainingTime / TURRET1_COOLDOWN;
        }
    }
}
