using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    [Header("Configurações de Créditos")]
    [SerializeField] private RectTransform containerCreditos; 
    [SerializeField] private float velocidadeScroll = 50f;
    [SerializeField] private float limiteDeParadaY = 800f; 

    [Header("Configurações de Menu")]
    [SerializeField] private string nomeDaCenaMenu = "Menu";

    private bool rolando = true;

    void Update()
    {
        if (rolando && containerCreditos != null)
        {
            containerCreditos.anchoredPosition += Vector2.up * velocidadeScroll * Time.deltaTime;

            if (containerCreditos.anchoredPosition.y >= limiteDeParadaY)
            {
                rolando = false;
            }
        }
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeDaCenaMenu);
    }
}