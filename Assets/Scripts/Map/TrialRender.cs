using UnityEngine;

public class TrialRender : MonoBehaviour
{
    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private WayPointPath[] _wayPointPath;
        //private int _count = 0;
    
    public void Move(int _count)
    {
        if (_monsterPrefab != null || _wayPointPath != null)
        {
            Transform spawnPoint = _wayPointPath[_count].transform;

            GameObject monster = Instantiate(
                _monsterPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            MonsterMove monsterMove = monster.GetComponent<MonsterMove>();

            if (monsterMove != null)
            {
                monsterMove.Initialize(_wayPointPath[_count]);
            }
            else
            {
                Debug.LogError("MonsterMove 컴포넌트가 없습니다.");
            }
        }
    }
}