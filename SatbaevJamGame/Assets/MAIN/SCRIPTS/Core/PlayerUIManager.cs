using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Entity entity;

    [Header("Timer Settings")]
    public float maxTime = 100f;       // Максимальное время
    public float startTime = 600f;     // Стартовое значение
    public float timeLossRate = 5f;    // Сколько времени теряется в секунду
    public GameObject player;
    [Header("UI")]
    public Slider TimeSlider;

    private float currentTime;
    private bool isTimerRunning = false;

    private void Start()
    {
        if (PlayerPrefs.GetInt("IsGaming", 0) == 1)
        {
            currentTime = startTime;

            TimeSlider.maxValue = maxTime;
            TimeSlider.value = currentTime;
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isTimerRunning) return; // таймер выключен ? не считаем

        // Постоянная потеря времени (управляемая)
        currentTime -= timeLossRate * Time.deltaTime;

        // Ограниения
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);

        // Обновление UI
        TimeSlider.value = currentTime;

        if (currentTime <= 0f)
        {
            OnTimeOut();
        }
    }

    // --- МЕТОД ЗАПУСКА ТАЙМЕРА ---

    public void StartTimer()
    {
        if (isTimerRunning) return;   // уже работает — не запускаем повторно

        isTimerRunning = true;
        currentTime = startTime;

        TimeSlider.maxValue = maxTime;
        TimeSlider.value = currentTime;

        Debug.Log("Timer started!");
    }

    // --- Методы изменения времени вручную ---

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

    // --- Методы изменения скорости потери времени ---

    public void ModifyTimeLossRate(float amount)
    {
        timeLossRate += amount;
        timeLossRate = Mathf.Max(0f, timeLossRate);
    }

    public void SetTimeLossRate(float newRate)
    {
        timeLossRate = Mathf.Max(0f, newRate);
    }

    private void OnTimeOut()
    {
        Debug.Log("TIME IS OVER!");
        player.GetComponent<PlayerEntity>().SetDeathAnim();
        // Фиксируем, что игрок теперь "в игре"
        PlayerPrefs.SetInt("IsGaming", 1);
        PlayerPrefs.Save();

        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
