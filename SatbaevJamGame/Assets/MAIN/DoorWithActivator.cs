using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public class DoorWithActivator : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;

    // два условия для активации двери
    [Header("Activation Conditions")]
    public bool conditionA = false;
    public bool conditionB = false;
    public UnityEvent ConditionFinish;

    private bool isUnlocked = false;   // дверь активируется навсегда
    private bool isOpen = false;
    private bool isDone = true;

    private int playerLayer;
    private int enemyLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }
    public bool once;
    private void Update()
    {
        // как только оба условия стали true — дверь активируется
        if (!isUnlocked && conditionA && conditionB)
        {
            if (!once)
            {
                ConditionFinish?.Invoke();
                once = true;    
            }
            isUnlocked = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isUnlocked)
            return;

        if (IsValidLayer(other.gameObject.layer))
            TryOpen(true);
    }

    public void LoadScene2()
    {
        PlayerPrefs.SetInt("IsGaming", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Level2");
    }
    public void ActivateA()
    {
        conditionA = true;
    }
    public void ActivateB()
    {
        conditionB = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!isUnlocked)
            return;

        if (IsValidLayer(other.gameObject.layer))
            TryOpen(false);
    }

    private bool IsValidLayer(int layer)
    {
        return layer == playerLayer || layer == enemyLayer;
    }

    private void TryOpen(bool open)
    {
        if (!isDone)
            return;

        isOpen = open;
        doorAnimator.SetBool("interact", isOpen);
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        isDone = false;
        yield return new WaitForSeconds(1f);
        isDone = true;
    }
}
