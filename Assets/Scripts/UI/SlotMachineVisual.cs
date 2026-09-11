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

    [Header("Sprites das Recompensas (Armas Padrão e Player)")]
    [SerializeField] private Sprite spriteDinheiro;
    [SerializeField] private Sprite spriteTamanhoEspada;
    [SerializeField] private Sprite spriteVelAtaqueEspada;
    [SerializeField] private Sprite spriteAnguloEspada;
    [SerializeField] private Sprite spriteVelAtaquePistola;
    [SerializeField] private Sprite spriteVelocidadePlayer;
    [SerializeField] private Sprite spritePistolBalas;
    [SerializeField] private Sprite spritePistolRicochete;
    [SerializeField] private Sprite spriteVazio;

    [Header("Sprites das Recompensas (Órbita)")]
    [SerializeField] private Sprite spriteOrbitaProjeteis;
    [SerializeField] private Sprite spriteOrbitaVelocidade;
    [SerializeField] private Sprite spriteOrbitaRaio;

    [Header("Frames da Animacao de Giro")]
    [SerializeField] private List<Sprite> framesAnimacaoGiro = new List<Sprite>();

    [Header("Configuracoes do Tempo de Giro")]
    [SerializeField] private float tempoGiroBase = 1.0f;
    [SerializeField] private float intervaloEntreSlots = 0.5f;
    [SerializeField] private float tempoPorFrame = 0.05f;

    [Header("Efeito de Pulo do Icone (Aparicao da Recompensa)")]
    [SerializeField] private float forcaDoPuloIcone = 1.3f;
    [SerializeField] private float duracaoSubidaIcone = 0.08f;
    [SerializeField] private float duracaoDescidaIcone = 0.12f;

    [Header("Efeito de Pulo Cartoon da Maquina (Juice)")]
    [SerializeField] private RectTransform containerMaquina;
    [SerializeField] private float alturaPuloMaquina = 40f;
    [SerializeField] private float duracaoSubidaMaquina = 0.1f;
    [SerializeField] private float duracaoDescidaMaquina = 0.15f;
    [SerializeField] private Vector3 esticamentoPulo = new Vector3(0.85f, 1.2f, 1f);
    [SerializeField] private Vector3 achatamentoImpacto = new Vector3(1.15f, 0.85f, 1f);

    [Header("Efeito de Dano (Shake)")]
    [SerializeField] private float forcaShake = 15f;
    [SerializeField] private float duracaoShake = 0.2f;

    [Header("Efeito Ondular (Wave / Respiro)")]
    [SerializeField] private bool ativarEfeitoOndular = true;
    [SerializeField] private float forcaoOnda = 3f;
    [SerializeField] private float velocidadeOnda = 1.5f;

    private Coroutine coroutineGiroAtual;
    private Vector2 posicaoOriginalMaquina;
    private Vector3 escalaOriginalMaquina = Vector3.one;

    private bool estaAnimandoEspecial = false;

    void Awake()
    {
        if (containerMaquina == null)
        {
            containerMaquina = GetComponent<RectTransform>();
        }

        if (containerMaquina != null)
        {
            posicaoOriginalMaquina = containerMaquina.anchoredPosition;
            escalaOriginalMaquina = containerMaquina.localScale;
        }
    }

    void Update()
    {
        AplicarEfeitoOndular();
    }

    private void AplicarEfeitoOndular()
    {
        if (!ativarEfeitoOndular || containerMaquina == null || estaAnimandoEspecial) return;

        float deslocamentoY = Mathf.Sin(UnityEngine.Time.time * velocidadeOnda) * forcaoOnda;

        containerMaquina.anchoredPosition = new Vector2(
            posicaoOriginalMaquina.x,
            posicaoOriginalMaquina.y + deslocamentoY
        );
    }

    public void AtualizarVisualDosSlots(SlotMachine.TipoRecompensa s1, SlotMachine.TipoRecompensa s2, SlotMachine.TipoRecompensa s3)
    {
        Debug.Log($"<color=yellow>[SLOT VISUAL] Inicio do Giro Sorteado -> Slot1: {s1} | Slot2: {s2} | Slot3: {s3}</color>");

        if (scriptAlavanca != null) scriptAlavanca.PuxarAlavanca();

        if (coroutineGiroAtual != null)
        {
            StopAllCoroutines();
            ResetarEstadoMaquina();
            ResetarEscalaDosSlots();
            if (scriptAlavanca != null) scriptAlavanca.PuxarAlavanca();
        }

        if (framesAnimacaoGiro == null || framesAnimacaoGiro.Count == 0)
        {
            Debug.LogWarning("[SLOT VISUAL] Nenhum frame de animacao atribuido. Colocando sprites finais diretamente.");
            ColocarSpritesFinaisDireto(s1, s2, s3);
            return;
        }

        coroutineGiroAtual = StartCoroutine(RotinaSequenciaGiro(s1, s2, s3));
    }

    private IEnumerator RotinaSequenciaGiro(SlotMachine.TipoRecompensa s1, SlotMachine.TipoRecompensa s2, SlotMachine.TipoRecompensa s3)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("moeda in slot");
            MusicManager.Instance.PlayLoopingSFX("slotmachinespin");
        }

        StartCoroutine(RotinaPuloMaquinaCartoon());

        yield return StartCoroutine(RotinaGirarComFrames(s1, s2, s3));
    }

    private IEnumerator RotinaPuloMaquinaCartoon()
    {
        if (containerMaquina == null) yield break;

        estaAnimandoEspecial = true;

        float tempo = 0f;
        Vector2 posAlvoPulo = posicaoOriginalMaquina + new Vector2(0f, alturaPuloMaquina);

        while (tempo < duracaoSubidaMaquina)
        {
            tempo += UnityEngine.Time.deltaTime;
            float t = tempo / duracaoSubidaMaquina;

            containerMaquina.anchoredPosition = Vector2.Lerp(posicaoOriginalMaquina, posAlvoPulo, t);
            containerMaquina.localScale = Vector3.Lerp(escalaOriginalMaquina, esticamentoPulo, t);
            yield return null;
        }

        tempo = 0f;
        while (tempo < duracaoDescidaMaquina)
        {
            tempo += UnityEngine.Time.deltaTime;
            float t = tempo / duracaoDescidaMaquina;

            containerMaquina.anchoredPosition = Vector2.Lerp(posAlvoPulo, posicaoOriginalMaquina, t);
            containerMaquina.localScale = Vector3.Lerp(esticamentoPulo, achatamentoImpacto, t);
            yield return null;
        }

        tempo = 0f;
        float duracaoRetorno = 0.08f;
        while (tempo < duracaoRetorno)
        {
            tempo += UnityEngine.Time.deltaTime;
            float t = tempo / duracaoRetorno;

            containerMaquina.localScale = Vector3.Lerp(achatamentoImpacto, escalaOriginalMaquina, t);
            yield return null;
        }

        ResetarEstadoMaquina();
    }

    private IEnumerator RotinaGirarComFrames(SlotMachine.TipoRecompensa resultado1, SlotMachine.TipoRecompensa resultado2, SlotMachine.TipoRecompensa resultado3)
    {
        float tempoParadaSlot1 = UnityEngine.Time.time + tempoGiroBase;
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

            cronometroFrame -= UnityEngine.Time.deltaTime;

            if (slot1Girando && UnityEngine.Time.time >= tempoParadaSlot1)
            {
                slot1Girando = false;
                if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("stopslotmachine");

                if (exibicaoSlot1 != null)
                {
                    Sprite spriteFinal1 = RetornarSpriteCorrespondente(resultado1);
                    exibicaoSlot1.sprite = spriteFinal1;
                    Debug.Log($"[SLOT VISUAL] Slot 1 Parou -> Recompensa: <b>{resultado1}</b> | Sprite Exibido: <b>{(spriteFinal1 != null ? spriteFinal1.name : "NULO (FALTANDO SPRITE!)")}</b>");
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot1));
                }
            }
            if (slot2Girando && UnityEngine.Time.time >= tempoParadaSlot2)
            {
                slot2Girando = false;
                if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("stopslotmachine");

                if (exibicaoSlot2 != null)
                {
                    Sprite spriteFinal2 = RetornarSpriteCorrespondente(resultado2);
                    exibicaoSlot2.sprite = spriteFinal2;
                    Debug.Log($"[SLOT VISUAL] Slot 2 Parou -> Recompensa: <b>{resultado2}</b> | Sprite Exibido: <b>{(spriteFinal2 != null ? spriteFinal2.name : "NULO (FALTANDO SPRITE!)")}</b>");
                    StartCoroutine(RotinaPulandoIcone(exibicaoSlot2));
                }
            }
            if (slot3Girando && UnityEngine.Time.time >= tempoParadaSlot3)
            {
                slot3Girando = false;
                if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("stopslotmachine");

                if (exibicaoSlot3 != null)
                {
                    Sprite spriteFinal3 = RetornarSpriteCorrespondente(resultado3);
                    exibicaoSlot3.sprite = spriteFinal3;
                    Debug.Log($"[SLOT VISUAL] Slot 3 Parou -> Recompensa: <b>{resultado3}</b> | Sprite Exibido: <b>{(spriteFinal3 != null ? spriteFinal3.name : "NULO (FALTANDO SPRITE!)")}</b>");
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

    public float ObterTempoTotalGiro() => tempoGiroBase + (intervaloEntreSlots * 2f) + duracaoSubidaIcone + duracaoDescidaIcone;

    private IEnumerator RotinaPulandoIcone(Image imagem)
    {
        if (imagem == null) yield break;
        Transform transformIcone = imagem.transform;
        Vector3 escalaOriginal = Vector3.one;
        Vector3 escalaMaxima = escalaOriginal * forcaDoPuloIcone;

        float tempoMudar = 0f;
        while (tempoMudar < duracaoSubidaIcone)
        {
            tempoMudar += UnityEngine.Time.deltaTime;
            transformIcone.localScale = Vector3.Lerp(escalaOriginal, escalaMaxima, tempoMudar / duracaoSubidaIcone);
            yield return null;
        }
        tempoMudar = 0f;
        while (tempoMudar < duracaoDescidaIcone)
        {
            tempoMudar += UnityEngine.Time.deltaTime;
            transformIcone.localScale = Vector3.Lerp(escalaMaxima, escalaOriginal, tempoMudar / duracaoDescidaIcone);
            yield return null;
        }
        transformIcone.localScale = escalaOriginal;
    }

    private void ResetarEstadoMaquina()
    {
        estaAnimandoEspecial = false;
        if (containerMaquina != null)
        {
            containerMaquina.anchoredPosition = posicaoOriginalMaquina;
            containerMaquina.localScale = escalaOriginalMaquina;
        }
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
        Sprite spriteCorrespondente = tipo switch
        {
            SlotMachine.TipoRecompensa.Dinheiro => spriteDinheiro,
            SlotMachine.TipoRecompensa.TamanhoEspada => spriteTamanhoEspada,
            SlotMachine.TipoRecompensa.VelocidadeAtaqueEspada => spriteVelAtaqueEspada,
            SlotMachine.TipoRecompensa.AnguloEspada => spriteAnguloEspada,
            SlotMachine.TipoRecompensa.VelocidadeAtaquePistola => spriteVelAtaquePistola,
            SlotMachine.TipoRecompensa.VelocidadePlayer => spriteVelocidadePlayer,
            SlotMachine.TipoRecompensa.PistolBalas => spritePistolBalas,
            SlotMachine.TipoRecompensa.PistolRicochete => spritePistolRicochete,
            SlotMachine.TipoRecompensa.Vazio => spriteVazio,
            SlotMachine.TipoRecompensa.OrbitaProjeteis => spriteOrbitaProjeteis,
            SlotMachine.TipoRecompensa.OrbitaVelocidade => spriteOrbitaVelocidade,
            SlotMachine.TipoRecompensa.OrbitaRaio => spriteOrbitaRaio,
            _ => null
        };

        if (spriteCorrespondente == null)
        {
            Debug.LogError($"<color=red>[SLOT VISUAL] ERRO DE SPRITE: O tipo de recompensa '{tipo}' nao possui um Sprite atribuido no Inspector!</color>");
        }

        return spriteCorrespondente;
    }

    public void AtivarShakeDano()
    {
        StartCoroutine(RotinaShakeDano());
    }

    private IEnumerator RotinaShakeDano()
    {
        if (containerMaquina == null) yield break;

        estaAnimandoEspecial = true;

        float tempo = 0f;
        while (tempo < duracaoShake)
        {
            tempo += UnityEngine.Time.deltaTime;
            Vector2 deslocamento = Random.insideUnitCircle * forcaShake;
            containerMaquina.anchoredPosition = posicaoOriginalMaquina + deslocamento;
            yield return null;
        }

        ResetarEstadoMaquina();
    }
}