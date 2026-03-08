using UnityEngine;
using TMPro;

public class InteractorDisplayer : MonoBehaviour
{
    public void Show(string text)
    {
        G.hintText.GetComponent<TMP_Text>().text = text;
        G.hintText.SetActive(true);
    }

    public void Hide()
    {
        G.hintText.SetActive(false);
    }
}
