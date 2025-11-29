using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisnovellaManager : MonoBehaviour
{
    [System.Serializable]
    public class Line
    {
        public string speaker;
        [TextArea] public string text;
        public Sprite portrait;
    }

    [System.Serializable]
    public class Phase
    {
        public string phaseName;
        public Line[] lines;
    }

    [Header("UI Components")]
    public Image portraitUI;
    public Text speakerUI;
    public SpeechTextMesh textPrinter;

    [Header("Phases")]
    public Phase[] phases;

    private int currentPhaseIndex = -1;
    private int currentLineIndex = 0;
    private Coroutine printRoutine;
    [Header("Optional Objects to Disable After Phase")]
    public GameObject objectToDisableAfterPhase;

    /// <summary>
    /// Запуск указанной фазы
    /// </summary>
    public void StartPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= phases.Length)
        {
            Debug.LogWarning("Phase index out of range");
            return;
        }

        currentPhaseIndex = phaseIndex;
        currentLineIndex = 0;
        gameObject.SetActive(true); // включаем UI
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        var lines = phases[currentPhaseIndex].lines;
        if (currentLineIndex >= lines.Length)
        {
            // Фаза закончена
            gameObject.SetActive(false); // твой основной UI диалога
            if (objectToDisableAfterPhase != null)
                objectToDisableAfterPhase.SetActive(false); // отключаем дополнительный объект
            printRoutine = null;
            return;
        }


        var line = lines[currentLineIndex];

        if (portraitUI != null) portraitUI.sprite = line.portrait;
        if (speakerUI != null) speakerUI.text = line.speaker;

        // Прерываем старую печать, если есть
        if (printRoutine != null)
            StopCoroutine(printRoutine);

        printRoutine = StartCoroutine(PrintRoutine(line.text));
    }

    IEnumerator PrintRoutine(string text)
    {
        textPrinter.Clear();
        textPrinter.Set(text);
        yield return new WaitUntil(() => textPrinter.rawText == textPrinter.processedText);
        printRoutine = null;
    }

    /// <summary>
    /// Переход к следующей линии
    /// </summary>
    public void AdvanceDialogue()
    {
        if (printRoutine != null)
        {
            // досрочно выводим текст текущей линии
            textPrinter.FinishInstantly();
            StopCoroutine(printRoutine);
            printRoutine = null;
            return; // нужно второе нажатие для перехода
        }

        currentLineIndex++;
        ShowCurrentLine();
    }
}
