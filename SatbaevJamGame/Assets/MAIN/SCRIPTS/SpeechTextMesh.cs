using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(ContentSizeFitter))]
public class SpeechTextMesh : MonoBehaviour
{
    public int characterStartSize = 1;
    public float characterAnimateSpeed = 1000f;
    public string textToLoad;
    public Image bubbleBackground;
    public float backgroundMinimumHeight;
    public float backgroundVerticalMargin;

    private string _rawText;
    public string rawText => _rawText;

    private string _processedText;
    public string processedText => _processedText;
    public float delay = 0.35f; // задержка перед запуском Set

    private void OnEnable()
    {
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        if(label.text != "")
            textToLoad = label.text;
        label.text = "";
        if (textToLoad != null)
        {
            StartCoroutine(DelayedSet());
        }
    }

    private void OnDisable()
    {
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        label.text = "";
    }

    private IEnumerator DelayedSet()
    {
        yield return new WaitForSeconds(delay);
        Set(textToLoad);
    }

    public void Set(string text)
    {
        StopAllCoroutines();
        StartCoroutine(SetRoutine(text));
    }

    public IEnumerator SetRoutine(string text)
    {
        _rawText = text;
        yield return StartCoroutine(TestFit());
        yield return StartCoroutine(CharacterAnimation());
    }

    private IEnumerator TestFit()
    {
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();

        float alpha = label.color.a;
        label.color = new Color(label.color.r, label.color.g, label.color.b, 0f);

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        label.text = _rawText;

        yield return null;
        Canvas.ForceUpdateCanvases();

        yield return new WaitForEndOfFrame();

        float totalHeight = Mathf.Max(label.preferredHeight, 0f);

        if (bubbleBackground != null)
        {
            bubbleBackground.rectTransform.sizeDelta = new Vector2(
                bubbleBackground.rectTransform.sizeDelta.x,
                Mathf.Max(totalHeight + backgroundVerticalMargin, backgroundMinimumHeight));
        }

        _processedText = "";
        string buffer = "";
        string line = "";
        float currentHeight = -1f;

        foreach (string word in _rawText.Split(' '))
        {
            buffer += word + " ";
            label.text = buffer;
            yield return new WaitForEndOfFrame();
            if (currentHeight < 0f)
            {
                currentHeight = label.preferredHeight;
            }

            float newHeight = label.preferredHeight;
            if (Mathf.Abs(newHeight - currentHeight) > 0.01f)
            {
                currentHeight = newHeight;
                _processedText += line.TrimEnd(' ') + "\n";
                line = "";
            }
            line += word + " ";
        }
        _processedText += line;

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        label.text = "";
        label.rectTransform.sizeDelta = new Vector2(label.rectTransform.sizeDelta.x, totalHeight);
        label.color = new Color(label.color.r, label.color.g, label.color.b, alpha);
    }
    public void Clear()
    {
        StopAllCoroutines();
        _rawText = "";
        _processedText = "";
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        label.text = "";
    }

    public void FinishInstantly()
    {
        StopAllCoroutines();
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        label.text = _processedText;
        _rawText = _processedText;
    }

    private IEnumerator CharacterAnimation()
    {
        TextMeshProUGUI label = GetComponent<TextMeshProUGUI>();
        float targetFontSize = label.fontSize;

        string prefix = "";
        foreach (char c in _processedText.ToCharArray())
        {
            float size = characterStartSize;
            while (size < targetFontSize)
            {
                size += Time.deltaTime * characterAnimateSpeed;
                size = Mathf.Min(size, targetFontSize);
                label.text = prefix + $"<size={size:F0}>" + c + "</size>";
                yield return new WaitForEndOfFrame();
            }
            prefix += c;
        }

        label.text = _processedText;
    }
}
