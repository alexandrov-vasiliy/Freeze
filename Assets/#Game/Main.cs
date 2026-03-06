using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Main : MonoBehaviour
{
    private void Awake()
    {
        ResourceService resourceService = new ResourceService();
        G.Resources = resourceService;
    }
}
