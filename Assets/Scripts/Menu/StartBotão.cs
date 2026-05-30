using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBotão : MonoBehaviour
{
    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MeuTeste();
        }
    }

    void MeuTeste()
    {
        Debug.Log("Ta funcionando pai");
    }
}
