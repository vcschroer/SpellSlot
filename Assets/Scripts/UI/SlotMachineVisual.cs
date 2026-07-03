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
    [SerializeField] private SlotMachineLever scriptAlavanca;

    [Header("Sprites das Recompensas (Resultado Final)")]
    [SerializeField] private Sprite spriteDinheiro;
    [SerializeField] private Sprite spriteTamanhoEspada;
    [SerializeField] private Sprite spriteVelAtaqueEspada;   
    [SerializeField] private Sprite spriteVelAtaquePistola;  
    [SerializeField] private Sprite spriteVelocidadePlayer;
    [SerializeField] private Sprite spritePistolBalas;       
    [SerializeField] private Sprite spritePistolRicochete;   
    [SerializeField] private Sprite spriteVazio;

    [Header("Frames da Animacao de Giro")]
    [SerializeField] private List<Sprite> framesAnimacaoGiro = new List<Sprite>();

    [Header("Configuracoes do Tempo de Giro")]
    [SerializeField] private float tempoGiroBase = 1.0f;
    [SerializeField] private float intervaloEntreSlots = 0.5f;
    [SerializeField] private float tempoPorFrame = 0.05f;

    [Header("Efeito de Pulo (Juice/Feedback)")]
    [SerializeField] private float forcaDoPulo = 1.3f;
    [SerializeField] private float duracaoSubida = 0.08f;
    [SerializeField] private float duracaoDescida = 0.12f;

    [Header("Configuracoes do RGB (Jackpot)")]
    [SerializeField] private float velocidadeRGB = 2f;

    private PlayerController player;
    private Coroutine coroutineGiroAtual;
    private bool executandoRGB = false;

    private Color corOriginalSlot1 = Color.white;
    private Color corOriginalSlot2 = Color.white;
    private Color corOriginalSlot3 = Color.white;

    void Awake()
    {
        if (exibicaoSlot1 != null) corOriginalSlot1 = exibicaoSlot1.color;
        if (exibicaoSlot2 != null) corOriginalSlot2 = exibicaoSlot2.color;
        if (exibicaoSlot3 != null) corOriginalSlot3 = exibicaoSlot3.color;
    }

    void Start()
    {
        player = Object.FindAnyObjectByType<PlayerController>();
    }

    public void Update()
    {
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();

        if (player != null)
        {
            DefinirRGB(player.JackpotAtivo);
        }

        if (executandoRGB)
        {
            float h = (Time.time * velocidadeRGB) % 1f;
            Color corRainbow = Color.HSVToRGB(h, 0.75f, 1f);

            if (exibicaoSlot1 != null) exibicaoSlot1.color = corRainbow;
            if (exibicaoSlot2 != null) exibicaoSlot2.color = corRainbow;
            if (exibicaoSlot3 != null) exibicaoSlot3.color = corRainbow;
        }
    }

    private void DefinirRGB(bool ligado)
    {
        if (executandoRGB == ligado) return;
        executandoRGB = ligado;

        if (!ligado)
        {
            if (exibicaoSlot1 != null) exibicaoSlot1.color = corOriginalSlot1;
            if (exibicaoSlot2 != null) exibicaoSlot2.color = corOriginalSlot2;
            if (exibicaoSlot3 != null) exibicaoSlot3.color = corOriginalSlot3;
        }
    }

    public void UpdateOld() { }

    public void AtualizarVisualDosSlots(SlotMachine.TipoRecompensa s1, SlotMachine.TipoRecompensa s2, SlotMachine.TipoRecompensa s3)
    {
        if (scriptAlavanca != null) scriptAlavanca.PuxarAlavanca();

        if (coroutineGiroAtual != null)
        {
            StopAllCoroutines();
            ResetarEscalaDosSlots();
            if (scriptAlavanca != null) scriptAlavanca.PuxarAlavanca();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("moeda in slot");
            MusicManager.Instance.PlayLoopingSFX("slotmachinespin");
        }

        if (framesAnimacaoGiro == null || framesAnimacaoGiro.Count == 0)
        {
            ColocarSpritesFinaisDireto(s1, s2, s3);
            if (MusicManager.Instance != null) MusicManager.Instance.StopLoopingSFX();
            return;
        }

        coroutineGiroAtual = StartCoroutine(RotinaGirarComFrames(s1, s2, s3));
    }

    private IEnumerator RotinaGirarComFrames(SlotMachine.TipoRecompensa resultado1, SlotMachine.TipoRecompensa resultado2, SlotMachine.TipoRecompensa resultado3)
    {
        float tempoParadaSlot1 = Time.time + tempoGiroBase;
        float tempoParadaSlot2 = tempoParadaSlot1 + intervaloEntreSlots;
        float tempoParadaSlot3 = tempoParadaSlot2 + intervaloEntreSlots;

        bool slot1Girando = true; bool slot2Girando = true; bool slot3Girando = true;
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
                if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("stopslotmachine");

                if (exibicaoSlot1 != null)
                {
                    exibicaoSlot1.sprite = RetornarSpriteCorrespondente(resultado1);
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot1));
                }
            }
            if (slot2Girando && Time.time >= tempoParadaSlot2)
            {
                slot2Girando = false;
                if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("stopslotmachine");

                if (exibicaoSlot2 != null)
                {
                    exibicaoSlot2.sprite = RetornarSpriteCorrespondente(resultado2);
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot2));
                }
            }
            if (slot3Girando && Time.time >= tempoParadaSlot3)
            {
                slot3Girando = false;
                if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("stopslotmachine");

                if (exibicaoSlot3 != null)
                {
                    exibicaoSlot3.sprite = RetornarSpriteCorrespondente(resultado3);
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot3));
                }
            }
            yield return null;
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopLoopingSFX();
        }

        coroutineGiroAtual = null;
    }

    public float ObterTempoTotalGiro() => tempoGiroBase + (intervaloEntreSlots * 2f) + duracaoSubida + duracaoDescida;

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
            SlotMachine.TipoRecompensa.VelocidadeAtaqueEspada => spriteVelAtaqueEspada,    
            SlotMachine.TipoRecompensa.VelocidadeAtaquePistola => spriteVelAtaquePistola,   
            SlotMachine.TipoRecompensa.VelocidadePlayer => spriteVelocidadePlayer,
            SlotMachine.TipoRecompensa.PistolBalas => spritePistolBalas,                   
            SlotMachine.TipoRecompensa.PistolRicochete => spritePistolRicochete,
            SlotMachine.TipoRecompensa.Vazio => spriteVazio,
            _ => null
        };
    }
}