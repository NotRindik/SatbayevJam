using UnityEngine;
using DG.Tweening;

public class MoveSequence : MonoBehaviour
{
    [Header("Настройки движения")]
    public bool playOnStart = true;
    public bool loop = false;
    public float duration = 1f; // время на каждое перемещение
    public Ease easeType = Ease.Linear;

    [Header("Позиции (от текущей)")]
    public Vector3[] positions;

    private Sequence moveSequence;

    private void Start()
    {
        if (playOnStart)
            PlaySequence();
    }

    /// <summary>
    /// Создаёт и запускает последовательность анимаций
    /// </summary>
    public void PlaySequence()
    {
        // Если последовательность уже есть, уничтожаем
        moveSequence?.Kill();

        moveSequence = DOTween.Sequence();

        Vector3 startPos = transform.position;

        foreach (var pos in positions)
        {
            // Анимируем от текущей позиции к следующей
            moveSequence.Append(transform.DOMove(startPos + pos, duration).SetEase(easeType));
            startPos += pos;
        }

        if (loop)
        {
            moveSequence.SetLoops(-1, LoopType.Restart);
        }

        moveSequence.Play();
    }

    /// <summary>
    /// Останавливает анимацию
    /// </summary>
    public void StopSequence()
    {
        moveSequence?.Kill();
    }
}
