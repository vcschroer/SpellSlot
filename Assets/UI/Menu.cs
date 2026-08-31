using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private string nomeDaCenaDoJogo = "Game";

    [Header("Painéis do Menu")]
    [SerializeField] private GameObject painelPrincipal;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelTutorial; 

    public void IniciarJogo()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.CarregarCena(nomeDaCenaDoJogo);
        }
        else
        {
            SceneManager.LoadScene(nomeDaCenaDoJogo);
        }
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }

    public void AbrirOpcoes()
    {
        painelPrincipal.SetActive(false);
        painelOpcoes.SetActive(true);
        if (painelTutorial != null) painelTutorial.SetActive(false);
    }

    public void AbrirTutorial() 
    {
        painelPrincipal.SetActive(false);
        painelOpcoes.SetActive(false); 
        if (painelTutorial != null) painelTutorial.SetActive(true);
    }

    public void VoltarMenu()
    {
        painelPrincipal.SetActive(true);
        painelOpcoes.SetActive(false);
        if (painelTutorial != null) painelTutorial.SetActive(false); 
    }
}