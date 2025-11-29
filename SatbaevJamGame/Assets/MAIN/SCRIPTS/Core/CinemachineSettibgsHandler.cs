using Unity.Cinemachine;
using UnityEngine;

public class CinemachineSettibgsHandler : MonoBehaviour
{
    CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    public void ManualUpdate()
    {
        cinemachineCamera.PreviousStateIsValid = false;
    }
}
