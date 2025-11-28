using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Systems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeDataManager : MonoBehaviour
{

    private static TimeDataManager instance;
    public static TimeDataManager Instance { get { return instance; } set { instance = value; } }
    public Dictionary<Entity,List<SaveTimeData>> saveTimeDatas = new(100);

    public List<Entity> entities = new List<Entity>();

    public float saveDelay = 1;
    private Coroutine _rePlayProcess;
    private Action<Entity> onFinish;
    public bool isReplay => _rePlayProcess != null;
    public bool preFinish;
    public float maxTime,timeReplayCooldown;
    Action<InputAction.CallbackContext> replayStart;
    Action<InputAction.CallbackContext> replayEnd;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        replayStart = c =>
        {
            RePlay();
        };
        replayEnd = c =>
        {
            preFinish = true;
        };
    }
    public void Start()
    {
        InputManager.inputActions.Player.Replay.started += replayStart;
        //InputManager.inputActions.Player.Replay.canceled += replayEnd;

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
        if(isReplay == false)
        {
            _rePlayProcess = StartCoroutine(RePlaySavesProcess());
        }
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
        Coroutine stopTime = null;
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
            preFinish = false;
            if(stopTime != null) 
                StopCoroutine(stopTime);

            stopTime = null;
        };
        for (int i = 0; i < entities.Count; i++)
        {
            StartCoroutine(EntityPosReplay(entities[i], onFinish));
        }
        stopTime = StartCoroutine(AutoStop());
        yield return new WaitUntil(() => finishes == entities.Count);

        yield return ReplayCoolDown();
    }
    
    public IEnumerator ReplayCoolDown()
    {
        yield return new WaitForSeconds(timeReplayCooldown);
        _rePlayProcess = null;
    }

    public IEnumerator AutoStop()
    {
        yield return new WaitForSecondsRealtime(maxTime);
        preFinish = true;
        print("AutoStop");
    }

    private IEnumerator EntityPosReplay(Entity ent, Action<Entity> finish)
    {
        List<SaveTimeData> data = saveTimeDatas[ent];
        int count = data.Count;

        float duration = 0.2f;

        // »дЄм с конца к началу
        for (int j = count - 1; j >= 0; j--)
        {
            if (preFinish)
                break;

            Vector3 startPos = ent.transform.position;
            Quaternion startRot = ent.transform.rotation;

            Vector3 targetPos = data[j].position;
            Quaternion targetRot = data[j].rotation;

            float t = 0f;

            // ѕлавна€ интерпол€ци€ руками
            while (t < duration)
            {
                if (preFinish)
                    break;

                t += Time.unscaledDeltaTime;
                float lerp = t / duration;

                ent.transform.position = Vector3.Lerp(startPos, targetPos, lerp);
                ent.transform.rotation = Quaternion.Lerp(startRot, targetRot, lerp);

                yield return null;
            }

            // Ќа вс€кий случай Ч зафиксировать точное значение
            ent.transform.position = targetPos;
            ent.transform.rotation = targetRot;
            Debug.Log(ent.name + " Was Moved");
            // —ледующий кадр истории
        }

        finish?.Invoke(ent);
    }


    private void Save(Entity who,SaveTimeData saveTimeData)
    {
        saveTimeDatas[who].Add(saveTimeData);
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
