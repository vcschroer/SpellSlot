using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Referências UI")]
    [SerializeField] private RectTransform circuloPreto;

    [Header("Configurações")]
    [SerializeField] private float duracaoTransicao = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            circuloPreto.gameObject.SetActive(false);
        }
        else
        {
            circuloPreto.gameObject.SetActive(true);
            circuloPreto.localScale = Vector3.one;
            StartCoroutine(RotinaEntrada());
        }
    }

    public void CarregarCena(string nomeDaCena)
    {
        StartCoroutine(RotinaMudarCena(nomeDaCena));
    }

    private IEnumerator RotinaMudarCena(string nomeDaCena)
    {
        circuloPreto.gameObject.SetActive(true);

        float tempo = 0f;
        circuloPreto.localScale = Vector3.zero;

        while (tempo < duracaoTransicao)
        {
            tempo += Time.deltaTime;
            circuloPreto.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, tempo / duracaoTransicao);
            yield return null;
        }
        circuloPreto.localScale = Vector3.one;

        AsyncOperation operacaoAsync = SceneManager.LoadSceneAsync(nomeDaCena);
        while (!operacaoAsync.isDone)
        {
            yield return null;
        }

        if (nomeDaCena == "Menu")
        {
            circuloPreto.gameObject.SetActive(false);
        }
        else
        {
            yield return StartCoroutine(RotinaEntrada());
        }
    }

    private IEnumerator RotinaEntrada()
    {
        float tempo = 0f;
        Vector3 escalaInicial = circuloPreto.localScale;

        while (tempo < duracaoTransicao)
        {
            tempo += Time.deltaTime;
            circuloPreto.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, tempo / duracaoTransicao);
            yield return null;
        }
        circuloPreto.localScale = Vector3.zero;
    }
}