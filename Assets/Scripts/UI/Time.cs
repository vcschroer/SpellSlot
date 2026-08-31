using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Time : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private TextMeshProUGUI textoRelogio;
    [SerializeField] private RectTransform containerTempo;

    [Header("Configurações do Timer")]
    [SerializeField] private bool comecarAoIniciar = true;

    [Header("Efeito Ondular (Wave / Respiro)")]
    [SerializeField] private bool ativarEfeitoOndular = true;
    [SerializeField] private float forcaoOnda = 3f;
    [SerializeField] private float velocidadeOnda = 1.5f;

    private float tempoDecorrido = 0f;
    private bool estaRodando = false;
    private Vector2 posicaoOriginalTempo;

    private void Awake()
    {
        if (containerTempo == null)
        {
            containerTempo = GetComponent<RectTransform>();
        }

        if (containerTempo != null)
        {
            posicaoOriginalTempo = containerTempo.anchoredPosition;
        }
    }

    private void Start()
    {
        if (comecarAoIniciar)
        {
            IniciarRelogio();
        }
    }

    private void Update()
    {
        if (estaRodando)
        {
            tempoDecorrido += UnityEngine.Time.deltaTime;
            AtualizarTextoUI();
        }

        AplicarEfeitoOndular();
    }

    private void AplicarEfeitoOndular()
    {
        if (!ativarEfeitoOndular || containerTempo == null) return;

        float deslocamentoY = Mathf.Sin(UnityEngine.Time.time * velocidadeOnda) * forcaoOnda;

        containerTempo.anchoredPosition = new Vector2(
            posicaoOriginalTempo.x,
            posicaoOriginalTempo.y + deslocamentoY
        );
    }

    private void AtualizarTextoUI()
    {
        if (textoRelogio == null) return;

        int minutos = Mathf.FloorToInt(tempoDecorrido / 60f);
        int segundos = Mathf.FloorToInt(tempoDecorrido % 60f);

        textoRelogio.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    public void IniciarRelogio()
    {
        estaRodando = true;
    }

    public void PausarRelogio()
    {
        estaRodando = false;
    }

    public void ResetarRelogio()
    {
        tempoDecorrido = 0f;
        AtualizarTextoUI();
    }

    public float ObterTempoEmSegundos()
    {
        return tempoDecorrido;
    }
}