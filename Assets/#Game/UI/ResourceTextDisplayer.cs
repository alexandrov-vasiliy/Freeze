using UnityEngine;
using TMPro;

/// <summary>
/// Отображает количество заданного ресурса в TextMeshPro. Подписывается на G.Resources и обновляет текст при изменении.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ResourceTextDisplayer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Тип ресурса, количество которого отображается")]
    private ResourceType resourceType;

    [SerializeField]
    [Tooltip("Поле текста для отображения количества. Если не задано — берётся с этого же объекта")]
    private TMP_Text text;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (G.Resources == null)
            return;

        G.Resources.OnResourceChanged += OnResourceChanged;
        RefreshText();
    }

    private void OnDisable()
    {
        if (G.Resources != null)
            G.Resources.OnResourceChanged -= OnResourceChanged;
    }

    private void OnResourceChanged(ResourceType changedType)
    {
        if (changedType == resourceType)
            RefreshText();
    }

    private void RefreshText()
    {
        if (text != null && G.Resources != null)
            text.text = G.Resources.GetCount(resourceType).ToString();
    }
}
