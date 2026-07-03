using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 5f;

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

        if (scriptEfeitos == null)
        {
            Debug.LogWarning("[AVISO]: O script SpriteEffects nao foi encontrado no Player! Verifique o Inspector.");
        }

        SpawnarArmaInicial();

        dinheiroAtual = Mathf.Clamp(dinheiroAtual, 0, maxDinheiro);
        StartCoroutine(RotinaPerdaDeDinheiro());
    }

    void Update()
    {
        if (derrotaDisparada) return;

        bool jackpotAtual = JackpotAtivo;

        if (scriptAnimacao != null)
        {
            scriptAnimacao.SetarModoJackpot(jackpotAtual);
        }

        if (jackpotAtual != estavaEmJackpot)
        {
            estavaEmJackpot = jackpotAtual;

            Debug.Log($"[JACKPOT]: Mudou de estado! Ativo: {jackpotAtual}");

            if (MusicManager.Instance != null) MusicManager.Instance.PlayJackpotSound(jackpotAtual);

            if (scriptEfeitos != null)
            {
                scriptEfeitos.DefinirRGB(jackpotAtual);
            }
        }
    }

    void FixedUpdate()
    {
        if (derrotaDisparada)
        {
            inputsMovimento = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + inputsMovimento * velocidade * Time.fixedDeltaTime);
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

    public void DispararAnimacaoAtaqueExterno()
    {
        if (scriptAnimacao != null && !derrotaDisparada) scriptAnimacao.DispararAnimacaoAtaque();
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
        if (JackpotAtivo) return;
        if (scriptEfeitos != null && dinheiroAtual > 0)
        {
            scriptEfeitos.PlayFlash(0.15f);
            scriptEfeitos.PlaySquashAndStretch(1.3f, 0.7f, 0.15f);
        }
        PerderDinheiro(dano);
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
        if (scriptEfeitos != null) scriptEfeitos.DefinirRGB(false);
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