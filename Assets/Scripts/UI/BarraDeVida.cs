using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BarraDeVida : MonoBehaviour
{
    [Header("Configurações das Barras")]
    [SerializeField] private Slider sliderPrincipal;
    [SerializeField] private Slider sliderFantasma;
    [SerializeField] private PlayerController player;

    [Header("Efeito Fantasma (Perda de Vida)")]
    [SerializeField] private float tempoEsperaAtraso = 0.4f;
    [SerializeField] private float velocidadeSuavizacaoDano = 5f;

    [Header("Efeito de Ganho de Vida")]
    [SerializeField] private float velocidadeGanhoVida = 5f;

    [Header("Efeito de Dano (Shake)")]
    [SerializeField] private RectTransform containerBarra;
    [SerializeField] private float forcaShake = 8f;
    [SerializeField] private float duracaoShake = 0.2f;

    private float dinheiroAnterior;
    private Coroutine coroutineAtrasoBarra;
    private Coroutine coroutineShake;
    private Vector2 posicaoOriginalBarra;

    void Awake()
    {
        if (containerBarra == null)
        {
            containerBarra = GetComponent<RectTransform>();
        }

        if (containerBarra != null)
        {
            posicaoOriginalBarra = containerBarra.anchoredPosition;
        }
    }

    void Start()
    {
        ConfigurarComponentes();
    }

    void Update()
    {
        if (player == null)
        {
            player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                SincronizarValoresIniciais();
            }
        }

        if (player == null || sliderPrincipal == null) return;

        sliderPrincipal.maxValue = player.maxDinheiro;
        if (sliderFantasma != null) sliderFantasma.maxValue = player.maxDinheiro;

        if (player.dinheiroAtual < dinheiroAnterior)
        {
            sliderPrincipal.value = player.dinheiroAtual;

            AtivarShake();

            IniciarAtrasoBarraFantasma();

            dinheiroAnterior = player.dinheiroAtual;
        }
        else if (player.dinheiroAtual > dinheiroAnterior)
        {
            if (coroutineAtrasoBarra != null) StopCoroutine(coroutineAtrasoBarra);

            dinheiroAnterior = player.dinheiroAtual;
        }

        if (sliderPrincipal.value < player.dinheiroAtual)
        {
            sliderPrincipal.value = Mathf.Lerp(sliderPrincipal.value, player.dinheiroAtual, UnityEngine.Time.deltaTime * velocidadeGanhoVida);

            if (Mathf.Abs(sliderPrincipal.value - player.dinheiroAtual) < 0.05f)
            {
                sliderPrincipal.value = player.dinheiroAtual;
            }

            if (sliderFantasma != null)
            {
                sliderFantasma.value = sliderPrincipal.value;
            }
        }
    }

    private void IniciarAtrasoBarraFantasma()
    {
        if (coroutineAtrasoBarra != null) StopCoroutine(coroutineAtrasoBarra);
        coroutineAtrasoBarra = StartCoroutine(RotinaAtrasoBarraFantasma());
    }

    private IEnumerator RotinaAtrasoBarraFantasma()
    {
        yield return new WaitForSeconds(tempoEsperaAtraso);

        if (sliderFantasma != null)
        {
            while (Mathf.Abs(sliderFantasma.value - sliderPrincipal.value) > 0.01f)
            {
                sliderFantasma.value = Mathf.Lerp(sliderFantasma.value, sliderPrincipal.value, UnityEngine.Time.deltaTime * velocidadeSuavizacaoDano);
                yield return null;
            }
            sliderFantasma.value = sliderPrincipal.value;
        }
    }

    public void AtivarShake()
    {
        if (coroutineShake != null) StopCoroutine(coroutineShake);
        coroutineShake = StartCoroutine(RotinaShake());
    }

    private IEnumerator RotinaShake()
    {
        if (containerBarra == null) yield break;

        float tempo = 0f;
        while (tempo < duracaoShake)
        {
            tempo += UnityEngine.Time.deltaTime;
            Vector2 deslocamento = Random.insideUnitCircle * forcaShake;
            containerBarra.anchoredPosition = posicaoOriginalBarra + deslocamento;
            yield return null;
        }

        containerBarra.anchoredPosition = posicaoOriginalBarra;
    }

    private void ConfigurarComponentes()
    {
        if (sliderPrincipal == null) sliderPrincipal = GetComponent<Slider>();
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();

        if (player != null)
        {
            SincronizarValoresIniciais();
        }
    }

    private void SincronizarValoresIniciais()
    {
        dinheiroAnterior = player.dinheiroAtual;

        if (sliderPrincipal != null)
        {
            sliderPrincipal.maxValue = player.maxDinheiro;
            sliderPrincipal.value = player.dinheiroAtual;
        }

        if (sliderFantasma != null)
        {
            sliderFantasma.maxValue = player.maxDinheiro;
            sliderFantasma.value = player.dinheiroAtual;
        }
    }
}