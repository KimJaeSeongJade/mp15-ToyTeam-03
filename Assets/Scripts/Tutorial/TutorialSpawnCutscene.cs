using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

// 등장한 실제 몬스터를 보여 준 뒤 플레이어 카메라와 이동 상태를 복구한다.
public class TutorialSpawnCutscene : MonoBehaviour
{
    private Camera _camera;
    private PlayerMovement _movement;
    private CinemachineBrain _brain;
    private CinemachineVirtualCamera _focus;
    private NavMeshAgent _agent;
    private bool _movementEnabled;
    private bool _brainEnabled;
    private bool _agentStopped;
    private bool _captured;
    private Vector3 _position;
    private Quaternion _rotation;

    public void BeginView(PlayerStatus player, Vector3 focus, Vector3 offset)
    {
        RestoreCamera();
        _camera = Camera.main;
        if (_camera == null) return;
        _captured = true;
        _position = _camera.transform.position;
        _rotation = _camera.transform.rotation;
        _movement = player.GetComponent<PlayerMovement>();
        _movementEnabled = _movement != null && _movement.enabled;
        if (_movement != null) _movement.enabled = false;
        _brain = _camera.GetComponent<CinemachineBrain>();
        if (_brain != null)
        {
            _brainEnabled = _brain.enabled;
            _brain.enabled = true;
        }
        GameObject cameraObject = new GameObject("TutorialSequenceCamera");
        cameraObject.transform.SetParent(transform, false);
        cameraObject.transform.position = focus + offset;
        cameraObject.transform.LookAt(focus);
        _focus = cameraObject.AddComponent<CinemachineVirtualCamera>();
        _focus.Priority = 150;
        _focus.m_Lens.FieldOfView = 55f;
    }

    public IEnumerator Play(PlayerStatus player, BaseEnemy enemy)
    {
        if (enemy == null || Camera.main == null) yield break;
        Vector3 direction = player.transform.position - enemy.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.back;
        BeginView(player, enemy.transform.position + Vector3.up * 1.3f, direction.normalized * 7f + Vector3.up * 2.7f);
        _agent = enemy.GetComponent<NavMeshAgent>();
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agentStopped = _agent.isStopped;
            _agent.isStopped = true;
        }
        yield return new WaitForSeconds(2.5f);
        RestoreCamera();
    }

    public void RestoreCamera()
    {
        if (_focus != null)
        {
            _focus.gameObject.SetActive(false);
            Destroy(_focus.gameObject);
            _focus = null;
        }
        if (!_captured) return;
        if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = _agentStopped;
        _agent = null;
        if (_brain != null) _brain.enabled = _brainEnabled;
        if (_camera != null) _camera.transform.SetPositionAndRotation(_position, _rotation);
        if (_movement != null) _movement.enabled = _movementEnabled;
        _captured = false;
    }

    private void OnDisable() => RestoreCamera();
}
