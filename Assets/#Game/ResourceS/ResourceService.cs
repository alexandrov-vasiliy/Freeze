using System;
using System.Collections.Generic;

/// <summary>
/// Сервис хранения и изменения количества ресурсов. Событие OnResourceChanged для обновления UI.
/// </summary>
public class ResourceService
{
    private readonly Dictionary<ResourceType, int> _counts = new Dictionary<ResourceType, int>();

    /// <summary>
    /// Вызывается при изменении количества любого ресурса (передаётся тип изменившегося ресурса).
    /// </summary>
    public event Action<ResourceType> OnResourceChanged;

    public void Add(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return;

        if (!_counts.TryGetValue(resourceType, out int current))
            current = 0;

        _counts[resourceType] = current + amount;
        OnResourceChanged?.Invoke(resourceType);
    }

    public int GetCount(ResourceType resourceType)
    {
        return _counts.TryGetValue(resourceType, out int count) ? count : 0;
    }

    /// <summary>
    /// Проверяет, хватает ли ресурсов (без списания). Для подсказок UI и проверки перед действием.
    /// </summary>
    public bool CanSpend(ResourceType resourceType, int amount)
    {
        return GetCount(resourceType) >= amount;
    }

    /// <summary>
    /// Пытается списать ресурсы. Возвращает true и списывает при достаточном количестве, иначе false.
    /// </summary>
    public bool TrySpend(ResourceType resourceType, int amount)
    {
        if (GetCount(resourceType) < amount)
            return false;

        int current = _counts[resourceType];
        _counts[resourceType] = current - amount;
        OnResourceChanged?.Invoke(resourceType);
        return true;
    }
}
