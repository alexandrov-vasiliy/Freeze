using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-100)]
public class Main : MonoBehaviour
{
    [SerializeField] private DeathPanel deathPanel;
    [SerializeField] private MainMenu mainMenu;
    private void Awake()
    {
        ResourceService resourceService = new ResourceService();
        G.Resources = resourceService;
        G.DeathPanel = deathPanel;
        G.mainMenu = mainMenu;
    }
}
