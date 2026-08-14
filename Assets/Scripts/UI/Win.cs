using System.Collections; 
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

    [Tooltip("Tempo em segundos que o jogo espera após os créditos pararem antes de voltar ao menu")]
    [SerializeField] private float tempoEsperaPosCreditos = 5f;

    private bool rolando = true;

    void Update()
    {
        if (rolando && containerCreditos != null)
        {
            containerCreditos.anchoredPosition += Vector2.up * velocidadeScroll * UnityEngine.Time.deltaTime;

            if (containerCreditos.anchoredPosition.y >= limiteDeParadaY)
            {
                rolando = false;
                StartCoroutine(RotinaEsperaMenu());
            }
        }
    }

    private IEnumerator RotinaEsperaMenu()
    {
        yield return new WaitForSecondsRealtime(tempoEsperaPosCreditos);

        VoltarAoMenu();
    }

    public void VoltarAoMenu()
    {
        UnityEngine.Time.timeScale = 1f;
        SceneManager.LoadScene(nomeDaCenaMenu);
    }
}