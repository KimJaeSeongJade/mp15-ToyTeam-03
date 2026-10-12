using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BountyMark : MonoBehaviour
{
    private int _rewardGold;
    private bool _isRewarded = false;

    public void Setup(int gold)
    {
        _rewardGold = gold;
    }

    // 💡 [핵심] 처형 터렛에 의해 죽든, 일반 공격에 맞아 죽든 
    // 몬스터 오브젝트가 씬에서 소멸(ReturnToPool 또는 Destroy)할 때 100% 실행됩니다.
    private void OnDestroy()
    {
        if (_isRewarded) return;
        _isRewarded = true;

        // 플레이어 지갑 시스템 루트 추적 (기존 프로젝트 규칙 활용)
        if (GameManager.Instance != null && GameManager.Instance.PlayerStatus != null)
        {
            var playerWallet = GameManager.Instance.PlayerStatus.GetComponent<PlayerWallet>();
            if (playerWallet != null)
            {
                playerWallet.AddGold(_rewardGold);
                Debug.Log($"<color=gold>[💎 현상금 수금 완료]</color> 표식이 찍힌 적이 처치되어 지갑에 +{_rewardGold}골드가 추가되었습니다!");
            }
        }
    }
}