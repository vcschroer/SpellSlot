using System.Collections; 
using UnityEngine;
using UnityEngine.SceneManagement;
public class Defeat : MonoBehaviour
{
    [Header("Configurações de Cena")]
    [SerializeField] private string nomeDaCenaMenu = "Menu";

    [Header("Tempo Automático")]
    [Tooltip("Tempo em segundos antes de voltar ao menu automaticamente")]
    [SerializeField] private float tempoEspera = 10f;

    private void Start()
    {
        StartCoroutine(RotinaContagemRegressiva());
    }

    private IEnumerator RotinaContagemRegressiva()
    {
        yield return new WaitForSecondsRealtime(tempoEspera);

        VoltarAoMenu();
    }

    public void VoltarAoMenu()
    {
        UnityEngine.Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void ReiniciarFase()
    {
        UnityEngine.Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}