using DG.Tweening;
using System;
using Systems;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Entity entity;
    private HealthComponent _hpc;
    private Action<float> healthChangeAction;
    public Slider hpSlider;

    private void Start()
    {
/*        healthChangeAction = h =>
        {
            hpSlider.DOKill();
            hpSlider.DOValue(h,0.2f).SetEase(Ease.InBounce);
        };


        _hpc = entity.GetControllerComponent<HealthComponent>();
        _hpc.OnCurrHealthDataChanged += healthChangeAction;

        hpSlider.maxValue = _hpc.maxHealth;
        hpSlider.value = _hpc.maxHealth;*/
    }
}
