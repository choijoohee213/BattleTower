using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GameObject ParentObj { get; set; }

    [SerializeField]
    private Image gaugeBar;
    public Image GaugeBar { get => gaugeBar; }

}
