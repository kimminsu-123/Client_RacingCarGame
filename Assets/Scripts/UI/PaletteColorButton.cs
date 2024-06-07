using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaletteColorButton : MonoBehaviour, IPointerClickHandler
{
    public Color color = Color.white;
    public UnityEvent<PaletteColorButton> onClickColor;
    public bool interactive = true;

    public float disableInterval = 1f;
    
    private Image _baseSprite;

    private void Awake()
    {
        _baseSprite = GetComponent<Image>();
    }

    private void Start()
    {
        _baseSprite.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactive) return;
        
        onClickColor?.Invoke(this);
    }

    public void ApplyInteractInterval()
    {
        StartCoroutine(ApplyInteractIntervalCoroutine());
    }

    private IEnumerator ApplyInteractIntervalCoroutine()
    {
        interactive = false;
        
        var alphaColor = color;
        alphaColor.a = 0.5f;
        _baseSprite.color = alphaColor;
        
        yield return new WaitForSeconds(disableInterval);
        
        _baseSprite.color = color;
        
        interactive = true;
    }
}
