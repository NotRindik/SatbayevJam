using UnityEngine;

public class LightHandler : MonoBehaviour
{
    public Color color;
    private Light ligh;

    private void Start()
    {
        ligh = GetComponent<Light>();
    }

    public void ChangeColor()
    {
        ligh.color = color;
    }
}
