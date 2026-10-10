using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class TutorialEnemyDefinition
{
    public BaseEnemy Prefab;
    public Transform SpawnPoint;
    public Transform Destination;
    [Tooltip("도착 지점 이후의 경유 위치. 마지막 Transform이 캐슬 골인지점입니다.")]
    public Transform[] Route = new Transform[0];
    public bool HoldAtGoal;
    [Min(1f)] public float Health = 20f;
    [Min(0f)] public float Defence;
    [Min(0f)] public float Speed = 1.5f;
    [Min(0f)] public float CastleDamage = 10f;
    [Min(0)] public int Gold;
    [Min(0f)] public float Experience;
    public bool GrantNextLevel;
}

public enum TutorialWaveCompletion { FirstRemoved, AllRemoved, AllKilled, CastleArrival, LeavePaused }

[Serializable]
public class TutorialWaveStep : TutorialStep
{
    public string WaveId;
    public bool ContinueExistingWave;
    public TutorialEnemyDefinition[] Enemies = new TutorialEnemyDefinition[0];
    public TutorialWaveCompletion Completion;
    [Min(0f)] public float BeforeSpawn;
    [Min(0f)] public float SpawnInterval = 1f;
    [Min(0f)] public float AfterWave;
    public bool ShowSpawnCamera;
    public bool ShowMarker;
    public string MarkerLabel;
    public TutorialPresentation ArrivalText = new TutorialPresentation();
    public TutorialControls Controls = new TutorialControls();
    public BuildPoint AvailableBuildPoint;
    public bool RetryIfLeaked;
    public TutorialPresentation BeforeCastleText = new TutorialPresentation();
    public TutorialPresentation CastleText = new TutorialPresentation();
    [Min(0f)] public float BeforeCastleHit = 0.8f;
    [Min(0f)] public float AfterCastleHit = 1.5f;

    public override IEnumerator Execute(TutorialContext context)
    {
        List<BaseEnemy> wave = ContinueExistingWave ? context.GetWave(WaveId) : context.CreateWave(WaveId);
        if (wave == null) yield break;
        context.BuildAreas.Allow(AvailableBuildPoint);
        context.Input.InputEnabled = false;
        yield return context.Wait(BeforeSpawn);
        if (ContinueExistingWave)
            foreach (BaseEnemy existing in wave)
                if (TutorialContext.IsAlive(existing)) existing.GetComponent<NavMeshAgent>().isStopped = false;

        bool retry;
        do
        {
            retry = false;
            for (int i = 0; i < Enemies.Length; i++)
            {
                BaseEnemy enemy = context.Spawn(Enemies[i]);
                if (enemy == null) yield break;
                wave.Add(enemy);
                if (Completion == TutorialWaveCompletion.LeavePaused)
                    enemy.GetComponent<NavMeshAgent>().isStopped = true;
                if (i == 0 && ShowSpawnCamera)
                {
                    context.Input.InputEnabled = false;
                    ArrivalText.Show(context);
                    yield return context.Camera.Play(context.Player, enemy);
                }
                Controls.Apply(context);
                Text.Show(context);
                if (i == 0 && ShowMarker) context.GuideEnemy(enemy, MarkerLabel);
                if (i + 1 < Enemies.Length) yield return context.Wait(SpawnInterval);
            }
            if (wave.Count == 0) { context.Fail($"웨이브 {WaveId}에 몬스터가 없습니다."); yield break; }
            Controls.Apply(context);
            Text.Show(context);
            if (!ShowMarker) context.HideEnemyGuide();
            if (Completion == TutorialWaveCompletion.LeavePaused) yield break;

            if (Completion == TutorialWaveCompletion.CastleArrival)
            {
                BaseEnemy enemy = wave[0];
                TutorialEnemyRoute route = enemy.GetComponent<TutorialEnemyRoute>();
                if (route == null) { context.Fail("캐슬 시연 웨이브에 이동 경로를 지정하세요."); yield break; }
                yield return new WaitUntil(() => !context.Paused && (!TutorialContext.IsAlive(enemy) || route.Arrived || route.Failed));
                if (route.Failed || !TutorialContext.IsAlive(enemy) || enemy.MonHp <= 0f)
                { context.Fail("캐슬 피해 시연 몬스터가 골인 전에 죽었거나 경로가 끊겼습니다."); yield break; }
                context.Input.InputEnabled = false;
                context.Camera.BeginView(context.Player, enemy.transform.position + Vector3.up, new Vector3(-4f, 4f, -6f));
                BeforeCastleText.Show(context);
                yield return context.Wait(BeforeCastleHit);
                route.ResolveGoal();
                CastleText.Show(context);
                yield return context.Wait(AfterCastleHit);
                context.Camera.RestoreCamera();
            }
            else
            {
                while (context.Paused || !Finished(wave))
                {
                    if (wave.Exists(RouteFailed)) { context.Fail($"웨이브 {WaveId}의 이동 경로가 끊겼습니다."); yield break; }
                    if (ShowMarker)
                    {
                        BaseEnemy next = wave.Find(TutorialContext.IsAlive);
                        if (next != null) context.GuideEnemy(next, MarkerLabel);
                    }
                    yield return null;
                }
                if (wave.Exists(RouteFailed)) { context.Fail($"웨이브 {WaveId}의 이동 경로가 끊겼습니다."); yield break; }
                if (Completion == TutorialWaveCompletion.AllKilled && wave.Exists(enemy => enemy != null && enemy.MonHp > 0f))
                {
                    if (!RetryIfLeaked || ContinueExistingWave || Enemies.Length == 0)
                    { context.Fail($"웨이브 {WaveId}에서 몬스터가 골인했습니다."); yield break; }
                    wave = context.CreateWave(WaveId);
                    if (wave == null) yield break;
                    retry = true;
                }
            }
        } while (retry);

        if (Completion == TutorialWaveCompletion.FirstRemoved && ShowMarker)
        {
            BaseEnemy next = wave.Find(TutorialContext.IsAlive);
            if (next != null) context.GuideEnemy(next, MarkerLabel);
            else context.HideEnemyGuide();
        }
        else context.HideEnemyGuide();
        yield return context.Wait(AfterWave);
    }

    private bool Finished(List<BaseEnemy> wave)
    {
        return Completion == TutorialWaveCompletion.FirstRemoved
            ? !TutorialContext.IsAlive(wave[0]) : wave.TrueForAll(enemy => !TutorialContext.IsAlive(enemy));
    }

    private static bool RouteFailed(BaseEnemy enemy)
    {
        TutorialEnemyRoute route = enemy != null ? enemy.GetComponent<TutorialEnemyRoute>() : null;
        return route != null && route.Failed;
    }
}
