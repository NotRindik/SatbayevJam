using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        public UnityEvent onTriggerEnterEvent;

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
    public Entity entity;

    // ? Автозапуск первой фазы
    private void Start()
    {
        // Если уже в игре — сразу выходим из новеллы
        if (PlayerPrefs.GetInt("IsGaming", 0) == 1)
        {
            portraitUI.gameObject.SetActive(false);
            speakerUI.gameObject.SetActive(false);
            textPrinter.gameObject.SetActive(false);
        }
        InputManager.inputActions.Player.Jump.performed += c => AdvanceDialogue();

        if (phases != null && phases.Length > 0 && PlayerPrefs.GetInt("IsGaming", 0) != 1)
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
        entity.SetActiveAllSys(false);
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

        if(enabled && gameObject.activeSelf)
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
        // вызываем события фазы, если они есть
        phases[currentPhaseIndex].onTriggerEnterEvent?.Invoke();

        entity.SetActiveAllSys(true);
        // отключаем диалог
        gameObject.SetActive(false);

        // выключаем объект если надо
        if (objectToDisableAfterPhase != null)
            objectToDisableAfterPhase.SetActive(false);

        printRoutine = null;
    }


    /// <summary>
    /// Переход к следующей линии
    /// </summary>
    public void AdvanceDialogue()
    {
        Debug.Log("ABOBA");
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
