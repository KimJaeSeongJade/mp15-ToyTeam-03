using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*public class MonsterPool : MonoBehaviour
{
    [SerializeField] private int _waveMonstersCount;
    [SerializeField] private GameObject _monsterPrefab;
    //생성한 것을 다시 죽을 때 회수
    private GameObject[] _monsters;
    
    //prefab를 list안에 저장
    private int _count;
    //하나 생성할 때마다 --;


    private void Start() => FillPool();

    public void Pop()
    {
        if (_count > 0)
        {
            GameObject monster = _monsters[_count - 1];
            monster.SetActive(true);
            //yield return; coroutine
            
        }
    }
    
    
    public void FillPool() //몬스터를 배열에 저장
    {
        _monsters = new GameObject[_waveMonstersCount];

        for (int i = 0; i < _waveMonstersCount; i++)
        {
            GameObject monster = Instantiate(_monsterPrefab);
            monster.SetActive(false);
            
            _monsters[i] = monster;
        }
        _count = _waveMonstersCount;
        
        Debug.Log($"이번 Wave 생성 예정 몬스터 수:{_waveMonstersCount}");
    }

    
    


}*/
