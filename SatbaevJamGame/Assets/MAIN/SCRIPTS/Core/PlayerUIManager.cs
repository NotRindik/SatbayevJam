using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Entity entity;

    [Header("Timer Settings")]
    public float maxTime = 100f;       // ћаксимальное врем€
    public float startTime = 600f;     // —тартовое значение
    public float timeLossRate = 5f;    // —колько времени тер€етс€ в секунду

    [Header("UI")]
    public Slider TimeSlider;

    private float currentTime;

    private void Start()
    {
        currentTime = startTime;

        TimeSlider.maxValue = maxTime;
        TimeSlider.value = currentTime;
    }

    private void Update()
    {
        // ѕосто€нна€ потер€ времени (управл€ема€)
        currentTime -= timeLossRate * Time.deltaTime;

        // ќграниени€
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);

        // ќбновление UI
        TimeSlider.value = currentTime;

        if (currentTime <= 0f)
        {
            OnTimeOut();
        }
    }

    // --- ћетоды изменени€ времени вручную ---

    public void SpendTime(float amount)
    {
        currentTime -= amount;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);
    }

    public void AddTime(float amount)
    {
        currentTime += amount;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);
    }

    // --- ћетоды изменени€ скорости потери времени ---

    /// <summary>
    /// »змен€ет скорость посто€нной потери времени (положительное = быстрее убывает).
    /// </summary>
    public void ModifyTimeLossRate(float amount)
    {
        timeLossRate += amount;
        timeLossRate = Mathf.Max(0f, timeLossRate); // „тобы не ушло в отрицательное
    }

    /// <summary>
    /// ”станавливает конкретную скорость убывани€.
    /// </summary>
    public void SetTimeLossRate(float newRate)
    {
        timeLossRate = Mathf.Max(0f, newRate);
    }

    private void OnTimeOut()
    {
        Debug.Log("TIME IS OVER!");
        // сюда вставишь смерть или переход в меню
    }
}
