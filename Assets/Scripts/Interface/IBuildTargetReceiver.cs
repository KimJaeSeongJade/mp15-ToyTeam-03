using UnityEngine;

// 빌드 모드는 이 계약으로만 터렛에 설치 결과를 전달한다.
public interface IBuildTargetReceiver
{
    void PreviewShow(BaseTurret resultPrefab, bool canBuild, Vector3 position, Quaternion rotation);
    void PreviewHide();
}
