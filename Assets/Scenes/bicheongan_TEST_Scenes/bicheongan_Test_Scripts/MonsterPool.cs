using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private int _monsterSize = 30;


    private GameObject[] _monster;
    private int _count;

    //private static MonsterPool instance { get; private set; }

    private void Start()
    {
        FillPool();
    }


    private void FillPool()
    {
        _monster = new GameObject[_monsterSize];
        for (int i = 0; i < _monsterSize; i++)
        {
            _monster[i] = Instantiate(_monsterPrefab);
        }
        _count = _monsterSize;
    }

}
