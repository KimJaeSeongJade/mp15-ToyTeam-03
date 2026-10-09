using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

// 캐슬 시연에만 사용한다. 실제 캐슬 피해 함수를 호출하고 카메라를 복구한다.
public class TutorialCastleCutscene : MonoBehaviour
{
    public CinemachineVirtualCamera FocusCamera;
    public Transform MonsterSpawn;
    public Transform HitPoint;
    public CastleHp Castle;
    public BaseEnemy MonsterPrefab;
    [Min(0f)] public float Damage = 10f;
    [Min(0.1f)] public float ApproachTime = 1.5f;

    private Camera _camera;
    private PlayerMovement _movement;
    private CinemachineBrain _brain;
    private bool _movementEnabled;
    private bool _brainEnabled;
    private Vector3 _cameraPosition;
    private Quaternion _cameraRotation;
    private BaseEnemy _actor;
    private bool _cameraCaptured;

    public IEnumerator Play(PlayerStatus player)
    {
        _camera = Camera.main;
        _cameraCaptured = _camera != null;
        _movement = player.GetComponent<PlayerMovement>();
        _movementEnabled = _movement != null && _movement.enabled;
        if (_movement != null) _movement.enabled = false;
        if (_camera != null)
        {
            _cameraPosition = _camera.transform.position;
            _cameraRotation = _camera.transform.rotation;
            _brain = _camera.GetComponent<CinemachineBrain>();
            if (_brain != null)
            {
                _brainEnabled = _brain.enabled;
                _brain.enabled = true;
            }
        }
        FocusCamera.gameObject.SetActive(true);
        FocusCamera.Priority = 100;
        yield return new WaitForSeconds(0.8f);

        // 이 몬스터는 연출용으로만 이동한다. 일반 이동/공격 AI와 중복 실행하지 않는다.
        BaseEnemy actor = Instantiate(MonsterPrefab, MonsterSpawn.position, MonsterSpawn.rotation);
        _actor = actor;
        actor.MonExp = 0f;
        foreach (MonoBehaviour behaviour in actor.GetComponentsInChildren<MonoBehaviour>())
            if (!(behaviour is BaseEnemy)) behaviour.enabled = false;
        NavMeshAgent agent = actor.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        foreach (Collider collider in actor.GetComponentsInChildren<Collider>()) collider.enabled = false;

        Vector3 start = MonsterSpawn.position;
        actor.transform.LookAt(new Vector3(HitPoint.position.x, start.y, HitPoint.position.z));
        Animator animator = actor.GetComponentInChildren<Animator>();
        if (animator != null)
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.name == "IsMove" && parameter.type == AnimatorControllerParameterType.Bool)
                    animator.SetBool("IsMove", true);
        float elapsed = 0f;
        while (elapsed < ApproachTime)
        {
            elapsed += Time.deltaTime;
            actor.transform.position = Vector3.Lerp(start, HitPoint.position, elapsed / ApproachTime);
            yield return null;
        }
        if (animator != null)
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == "IsMove" && parameter.type == AnimatorControllerParameterType.Bool)
                    animator.SetBool("IsMove", false);
                if (parameter.name == "IsAttack" && parameter.type == AnimatorControllerParameterType.Bool)
                    animator.SetBool("IsAttack", true);
            }
        yield return new WaitForSeconds(0.35f);
        Castle.CastleTakeDamage(Damage);
        yield return new WaitForSeconds(1.2f);
        RestoreCamera();
    }

    public void RestoreCamera()
    {
        if (FocusCamera != null) FocusCamera.gameObject.SetActive(false);
        if (_actor != null) { Destroy(_actor.gameObject); _actor = null; }
        if (!_cameraCaptured) return;
        if (_brain != null) _brain.enabled = _brainEnabled;
        if (_camera != null) _camera.transform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        if (_movement != null) _movement.enabled = _movementEnabled;
        _cameraCaptured = false;
    }
}
