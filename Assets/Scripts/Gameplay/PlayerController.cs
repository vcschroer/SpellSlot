using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 5f;

    [Header("Configurações de Knockback")]
    [SerializeField] private float duracaoKnockback = 0.2f;
    private bool estaEmKnockback = false;

    [Header("Scripts Auxiliares")]
    [SerializeField] private AnimPlayer scriptAnimacao;
    [SerializeField] private SpriteEffects scriptEfeitos;

    private SpriteRenderer spritePlayer;
    private Rigidbody2D rb;
    private Vector2 inputsMovimento;
    private bool olhandoParaDireita = true;

    [Header("Armas Equipadas")]
    public List<BaseWeapon> armasEquipadas = new List<BaseWeapon>();

    [Header("Sistema de Economia / Vida")]
    [SerializeField] public int maxDinheiro = 100;
    [SerializeField] public int dinheiroAtual = 100;
    public int vidaAtual => dinheiroAtual;

    [Header("Invencibilidade / Cooldown de Dano")]
    [SerializeField] private float cooldownDano = 1.5f;
    private float tempoProximoDano = 0f;

    [Header("Efeitos Jackpot (RGB e Rastro)")]
    [SerializeField] private TrailRenderer rastroRGB;
    [SerializeField] private float velocidadeTrocaCorRGB = 3f;
    [Range(0f, 1f)]
    [Tooltip("Controla o tom pastel. 1 = cor forte, 0 = branco. 0.55 é um bom tom pastel.")]
    [SerializeField] private float saturacaoRGB = 0.55f;
    [Tooltip("Multiplica a cor para ativar o Bloom (Glow). Teste valores entre 2 e 5.")]
    [SerializeField] private float intensidadeBrilho = 3f;

    public bool JackpotAtivo
    {
        get
        {
            foreach (BaseWeapon arma in armasEquipadas)
            {
                if (arma != null && arma.EstaEmModoJackpot) return true;
            }
            return false;
        }
    }

    [Header("Taxa de Sobrevivência")]
    [SerializeField] private int custoPorTempo = 5;
    [SerializeField] private float intervaloTempo = 5f;

    private bool derrotaDisparada = false;
    private bool estavaEmJackpot = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spritePlayer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        scriptAnimacao = scriptAnimacao ?? GetComponent<AnimPlayer>();
        scriptEfeitos = scriptEfeitos ?? GetComponent<SpriteEffects>() ?? GetComponentInChildren<SpriteEffects>();

        if (rastroRGB == null) rastroRGB = GetComponent<TrailRenderer>();
        if (rastroRGB != null) rastroRGB.emitting = false;

        SpawnarArmaInicial();

        dinheiroAtual = Mathf.Clamp(dinheiroAtual, 0, maxDinheiro);
        StartCoroutine(RotinaPerdaDeDinheiro());
    }

    void Update()
    {
        if (derrotaDisparada) return;

        bool jackpotAtual = JackpotAtivo;

        if (jackpotAtual != estavaEmJackpot)
        {
            estavaEmJackpot = jackpotAtual;

            if (scriptAnimacao != null)
            {
                scriptAnimacao.SetarModoJackpot(jackpotAtual);
                if (!jackpotAtual) scriptAnimacao.AtualizarMovimento(inputsMovimento.magnitude);
            }

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayJackpotSound(jackpotAtual);
            }

            if (rastroRGB != null)
            {
                rastroRGB.emitting = jackpotAtual;
                if (!jackpotAtual) rastroRGB.Clear();
            }
        }

        // --- SISTEMA DE COR PASTEL E BRILHO (HDR) ---
        if (estavaEmJackpot && rastroRGB != null)
        {
            // O segundo parâmetro (saturacaoRGB) é o que deixa a cor pastel
            Color corBase = Color.HSVToRGB(Mathf.Repeat(UnityEngine.Time.time * velocidadeTrocaCorRGB, 1f), saturacaoRGB, 1f);

            // Multiplicamos a cor pela intensidade para gerar o efeito de HDR/Emissão
            Color corHDR = corBase * intensidadeBrilho;

            rastroRGB.startColor = corHDR;
            // No final do trail, a cor perde o alpha (fica transparente)
            rastroRGB.endColor = new Color(corHDR.r, corHDR.g, corHDR.b, 0f);
        }
    }

    void FixedUpdate()
    {
        if (derrotaDisparada || estaEmKnockback) return;

        float velocidadeAtual = estavaEmJackpot ? (velocidade * 2f) : velocidade;

        rb.MovePosition(rb.position + inputsMovimento * velocidadeAtual * UnityEngine.Time.fixedDeltaTime);
        VerificarFlip();

        if (scriptAnimacao != null)
        {
            float velocidadFisica = inputsMovimento.magnitude;
            scriptAnimacao.AtualizarMovimento(velocidadFisica);
        }
    }

    private void SpawnarArmaInicial()
    {
        if (GameManager.Instance != null && GameManager.Instance.personagemEscolhido != null)
        {
            GameObject prefabArma = GameManager.Instance.personagemEscolhido.prefabDaArmaInicial;
            if (prefabArma != null)
            {
                GameObject armaInstanciada = Instantiate(prefabArma, transform);
                BaseWeapon novaArma = armaInstanciada.GetComponent<BaseWeapon>();
                if (novaArma != null) armasEquipadas.Add(novaArma);
            }
        }

        BaseWeapon[] armasFilhas = GetComponentsInChildren<BaseWeapon>();
        foreach (BaseWeapon arma in armasFilhas)
        {
            if (!armasEquipadas.Contains(arma)) armasEquipadas.Add(arma);
        }
    }

    public BaseWeapon ObterArmaPorTipo(TipoArma tipo)
    {
        foreach (BaseWeapon arma in armasEquipadas)
        {
            if (arma != null && arma.tipoArma == tipo) return arma;
        }
        return null;
    }

    public void AdicionarNovaArma(GameObject prefabDaNovaArma)
    {
        if (prefabDaNovaArma == null) return;
        GameObject armaInstanciada = Instantiate(prefabDaNovaArma, transform);
        BaseWeapon novaArma = armaInstanciada.GetComponent<BaseWeapon>();
        if (novaArma != null) armasEquipadas.Add(novaArma);
    }

    public void OnMove(InputValue value)
    {
        if (derrotaDisparada) return;
        inputsMovimento = value.Get<Vector2>();
    }

    private IEnumerator RotinaPerdaDeDinheiro()
    {
        while (dinheiroAtual > 0 && !derrotaDisparada)
        {
            yield return new WaitForSeconds(intervaloTempo);
            PerderDinheiro(custoPorTempo);
        }
    }

    public void PerderDinheiro(int quantity)
    {
        if (derrotaDisparada) return;
        dinheiroAtual -= quantity;
        if (dinheiroAtual < 0) dinheiroAtual = 0;
        if (dinheiroAtual <= 0) Gamover();
    }

    public void TomarDano(int dano)
    {
        TomarDano(dano, Vector2.zero, 0f);
    }

    public void TomarDano(int dano, Vector2 direcaoKnockback, float forcaKnockback)
    {
        if (JackpotAtivo) return;
        if (UnityEngine.Time.time < tempoProximoDano) return;

        tempoProximoDano = UnityEngine.Time.time + cooldownDano;

        if (scriptEfeitos != null && dinheiroAtual > 0)
        {
            scriptEfeitos.PlayFlash(0.15f);
            scriptEfeitos.PlaySquashAndStretch(1.3f, 0.7f, 0.15f);
        }

        SlotMachineVisual slotVisual = Object.FindAnyObjectByType<SlotMachineVisual>();
        if (slotVisual != null)
        {
            slotVisual.AtivarShakeDano();
        }

        if (forcaKnockback > 0f && rb != null)
        {
            StartCoroutine(RotinaKnockback(direcaoKnockback, forcaKnockback));
        }

        PerderDinheiro(dano);
    }

    private IEnumerator RotinaKnockback(Vector2 direcao, float forca)
    {
        estaEmKnockback = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direcao * forca, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duracaoKnockback);

        rb.linearVelocity = Vector2.zero;
        estaEmKnockback = false;
    }

    public void GanharDinheiro(int quantity)
    {
        if (derrotaDisparada) return;
        dinheiroAtual = Mathf.Clamp(dinheiroAtual + quantity, 0, maxDinheiro);
    }

    private void Gamover()
    {
        if (derrotaDisparada) return;
        derrotaDisparada = true;
        if (scriptAnimacao != null) scriptAnimacao.AtualizarMovimento(0f);
        if (TransitionManager.Instance != null) TransitionManager.Instance.CarregarCena("Defeat");
        else SceneManager.LoadScene("Defeat");
    }

    private void VerificarFlip()
    {
        if (inputsMovimento.x < 0 && olhandoParaDireita) Flipar();
        else if (inputsMovimento.x > 0 && !olhandoParaDireita) Flipar();
    }

    private void Flipar()
    {
        olhandoParaDireita = !olhandoParaDireita;
        if (spritePlayer != null) spritePlayer.flipX = !olhandoParaDireita;
    }

    public void AumentarVelocidade(float quantidade) => velocidade += quantidade;
}