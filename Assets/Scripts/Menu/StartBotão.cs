using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBotão : MonoBehaviour
{
    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}
