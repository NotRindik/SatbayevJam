using UnityEngine;
using System.Collections.Generic;
using Systems;

public class EntitiesHealthMonitor : MonoBehaviour
{
    public List<Entity> entitiesToWatch;
    public CanvasGroup canvasGroup;
    public AudioClip finalMusica;

    public bool once;
    private void Update()
    {
        bool allDead = true;

        for (int i = 0; i < entitiesToWatch.Count; i++)
        {
            // Берём компонент КАЖДУЮ итерацию
            var hc = entitiesToWatch[i].GetControllerComponent<HealthComponent>();
            if (hc == null)
                continue;

            if (hc.currHealth > 0) // жив?
            {
                allDead = false;
                break; // можно выйти
            }
        }

        if(allDead )
        {
            canvasGroup.alpha = 1f;
            TimeDataManager.Instance.uIManager.isTimerRunning = false;
            if (!once)
            {
                once = true;
                AudioManager.instance.PlayMusic(finalMusica);
            }
        }
    }
}
