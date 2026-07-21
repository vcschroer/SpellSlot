using System.Collections;
using UnityEngine;
using static EnemySpawn;

public class Enemy : MonoBehaviour
{
    [Header("Configurações de Vida")]
    [SerializeField] protected int vidaAtual = 10;

    [Header("Configurações de Feedback de Dano")]
    [Tooltip("Prefab do TextMeshPro 2D com o script DamagePopup")]
    [SerializeField] private GameObject prefabDamagePopup;

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaMinima = 0.5f;

    [Header("Configurações de Ataque")]
    [SerializeField] public int danoNoPlayer = 20;
    [SerializeField] private float tempoAnimacaoMorte = 0.5f;

    [Header("Componentes Visuais")]
    [SerializeField] private AnimEnemy scriptAnimacao;
    [SerializeField] private SpriteEffects scriptEfeitos;

    [Header("Configurações de Efeitos e Partículas")]
    [Tooltip("Arraste aqui o Prefab da partícula de explosão")]
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

    public void TomarDano(int quantidadeDano)
    {
        if (estaMorto || Time.time < tempoProximoDano) return;

        vidaAtual -= quantidadeDano;
        tempoProximoDano = Time.time + intervaloInvenclibilidade;

        CriarPopUpDano(quantidadeDano);

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
                player.TomarDano(danoNoPlayer); 
            }

            Destroy(gameObject);
        }
    }

    protected virtual void IniciarProcessoMorte(bool deveDroparMoeda)
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