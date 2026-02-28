using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    public float time;
    public int turns = 1; // Для одного оборота ставим 1
    public float period = 100;
    public PlayerUIManager playerUIManager;
    public RectTransform arrowHandler, heart;

    [Header("Color Settings")]
    public Gradient clockGradient; // Настройка цвета от начала до конца
    public Color emptySpaceColor = Color.black; // Цвет "пустоты" за шкалой

    public Image[] rings; // 0 - фон, 1 - текущий прогресс, 2 - не используется в 1 цикле, но оставим
    float timeVelocity;

    private void Start()
    {
        StartCoroutine(HeartBeat());
    }

    void Update()
    {
        float newTime = playerUIManager.currentTime;
        // Считаем скорость изменения для сердца
        timeVelocity = Mathf.Abs(newTime - time) / Time.deltaTime;
        time = newTime;

        period = playerUIManager.maxTime / turns;

        // Вычисляем цикл и фазу (0.0 - 1.0)
        int cycle = Mathf.FloorToInt(time / period);
        float phase = Mathf.Repeat(time, period) / period;

        // --- ЖЕСТКАЯ СИНХРОНИЗАЦИЯ СТРЕЛКИ ---
        // В Unity Image.FillAmount (Radial 360) начинается сверху и идет по часовой стрелке.
        // Чтобы стрелка была точно на кончике Fill, используем phase.
        // Инвертируем в минус, так как UI Fill обычно идет по часовой (0 -> -360 градусов).
        arrowHandler.localRotation = Quaternion.Euler(0, 0, -phase * 360f);

        UpdateRings(cycle, phase);
    }

    void UpdateRings(int cycle, float phase)
    {
        // Фон (пустота)
        rings[0].fillAmount = 1f;
        rings[0].color = emptySpaceColor;

        // Текущий прогресс
        rings[1].fillAmount = phase; // Теперь стрелка и этот fillAmount используют одну переменную phase

        // Цвет зависит от общего времени (от 0 до MaxTime)
        float totalProgress = Mathf.Clamp01(time / playerUIManager.maxTime);
        rings[1].color = clockGradient.Evaluate(totalProgress);

        // Если есть дополнительные кольца, обнуляем их
        if (rings.Length > 2) rings[2].fillAmount = 0f;
    }

    public IEnumerator HeartBeat()
    {
        while (true)
        {
            heart.DOKill();
            heart.DOScale(1.1f, 0.1f).OnComplete(() => heart.DOScale(1f, 0.1f));

            // Пульсация: чем быстрее убывает время, тем чаще бьется
            float interval = Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(0f, 10f, timeVelocity));
            yield return new WaitForSeconds(interval);
        }
    }
}