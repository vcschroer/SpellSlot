using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configurações de Vida")]
    [SerializeField] private int vidaAtual = 10;
    [SerializeField] private int danoQueRecebeDaEspada = 10;

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaMinima = 0.5f;

    [Header("Configurações de Ataque")]
    [SerializeField] private int danoNoPlayer = 20;
    [SerializeField] private float tempoAnimacaoMorte = 0.5f;

    [Header("Componentes Visuais")]
    [SerializeField] private AnimEnemy scriptAnimacao;
    [SerializeField] private SpriteEffects scriptEfeitos;

    [Header("Configurações de Efeitos e Partículas")]
    [Tooltip("Arraste aqui o Prefab da partícula de explosão")]
    [SerializeField] private GameObject prefabExplosao;

    [Header("Configurações de Drops")]
    [SerializeField] private GameObject prefabMoeda;

    private Transform alvoPlayer;
    private Rigidbody2D rb;
    private Vector2 direcao;
    private bool olhandoParaDireita = false;
    private bool estaMorto = false;

    private float tempoProximoDano = 0f;
    private float intervaloInvenclibilidade = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) alvoPlayer = playerObj.transform;

        if (scriptAnimacao == null) scriptAnimacao = GetComponent<AnimEnemy>();

        scriptEfeitos = GetComponent<SpriteEffects>();
        if (scriptEfeitos == null) scriptEfeitos = GetComponentInChildren<SpriteEffects>();
    }

    void Update()
    {
        if (estaMorto || alvoPlayer == null) return;

        float distanciaAtual = Vector2.Distance(transform.position, alvoPlayer.position);

        if (distanciaAtual > distanciaMinima)
        {
            Vector2 direcaoParaOPlayer = alvoPlayer.position - transform.position;
            direcao = direcaoParaOPlayer.normalized;
        }
        else
        {
            direcao = Vector2.zero;
        }

        VerificarFlip();
    }

    void FixedUpdate()
    {
        if (estaMorto)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + direcao * velocidade * Time.fixedDeltaTime);

        if (scriptAnimacao != null)
        {
            scriptAnimacao.AtualizarMovimento(direcao.magnitude);
        }
    }

    public void TomarDano()
    {
        if (estaMorto || Time.time < tempoProximoDano) return;

        vidaAtual -= danoQueRecebeDaEspada;
        tempoProximoDano = Time.time + intervaloInvenclibilidade;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("hitenemy");
        }

        if (scriptEfeitos != null)
        {
            scriptEfeitos.PlayFlash(0.12f);
            scriptEfeitos.PlaySquashAndStretch(1.35f, 0.65f, 0.12f);
        }

        if (vidaAtual <= 0)
        {
            IniciarProcessoMorte(true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMorto) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            if (CameraShake.Instancia != null)
            {
                CameraShake.Instancia.Tremer(0.2f, 0.4f);
            }

            player.TomarDano(danoNoPlayer);

            IniciarProcessoMorte(false);
        }
    }

    private void IniciarProcessoMorte(bool deveDroparMoeda)
    {
        estaMorto = true;
        direcao = Vector2.zero;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("enemyexplosion");
        }

        Collider2D colisor = GetComponent<Collider2D>();
        if (colisor != null) colisor.enabled = false;

        if (scriptAnimacao != null)
        {
            scriptAnimacao.DispararMorte();
        }

        if (prefabExplosao != null)
        {
            Instantiate(prefabExplosao, transform.position, Quaternion.identity);
        }

        if (deveDroparMoeda && prefabMoeda != null)
        {
            Instantiate(prefabMoeda, transform.position, Quaternion.identity);
        }

        StartCoroutine(RotinaDestruição());
    }

    private IEnumerator RotinaDestruição()
    {
        yield return new WaitForSeconds(tempoAnimacaoMorte);
        Destroy(gameObject);
    }

    private void VerificarFlip()
    {
        if (direcao.x < 0 && olhandoParaDireita) Flipar();
        else if (direcao.x > 0 && !olhandoParaDireita) Flipar();
    }

    private void Flipar()
    {
        olhandoParaDireita = !olhandoParaDireita;
        Vector3 rotator = transform.eulerAngles;
        rotator.y = olhandoParaDireita ? 0f : 180f;
        transform.eulerAngles = rotator;
    }
}