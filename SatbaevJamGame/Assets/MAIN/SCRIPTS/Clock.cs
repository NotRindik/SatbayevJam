using DG.Tweening;
using System.Collections;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    public float time;
    public int turns = 6;
    public float period = 5;
    public PlayerUIManager playerUIManager;
    public RectTransform arrowHandler,heart;

    public Image[] rings;
    float timeVelocity;
    private void Start()
    {
        StartCoroutine(HeartBeat());
    }
    void Update()
    {
        float newTime = playerUIManager.currentTime;
        timeVelocity = Mathf.Abs(newTime - time) / Time.unscaledTime;
        time = newTime;
        period = playerUIManager.maxTime / turns;


        int cycle = Mathf.FloorToInt(time / period);
        float phase = Mathf.Repeat(time, period) / period;
        arrowHandler.rotation = Quaternion.Euler(0, 0, -phase * 360f);
        UpdateRings( cycle,  phase);
    }

    public IEnumerator HeartBeat()
    {
        while (true)
        {
            heart.DOKill();

            heart.DOScale(1.1f, 0.1f)
                 .OnComplete(() => heart.DOScale(1f, 0.1f));

            float interval = Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(0f, 5f, timeVelocity));
            yield return new WaitForSeconds(interval);
        }
    }

    void UpdateRings(int cycle, float phase)
    {
        // предыдущий оборот
        rings[0].fillAmount = 1f;
        rings[0].color = GetCycleColor(cycle - 1);

        // текущий
        rings[1].fillAmount = phase;
        rings[1].color = GetCycleColor(cycle);

        // следующий
        rings[2].fillAmount = 0f;
        rings[2].color = GetCycleColor(cycle + 1);
    }
    Color GetCycleColor(int cycle)
    {
        if (cycle <= 0)
            return Color.white;

        float t = Mathf.Clamp01((float)cycle / turns);

        // Hue: от голубого (0.55) к красному (0.0)
        float hue = Mathf.Lerp(0.55f, 0.0f, t);

        // Насыщенность растёт, но не кислотная
        float sat = Mathf.Lerp(0.35f, 0.9f, t);

        // Яркость слегка уменьшается
        float val = Mathf.Lerp(1f, 0.9f, t);

        return Color.HSVToRGB(hue, sat, val);
    }

}
