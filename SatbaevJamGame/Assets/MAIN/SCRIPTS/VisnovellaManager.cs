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

    // ? Автозапуск первой фазы
    private void Start()
    {
        // Если уже в игре — сразу выходим из новеллы
        if (PlayerPrefs.GetInt("IsGaming", 0) == 1)
            return;

        if (phases != null && phases.Length > 0)
            StartPhase(0);
        else
            Debug.LogWarning("No phases assigned!");
    }

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

        gameObject.SetActive(true);
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        var lines = phases[currentPhaseIndex].lines;

        if (lines == null || lines.Length == 0)
        {
            EndPhase();
            return;
        }

        if (currentLineIndex >= lines.Length)
        {
            EndPhase();
            return;
        }

        var line = lines[currentLineIndex];

        if (portraitUI != null) portraitUI.sprite = line.portrait;
        if (speakerUI != null) speakerUI.text = line.speaker;

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

    void EndPhase()
    {
        gameObject.SetActive(false);

        if (objectToDisableAfterPhase != null)
            objectToDisableAfterPhase.SetActive(false);

        printRoutine = null;
    }

    /// <summary>
    /// Переход к следующей линии
    /// </summary>
    public void AdvanceDialogue()
    {
        if (printRoutine != null)
        {
            textPrinter.FinishInstantly();
            StopCoroutine(printRoutine);
            printRoutine = null;
            return;
        }

        currentLineIndex++;
        ShowCurrentLine();
    }
}
