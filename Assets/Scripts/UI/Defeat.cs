using UnityEngine;
using UnityEngine.SceneManagement;

public class Defeat : MonoBehaviour
{
    [Header("Configurações de Cena")]
    [SerializeField] private string nomeDaCenaMenu = "Menu";

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeDaCenaMenu);
    }

    public void ReiniciarFase()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
