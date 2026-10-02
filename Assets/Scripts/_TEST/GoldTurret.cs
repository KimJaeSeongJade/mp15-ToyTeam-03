using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldTurret : MonoBehaviour
{
    [SerializeField] private GameObject _goldPrefab;
    [SerializeField] private int _initialSize = 10;
    private List<GameObject> _golds = new List<GameObject>();

    public static GoldPool Instance { get; private set; }

    private void Start()
    {
        Instance = this; //인스턴스는 자기 자신
        FillPool(); //시작시 풀 채우기
    }
    private void Update() // 추가 생성이 되는지 확인
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Take();
        }
    }

    private void FillPool()
    {
        // 오브젝트 풀 _initialSize 만큼 채우기
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject gold = Instantiate(_goldPrefab);
            gold.SetActive(false);
            _golds.Add(gold);
        }
    }
    public GameObject Take() //담긴 오브젝트 꺼내기
    {
        foreach (GameObject gold in _golds) // 순회 돌아 비활성화이면 활성화로 변환
        {
            if (!gold.activeSelf)
            {
                gold.SetActive(true);
                return gold;
            }
        }
        // 만든 오브젝트 풀을 다 사용했다면 추가 생성
        GameObject newgold = Instantiate(_goldPrefab);
        newgold.SetActive(true);
        _golds.Add(newgold);
        return newgold;
    }
}
