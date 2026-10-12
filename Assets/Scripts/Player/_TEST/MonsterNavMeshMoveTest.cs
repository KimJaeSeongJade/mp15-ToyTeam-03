using UnityEngine;
using UnityEngine.AI;

/*[RequireComponent(typeof(NavMeshAgent))]
public class MonsterNavMeshMoveTest : MonoBehaviour
{
    // 실제로는 쓰지 스포너가 지정한다
    [SerializeField] private WaypointPath _testPath;

    private NavMeshAgent _agent;
    private BaseEnemy _enemy;
    private Transform[] _waypoints;
    private int _waypointIndex;
    private bool _moving;

    private void Awake() => CacheComponent();
    private void Start() => Init();
    private void Update() => CheckWaypointIsArrived();
    
    public void Initialize(WaypointPath path)
    {
        _testPath = path;


        _waypoints = path.Waypoint;
        _waypointIndex = 0;
        _moving = true;
        _agent.SetDestination(_waypoints[_waypointIndex].position);
    }
    // 매 프레임 남은 거리 체크 함수
    private void CheckWaypointIsArrived()
    {
        // 멈춰있거나, 길을 계산중이거나, 길 설정이 안되어있다면
        if (!_moving || _agent.pathPending || !_agent.hasPath) return;

        // 남은 거리가 멈춰야할 거리보다 높다면
        if (_agent.remainingDistance > _agent.stoppingDistance) return;

        _waypointIndex++;

        if (_waypointIndex < _waypoints.Length)
        {
            // _waypointIndex 번째 목적지로 설정하고 return
            _agent.SetDestination(_waypoints[_waypointIndex].position);
            return;
        }


        // 더이상 갈 목적지가 없다면 풀링 종료
        _moving = false;
        _enemy.ReturnToPool();

        Debug.Log("NavMesh 테스트 몬스터 경로 완료", this);
    }

    private void CacheComponent()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<BaseEnemy>();
    }

    private void Init()
    {
        // 프리팹은 씬 참조를 저장할 수 없으므로 테스트 씬의 경로 하나를 자동으로 찾는다.
        // 실제 스포너 연결 시에는 생성 직후 Initialize(path)를 호출하면 된다.
        // 죽, 실제 씬에서는 사용하지 않는다는 소리
        if (_testPath == null) _testPath = FindObjectOfType<WaypointPath>();
        if (!_moving) Initialize(_testPath);
    }
}
*/