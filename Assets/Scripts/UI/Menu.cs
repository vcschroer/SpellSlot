using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private string nomeDaCenaDoJogo = "Game";
    [SerializeField] private GameObject painelPrincipal;
    [SerializeField] private GameObject painelOpcoes;
    public void IniciarJogo()
    {
        SceneManager.LoadScene(nomeDaCenaDoJogo);
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }
    public void AbrirOpcoes()
    {
        painelPrincipal.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void VoltarMenu()
    {
        painelPrincipal.SetActive(true);
        painelOpcoes.SetActive(false);
    }
}
