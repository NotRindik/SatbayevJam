using UnityEngine;
using DG.Tweening;

[DefaultExecutionOrder(20)]
public class AudioScaleReactionWithFreq : MonoBehaviour
{
    [Header("Настройки скейла")]
    public Vector3 maxScaleMultiplier = new Vector3(1.2f, 1.2f, 1.2f);
    public float smoothTime = 0.1f;

    [Header("Настройки аудио")]
    public int sampleSize = 1024; // размер массива спектра
    public float minFrequency = 100f; // нижняя граница фильтра в Гц
    public float maxFrequency = 1000f; // верхняя граница фильтра в Гц

    private Vector3 initialScale;
    private AudioSource audioSource;
    private float[] spectrum;
    private float currentAmplitude;

    private void Awake()
    {
        initialScale = transform.localScale;
        spectrum = new float[sampleSize];
    }

    private void Start()
    {
        audioSource = AudioManager.instance._channels[0].activeTrack.source;
    }

    private void Update()
    {
        // Получаем спектр аудио
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // Рассчитываем амплитуду выбранного диапазона частот
        currentAmplitude = GetAmplitudeInFrequencyRange(minFrequency, maxFrequency);

        // Рассчитываем целевой скейл
        Vector3 targetScale = initialScale + (maxScaleMultiplier - Vector3.one) * currentAmplitude;

        // Плавное изменение скейла
        transform.DOScale(targetScale, smoothTime).SetEase(Ease.OutSine);
    }

    private float GetAmplitudeInFrequencyRange(float minFreq, float maxFreq)
    {
        float sum = 0f;
        int count = 0;

        float sampleRate = AudioSettings.outputSampleRate;
        float freqPerSample = sampleRate / 2f / sampleSize; // каждая ячейка спектра соответствует этому диапазону

        for (int i = 0; i < sampleSize; i++)
        {
            float freq = i * freqPerSample;
            if (freq >= minFreq && freq <= maxFreq)
            {
                sum += spectrum[i];
                count++;
            }
        }

        return count > 0 ? sum / count * 50f : 0f; // умножаем на коэффициент для наглядного эффекта
    }
}
