using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 실행 순서, ESC 스킵, 완료 저장만 관리한다. 단계는 컴포넌트가 아닌 직렬화 데이터다.
[DefaultExecutionOrder(-2000)]
public class TutorialManager : MonoBehaviour
{
    public const string CompletionKey = "Tutorial.Completed.v1";
    public static bool HasCompleted => PlayerPrefs.GetInt(CompletionKey, 0) == 1;

    public TutorialContext Context = new TutorialContext();
    [SerializeReference] public List<TutorialStep> Steps = new List<TutorialStep>();
    public string CompletionLog;
    public UnityEvent OnCompleted = new UnityEvent();

    public bool IsPaused { get; private set; }
    public int CurrentStepIndex { get; private set; } = -1;
    private bool _completed;
    private float _resumeTimeScale;
    private bool _resumeInput;
    private Coroutine _routine;
    private bool _initialized;

    private void Awake()
    {
        if (Context != null && Context.Game != null) Context.Game.enabled = false;
    }

    private IEnumerator Start()
    {
        yield return null;
        if (Context == null || !Context.Initialize(this)) yield break;
        _initialized = true;
        Context.UI.SkipButton.onClick.AddListener(Skip);
        Context.UI.ContinueButton.onClick.AddListener(CancelSkip);
        _routine = StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (Steps == null || Steps.Count == 0) { Context.Fail("실행할 단계 목록이 비어 있습니다."); yield break; }
        for (int i = 0; i < Steps.Count; i++)
        {
            CurrentStepIndex = i;
            TutorialStep step = Steps[i];
            if (step == null) { Context.Fail($"{i + 1}번째 단계 데이터가 없습니다."); yield break; }
            if (!step.Enabled) continue;
            yield return new WaitUntil(() => !IsPaused);
            Context.SetStepCrosshair(step.ShowCrosshair);
            Context.UI.Message.transform.parent.gameObject.SetActive(true);
            yield return step.Execute(Context);
            if (Context.Failed) yield break;
        }
        CurrentStepIndex = -1;
        Complete();
    }

    private void Update()
    {
        if (_completed || !_initialized || _routine == null || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (IsPaused) CancelSkip();
        else OpenSkip();
    }

    private void OpenSkip()
    {
        IsPaused = true;
        _resumeTimeScale = Time.timeScale;
        _resumeInput = Context.Input.InputEnabled;
        Context.Input.InputEnabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Context.UI.SkipDialog.SetActive(true);
    }

    private void CancelSkip()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Context.UI.SkipDialog.SetActive(false);
        Time.timeScale = _resumeTimeScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(ResumeInput());
    }

    private IEnumerator ResumeInput()
    {
        yield return null;
        if (!IsPaused && !_completed) Context.Input.InputEnabled = _resumeInput;
    }

    private void Skip()
    {
        if (_routine != null) StopCoroutine(_routine);
        Complete();
    }

    private void Complete()
    {
        if (_completed) return;
        _completed = true;
        Context.StopWorld();
        IsPaused = false;
        Context.Input.CancelPrompt();
        Context.Input.RequiredMoveKey = null;
        Context.Player.GetComponent<PlayerBuildMode>().enabled = false;
        Context.UI.SkipDialog.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerPrefs.SetInt(CompletionKey, 1);
        PlayerPrefs.Save();
        Context.UI.HideExplanation();
        if (!string.IsNullOrWhiteSpace(CompletionLog)) {
#if UNITY_EDITOR
            Debug.Log(CompletionLog, this);
#endif
        }
        OnCompleted.Invoke();
    }

    public void GoToNextScene(string sceneName)
    {
        SceneFade.Instance._destinationScene = sceneName;
        SceneFade.Instance.DestroeyBeforeLoad = GameManager.Instance.gameObject;
        SceneFade.Instance.TransitionToScene();
    }

    public void NotifyPracticeShot() { if (_initialized && !_completed) Context.NotifyShot(); }

    private void OnDestroy()
    {
        if (IsPaused) Time.timeScale = _resumeTimeScale;
        Context?.Dispose();
        if (!_initialized) return;
        Context.UI.SkipButton.onClick.RemoveListener(Skip);
        Context.UI.ContinueButton.onClick.RemoveListener(CancelSkip);
    }
}
