using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGroup : MonoBehaviour
{
    [Serializable]
    public struct MonsterData
    {
        public GameObject monster;
        public int count;
        public int waypointNum;
    }

    public List<MonsterData> monsterDatas;
}
