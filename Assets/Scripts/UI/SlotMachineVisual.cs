using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineVisual : MonoBehaviour
{
    [Header("Componentes de Imagem da UI")]
    [SerializeField] private Image exibicaoSlot1;
    [SerializeField] private Image exibicaoSlot2;
    [SerializeField] private Image exibicaoSlot3;

    [Header("Componente da Alavanca")]
    [Tooltip("Arraste aqui o objeto que possui o script SlotMachineLever")]
    [SerializeField] private SlotMachineLever scriptAlavanca;

    [Header("Sprites das Recompensas (Resultado Final)")]
    [SerializeField] private Sprite spriteDinheiro;
    [SerializeField] private Sprite spriteTamanhoEspada;
    [SerializeField] private Sprite spriteVelocidadeAtaque;
    [SerializeField] private Sprite spriteVelocidadePlayer;
    [SerializeField] private Sprite spriteVazio;

    [Header("Frames da Animação de Giro")]
    [Tooltip("Arraste aqui todos os frames da animação de roletagem/blur na ordem correta")]
    [SerializeField] private List<Sprite> framesAnimacaoGiro = new List<Sprite>();

    [Header("Configurações do Tempo de Giro")]
    [Tooltip("Tempo (em segundos) que o primeiro slot fica girando antes de parar")]
    [SerializeField] private float tempoGiroBase = 1.0f;

    [Tooltip("Intervalo de tempo entre a parada de um slot e o próximo")]
    [SerializeField] private float intervaloEntreSlots = 0.5f;

    [Tooltip("Velocidade da animação (tempo entre cada frame da roletagem)")]
    [SerializeField] private float tempoPorFrame = 0.05f;

    [Header("Efeito de Pulo (Juice/Feedback)")]
    [Tooltip("O multiplicador de tamanho no pico do pulo (ex: 1.3f significa que cresce 30%)")]
    [SerializeField] private float forcaDoPulo = 1.3f;

    [Tooltip("Tempo que o ícone leva para crescer")]
    [SerializeField] private float duracaoSubida = 0.08f;

    [Tooltip("Tempo que o ícone leva para voltar ao tamanho normal")]
    [SerializeField] private float duracaoDescida = 0.12f;

    private Coroutine coroutineGiroAtual;

    public void Update()
    {
    }

    public void AtualizarVisualDosSlots(SlotMachine.TipoRecompensa s1, SlotMachine.TipoRecompensa s2, SlotMachine.TipoRecompensa s3)
    {
        if (scriptAlavanca != null)
        {
            scriptAlavanca.PuxarAlavanca();
        }

        if (coroutineGiroAtual != null)
        {
            StopAllCoroutines(); 
            ResetarEscalaDosSlots(); 

            if (scriptAlavanca != null)
            {
                scriptAlavanca.PuxarAlavanca();
            }
        }

        if (framesAnimacaoGiro == null || framesAnimacaoGiro.Count == 0)
        {
            Debug.LogWarning("Nenhum frame de animação de giro foi colocado no SlotMachineVisual!");
            ColocarSpritesFinaisDireto(s1, s2, s3);
            return;
        }

        coroutineGiroAtual = StartCoroutine(RotinaGirarComFrames(s1, s2, s3));
    }

    private IEnumerator RotinaGirarComFrames(SlotMachine.TipoRecompensa resultado1, SlotMachine.TipoRecompensa resultado2, SlotMachine.TipoRecompensa resultado3)
    {
        float tempoParadaSlot1 = Time.time + tempoGiroBase;
        float tempoParadaSlot2 = tempoParadaSlot1 + intervaloEntreSlots;
        float tempoParadaSlot3 = tempoParadaSlot2 + intervaloEntreSlots;

        bool slot1Girando = true;
        bool slot2Girando = true;
        bool slot3Girando = true;

        float cronometroFrame = 0f;

        int indexFrame1 = 0;
        int indexFrame2 = Random.Range(0, framesAnimacaoGiro.Count);
        int indexFrame3 = Random.Range(0, framesAnimacaoGiro.Count);

        while (slot1Girando || slot2Girando || slot3Girando)
        {
            if (cronometroFrame <= 0f)
            {
                if (slot1Girando && exibicaoSlot1 != null)
                {
                    exibicaoSlot1.sprite = framesAnimacaoGiro[indexFrame1];
                    indexFrame1 = (indexFrame1 + 1) % framesAnimacaoGiro.Count;
                }

                if (slot2Girando && exibicaoSlot2 != null)
                {
                    exibicaoSlot2.sprite = framesAnimacaoGiro[indexFrame2];
                    indexFrame2 = (indexFrame2 + 1) % framesAnimacaoGiro.Count;
                }

                if (slot3Girando && exibicaoSlot3 != null)
                {
                    exibicaoSlot3.sprite = framesAnimacaoGiro[indexFrame3];
                    indexFrame3 = (indexFrame3 + 1) % framesAnimacaoGiro.Count;
                }

                cronometroFrame = tempoPorFrame;
            }

            cronometroFrame -= Time.deltaTime;

            if (slot1Girando && Time.time >= tempoParadaSlot1)
            {
                slot1Girando = false;
                if (exibicaoSlot1 != null)
                {
                    exibicaoSlot1.sprite = RetornarSpriteCorrespondente(resultado1);
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot1));
                }
            }

            if (slot2Girando && Time.time >= tempoParadaSlot2)
            {
                slot2Girando = false;
                if (exibicaoSlot2 != null)
                {
                    exibicaoSlot2.sprite = RetornarSpriteCorrespondente(resultado2);
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot2));
                }
            }

            if (slot3Girando && Time.time >= tempoParadaSlot3)
            {
                slot3Girando = false;
                if (exibicaoSlot3 != null)
                {
                    exibicaoSlot3.sprite = RetornarSpriteCorrespondente(resultado3);
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot3));
                }
            }

            yield return null;
        }

        coroutineGiroAtual = null;
    }

    public float ObterTempoTotalGiro()
    {
        return tempoGiroBase + (intervaloEntreSlots * 2f) + duracaoSubida + duracaoDescida;
    }

    private IEnumerator RotinaPulandoIcone(Image imagem)
    {
        if (imagem == null) yield break;

        Transform transformIcone = imagem.transform;
        Vector3 escalaOriginal = Vector3.one;
        Vector3 escalaMaxima = escalaOriginal * forcaDoPulo;

        float tempoMudar = 0f;
        while (tempoMudar < duracaoSubida)
        {
            tempoMudar += Time.deltaTime;
            transformIcone.localScale = Vector3.Lerp(escalaOriginal, escalaMaxima, tempoMudar / duracaoSubida);
            yield return null;
        }

        tempoMudar = 0f;
        while (tempoMudar < duracaoDescida)
        {
            tempoMudar += Time.deltaTime;
            transformIcone.localScale = Vector3.Lerp(escalaMaxima, escalaOriginal, tempoMudar / duracaoDescida);
            yield return null;
        }

        transformIcone.localScale = escalaOriginal;
    }

    private void ResetarEscalaDosSlots()
    {
        if (exibicaoSlot1 != null) exibicaoSlot1.transform.localScale = Vector3.one;
        if (exibicaoSlot2 != null) exibicaoSlot2.transform.localScale = Vector3.one;
        if (exibicaoSlot3 != null) exibicaoSlot3.transform.localScale = Vector3.one;
    }

    private void ColocarSpritesFinaisDireto(SlotMachine.TipoRecompensa s1, SlotMachine.TipoRecompensa s2, SlotMachine.TipoRecompensa s3)
    {
        if (exibicaoSlot1 != null) exibicaoSlot1.sprite = RetornarSpriteCorrespondente(s1);
        if (exibicaoSlot2 != null) exibicaoSlot2.sprite = RetornarSpriteCorrespondente(s2);
        if (exibicaoSlot3 != null) exibicaoSlot3.sprite = RetornarSpriteCorrespondente(s3);
    }

    private Sprite RetornarSpriteCorrespondente(SlotMachine.TipoRecompensa tipo)
    {
        return tipo switch
        {
            SlotMachine.TipoRecompensa.Dinheiro => spriteDinheiro,
            SlotMachine.TipoRecompensa.TamanhoEspada => spriteTamanhoEspada,
            SlotMachine.TipoRecompensa.VelocidadeAtaque => spriteVelocidadeAtaque,
            SlotMachine.TipoRecompensa.VelocidadePlayer => spriteVelocidadePlayer,
            SlotMachine.TipoRecompensa.Vazio => spriteVazio,
            _ => null
        };
    }
}