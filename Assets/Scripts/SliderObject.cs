using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Random = UnityEngine.Random;

public class SliderObject : MonoBehaviour {
    public float Value {
        get => _displayedValue;
        set {
            _displayedValue = value;
            if (MaxValue > 0) {
                Fill.transform.localScale = new Vector3(Value / MaxValue, 1, 0);
                Fill.transform.localPosition = new Vector3(2.5F * (1 - Fill.transform.localScale.x), 0, 0);
            }
        }
    }

    public float MinValue {
        get => _minValue;
        set => _minValue = value;
    }

    public float MaxValue {
        get => _maxValue;
        set => _maxValue = value;
    }

    private GameObject Fill => transform.Find("Fill").gameObject;

    private Creature Creature => transform.parent.parent.GetComponent<Creature>();

    private float _displayedValue;
    private float _minValue;
    private float _maxValue;

    private void Update() {
        MinValue = 0;
        MaxValue = Creature.MaxHp;
        Value = Creature.HealthPoints;
    }
}