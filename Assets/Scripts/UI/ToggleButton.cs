using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public TMP_Text text;
    public string onText;
    public Color onColor = Color.white;
    public string offText;
    public Color offColor = Color.white;
    
    public UnityEvent<bool> onValueChanged;

    public float canToggleDelay = 1f;

    [SerializeField] private bool value;
    public bool Value
    {
        get => value;
        set
        {
            this.value = value;

            _image.color = this.value ? onColor : offColor;
            text.text = this.value ? onText : offText;
            
            onValueChanged?.Invoke(value);
        }
    }

    private Image _image;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        Value = value;
        onValueChanged?.Invoke(Value);
    }

    private void Start()
    {
        _button.onClick.AddListener(() =>
        {
            Value = !Value;
            
            StartCoroutine(ApplyInteractIntervalCoroutine());
        });
    }
    
    private IEnumerator ApplyInteractIntervalCoroutine()
    {
        _button.interactable = false;
        
        yield return new WaitForSeconds(canToggleDelay);
        
        _button.interactable = true;
    }
}