using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;

interface IStopCoroutineSafely
{
    public void StopCoroutineSafe();
}

public class TimeDataManager : MonoBehaviour, IStopCoroutineSafely
{

    private static TimeDataManager instance;
    public static TimeDataManager Instance { get { return instance; } set { instance = value; } }
    public Dictionary<Entity,List<SaveTimeData>> saveTimeDatas;
    public Dictionary<Entity,Coroutine> entityReplayProcess = new(10);

    public List<Entity> entities = new List<Entity>();

    public float saveDelay = 1;
    private Coroutine _rePlayProcess;
    private Action<Entity> onFinish;
    public bool isReplay => _rePlayProcess != null;
    public float maxTime,timeReplayCooldown;
    Action<InputAction.CallbackContext> replayStart;
    Action<InputAction.CallbackContext> replayEnd;
    public int maxEntries => (int)Mathf.Ceil(maxTime / saveDelay);
    private Dictionary<Entity, TimeTrailRenderer> trails = new();
    public Material trailMaterial;
    public float trailWidth = 0.1f;
    private bool canReplay = true;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        saveTimeDatas = new(maxEntries);
        replayStart = c =>
        {
            RePlay();
        };
        replayEnd = c =>
        {
            if(canReplay)
                StopCoroutineSafe();
        };
    }
    public void Start()
    {
        InputManager.inputActions.Player.Replay.started += replayStart;
        InputManager.inputActions.Player.Replay.canceled += replayEnd;

        StartCoroutine(std.Utilities.InvokeRepeatedly(() =>
        {
            if(isReplay)
                return;
            for (int i = 0; i < entities.Count; i++)
            {
                Save(entities[i],new SaveTimeData(entities[i].transform.position, entities[i].Components, entities[i].transform.rotation));
            }
        }, saveDelay,1));
    }
    [Button]
    public void RePlay()
    {
        if(isReplay == false && canReplay)
        {
            _rePlayProcess = StartCoroutine(RePlaySavesProcess());
        }
    }

    public IEnumerator ReplayCoolDown()
    {
        canReplay = false;
        yield return new WaitForSeconds(timeReplayCooldown);
        canReplay = true;
    }
    private IEnumerator RePlaySavesProcess()
    {
        for(int i = 0;i < entities.Count; i++)
        {
            for (int j = 0; j < entities[i].Systems.Count; j++)
            {
                if(entities[i].Systems[j] is BaseSystem bs)
                {
                    bs.IsActive = false;
                }
            }
            var act = entities[i].GetControllerComponent<ReplayActions>();
            act?.OnReplayStart?.Invoke();
        }
        for (int i = 0; i < entities.Count; i++)
        {
            var ent = entities[i];
            var data = saveTimeDatas[ent];
            trails[ent] = new TimeTrailRenderer(ent, data, trailMaterial, trailWidth);
        }

        int finishes = 0;
        onFinish = e =>
        {
            for (int j = 0; j < e.Systems.Count; j++)
            {
                if (e.Systems[j] is BaseSystem bs)
                {
                    bs.IsActive = true;
                }
            }
            var act = e.GetControllerComponent<ReplayActions>();
            act?.OnReplayEnd?.Invoke();

            saveTimeDatas[e].Clear();
            finishes++;
            if (trails.ContainsKey(e))
            {
                trails[e].Destroy();
                trails.Remove(e);
            }
        };
        for (int i = 0; i < entities.Count; i++)
        {
            if(!entityReplayProcess.ContainsKey(entities[i])) 
                entityReplayProcess.Add(entities[i], StartCoroutine(EntityPosReplay(entities[i], onFinish)));
        }
        yield return new WaitUntil(() => finishes == entities.Count);

        _rePlayProcess = null;
        yield return ReplayCoolDown();
    }

    public IEnumerator AutoStop()
    {
        yield return new WaitForSecondsRealtime(maxTime);
        StopCoroutineSafe();
        print("AutoStop");
    }

    private IEnumerator EntityPosReplay(Entity ent, Action<Entity> finish)
    {
        List<SaveTimeData> data = saveTimeDatas[ent];
        int count = data.Count;
        if (count == 0)
        {
            finish?.Invoke(ent);
            yield break;
        }
        float duration = maxTime / (count - 1);

        // »дЄм с конца к началу
        for (int j = count - 1; j >= 0; j--)
        {
            Vector3 startPos = ent.transform.position;
            Quaternion startRot = ent.transform.rotation;

            Vector3 targetPos = data[j].position;
            Quaternion targetRot = data[j].rotation;

            float t = 0f;

            // ѕлавна€ интерпол€ци€ руками
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = t / duration;

                ent.transform.position = Vector3.Lerp(startPos, targetPos, lerp);
                ent.transform.rotation = Quaternion.Lerp(startRot, targetRot, lerp);

                if (trails.TryGetValue(ent, out var trail))
                    trail.FollowEntity(ent);

                yield return null;
            }

            ent.transform.position = targetPos;
            ent.transform.rotation = targetRot;
            ent.transform.DOKill();
            if (trails.TryGetValue(ent, out var trail2))
                trail2.FollowEntity(ent);
        }

        finish?.Invoke(ent);
    }
    void Save(Entity who, SaveTimeData data)
    {
        var list = saveTimeDatas[who];

        if (list.Count > maxEntries)
            list.RemoveAt(0);

        list.Add(data);
    }

    public void RegisterEntity(Entity entity)
    {
        entities.Add(entity);
        saveTimeDatas.Add(entity,new List<SaveTimeData>());
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void StopCoroutineSafe()
    {
        if (_rePlayProcess != null)
        {
            StopCoroutine(_rePlayProcess);
            _rePlayProcess = null;
        }

        foreach (var kvp in entityReplayProcess)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);

            onFinish?.Invoke(kvp.Key);
        }

        entityReplayProcess.Clear();
        StartCoroutine(ReplayCoolDown());
    }
}
[System.Serializable]
public struct SaveTimeData : IComponent
{
    public Vector3 position;
    public Quaternion rotation;
    public List<IComponent> component;

    public SaveTimeData(Vector3 position, List<IComponent> component,Quaternion rot)
    {
        this.position = position;
        rotation = rot;
        this.component = new List<IComponent>(component.Count);
        foreach (var c in component)
        {
            this.component.Add(c); // если есть Clone()
        }
    }
}

public class TimeTrailRenderer
{
    public LineRenderer line;
    public Vector3[] points;

    public TimeTrailRenderer(Entity ent, List<SaveTimeData> data, Material mat, float width)
    {
        int count = data.Count;
        points = new Vector3[count];

        for (int i = 0; i < count; i++)
            points[i] = data[i].position;

        GameObject go = new GameObject("Trail_" + ent.name);
        line = go.AddComponent<LineRenderer>();
        line.positionCount = count;
        line.SetPositions(points);
        line.material = mat;
        line.startWidth = line.endWidth = width;
        line.useWorldSpace = true;
    }
    
    public void FollowEntity(Entity ent)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (Vector3.Distance(points[i], ent.transform.position) < 5)
            {
                points[i] = ent.transform.position;
                line.SetPosition(i, points[i]);
            }
        }
    }

    public void Destroy()
    {
        if (line)
            UnityEngine.Object.Destroy(line.gameObject);
    }
}
