using UnityEngine;
using System.Collections;

public class AutomaticDoor : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;

    private bool isOpen = false;
    private bool isDone = true;

    // Номера слоёв (поставь свои, если другие)
    private int playerLayer;
    private int enemyLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidLayer(other.gameObject.layer))
            TryOpen(true);
    }

    private void OnTriggerExit(Collider other)
    {
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

