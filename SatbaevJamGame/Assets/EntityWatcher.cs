using UnityEngine;
using System.Collections.Generic;
using Systems;

public class EntitiesHealthMonitor : MonoBehaviour
{
    public List<Entity> entitiesToWatch;
    public CanvasGroup canvasGroup;
    public AudioClip finalMusica;

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

        TimeDataManager.Instance.uIManager.isTimerRunning = false;
        
        AudioManager.instance.PlayMusic(finalMusica);
        canvasGroup.alpha = allDead ? 1f : 0f;
    }
}
