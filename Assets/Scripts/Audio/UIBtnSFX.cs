using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnSFX : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("音效 Key 配置")]
    [AudioKey] public string hoverSound;
    [AudioKey] public string clickValidSound;
    [AudioKey] public string clickInvalidSound;

    [SerializeField]private Button _button;

    private void OnValidate()
    {
        _button = GetComponent<Button>();
    }

    private Button Button => _button != null ? _button : _button = GetComponent<Button>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Button != null && !string.IsNullOrEmpty(hoverSound))
        {
            AudioManager.Instance.PlaySFX(hoverSound);
        }
    }

    // 改为 OnPointerDown 而非 OnPointerClick
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Button == null) return;

        if (Button.interactable)
        {
            if (!string.IsNullOrEmpty(clickValidSound))
                AudioManager.Instance.PlaySFX(clickValidSound);
        }
        else
        {
            if (!string.IsNullOrEmpty(clickInvalidSound))
                AudioManager.Instance.PlaySFX(clickInvalidSound);
        }
    }

}