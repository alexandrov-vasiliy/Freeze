using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
public class Main : MonoBehaviour
{
    [SerializeField] private DeathPanel deathPanel;
    [SerializeField] private GameObject hint;
    [SerializeField] private SubtitlePlayer subtitle;
    private void Awake()
    {
        ResourceService resourceService = new ResourceService();
        resourceService.Add(ResourceType.Matches, 8);
        
        G.Resources = resourceService;
        G.DeathPanel = deathPanel;
        G.hintText = hint;
        G.subtitleText = subtitle;

    }
}
