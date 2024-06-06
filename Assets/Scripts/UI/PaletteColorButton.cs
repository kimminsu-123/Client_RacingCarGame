using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaletteColorButton : MonoBehaviour, IPointerClickHandler
{
    public Color color = Color.white;
    public UnityEvent<Color> onClickColor;
    
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
        onClickColor?.Invoke(color);
    }
}
