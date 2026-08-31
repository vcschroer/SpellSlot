using System.Collections;
using UnityEngine;
using static EnemySpawn;

public class Enemy : MonoBehaviour
{
    [Header("Configurações de Vida")]
    [SerializeField] protected int vidaAtual = 10;

    [Header("Configurações de Feedback de Dano")]
    [SerializeField] private GameObject prefabDamagePopup;
    [SerializeField] private float duracaoShakeDano = 0.15f;
    [SerializeField] private float intensidadeShakeDano = 0.1f;

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaMinima = 0.5f;

    [Header("Configurações de Ataque")]
    [SerializeField] public int danoNoPlayer = 20;
    [SerializeField] private float forcaKnockbackNoPlayer = 8f; 
    [SerializeField] private float tempoAnimacaoMorte = 0.5f;

    [Header("Configurações de Morte (Knockback e Rotação)")]
    [SerializeField] private float forçaKnockbackMorte = 3f;
    [SerializeField] private float velocidadeRotacaoMorte = 60;

    [Header("Componentes Visuais")]
    [SerializeField] private AnimEnemy scriptAnimacao;
    [SerializeField] private SpriteEffects scriptEfeitos;

    [Header("Configurações de Efeitos e Partículas")]
    [SerializeField] private GameObject prefabExplosao;

    [Header("Configurações de Drops")]
    [SerializeField] private GameObject prefabMoeda;

    protected Transform alvoPlayer;
    protected Rigidbody2D rb;
    protected Vector2 direcao;
    protected bool olhandoParaDireita = false;
    protected bool estaMorto = false;

    private float tempoProximoDano = 0f;
    private float intervaloInvenclibilidade = 0.1f;

    protected virtual void Start()
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
        if (estaMorto) return;

        rb.linearVelocity = direcao * velocidade;

        if (scriptAnimacao != null)
        {
            scriptAnimacao.AtualizarMovimento(direcao.magnitude);
        }
    }

    public void TomarDano(int quantidadeDano)
    {
        if (estaMorto || UnityEngine.Time.time < tempoProximoDano) return;

        vidaAtual -= quantidadeDano;
        tempoProximoDano = UnityEngine.Time.time + intervaloInvenclibilidade;

        CriarPopUpDano(quantidadeDano);

        if (CameraShake.Instancia != null)
        {
            CameraShake.Instancia.Tremer(duracaoShakeDano, intensidadeShakeDano);
        }

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

    private void CriarPopUpDano(int quantidadeDano)
    {
        if (prefabDamagePopup == null) return;

        Vector3 offsetAleatorio = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.3f, 0.6f), 0f);
        Vector3 posCriacao = transform.position + offsetAleatorio;

        GameObject popupObj = Instantiate(prefabDamagePopup, posCriacao, Quaternion.identity);
        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();

        if (popupScript != null)
        {
            popupScript.Setup(quantidadeDano);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                Vector2 direcaoEmpurrao = (player.transform.position - transform.position).normalized;

                player.TomarDano(danoNoPlayer, direcaoEmpurrao, forcaKnockbackNoPlayer);
            }

        }
    }

    protected virtual void IniciarProcessoMorte(bool deveDroparMoeda)
    {
        estaMorto = true;

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
            Instantiate(prefabExplosao, transform.position, Quaternion.identity, transform);
        }

        if (deveDroparMoeda && prefabMoeda != null)
        {
            Instantiate(prefabMoeda, transform.position, Quaternion.identity);
        }

        if (rb != null && alvoPlayer != null)
        {
            Vector2 direcaoEmpurrao = (transform.position - alvoPlayer.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direcaoEmpurrao * forçaKnockbackMorte, ForceMode2D.Impulse);
        }

        StartCoroutine(RotinaDestruição());
    }

    private IEnumerator RotinaDestruição()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        Color corInicial = sr != null ? sr.color : Color.white;
        float tempo = 0f;

        float sentidoRotacao = Random.value > 0.5f ? 1f : -1f;

        while (tempo < tempoAnimacaoMorte)
        {
            tempo += UnityEngine.Time.deltaTime;
            float progresso = tempo / tempoAnimacaoMorte;

            transform.Rotate(0f, 0f, sentidoRotacao * velocidadeRotacaoMorte * UnityEngine.Time.deltaTime);

            if (sr != null)
            {
                float alpha = Mathf.Lerp(corInicial.a, 0f, progresso);
                sr.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            }

            yield return null;
        }

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