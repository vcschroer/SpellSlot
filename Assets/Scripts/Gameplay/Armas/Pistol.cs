using System.Collections;
using UnityEngine;

public class Pistol : BaseWeapon
{
    [Header("Configuracoes da Pistola")]
    [SerializeField] private GameObject prefabProjetil;
    [SerializeField] private Transform puntoDeDisparo;
    [SerializeField] private float velocidadeProjetil = 12f;

    [Header("Configuracoes de Upgrade (Leque/Cone)")]
    [Tooltip("Quantidade de balas disparadas por clique/ataque. Pode ser aumentado via script/roleta.")]
    [SerializeField] private int quantidadeBalas = 1;

    [Tooltip("A abertura total (em graus) do cone de tiros quando houver mais de 1 bala.")]
    [SerializeField] private float anguloConeSpread = 30f;

    [Header("Configuracoes de Posicionamento e Auto-Mira")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);
    [Tooltip("O raio do circulo ao redor do player onde a pistola vai se posicionar")]
    [SerializeField] private float raioPosicionamento = 0.8f;

    [Tooltip("Multiplicador do raio de posicionamento caso o player tenha uma Espada equipada")]
    [SerializeField] private float multiplicadorComEspada = 3f;

    [Tooltip("Distancia maxima que a pistola consegue detectar e atirar em um inimigo")]
    [SerializeField] private float distanciaMaximaAlvo = 7f;

    [Header("Configuracoes de Jackpot")]
    [SerializeField] private float duracaoJackpot = 5f;
    [SerializeField] private float velocidadeGiroJackpot = 720f;
    [SerializeField] private float tempoEntreTirosJackpot = 0.08f;

    [Header("Configuracao de Ricochete Base")]
    [SerializeField] private int quantidadeRicochetes = 0;

    private float anguloMira;
    private SpriteRenderer spriteRenderer;

    public int QuantidadeBalas
    {
        get => quantidadeBalas;
        set => quantidadeBalas = Mathf.Max(1, value);
    }

    public int QuantidadeRicochetes
    {
        get => quantidadeRicochetes;
        set => quantidadeRicochetes = Mathf.Max(0, value);
    }

    protected override void Start()
    {
        tipoArma = TipoArma.Pistola;
        base.Start();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        if (!EstaEmModoJackpot && player != null)
        {
            ConfigurarMiraEPosicaoAutomatica();
        }

        AjustarFlipDoSprite();
    }

    protected override void DispararAtaqueAutomatico()
    {
        if (ObterInimigoMaisProximo() != null)
        {
            Atirar();
        }
    }

    private void ConfigurarMiraEPosicaoAutomatica()
    {
        Vector2 posCentroPlayer = (Vector2)player.transform.position + centroDoPlayerOffset;
        Enemy inimigoAlvo = ObterInimigoMaisProximo();
        Vector2 direcaoMira;

        if (inimigoAlvo != null)
        {
            direcaoMira = ((Vector2)inimigoAlvo.transform.position - posCentroPlayer).normalized;
        }
        else
        {
            direcaoMira = Vector2.right;
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.flipX) direcaoMira = Vector2.left;
            else if (player.transform.localScale.x < 0f) direcaoMira = Vector2.left;
        }

        float raioCalculado = raioPosicionamento;

        Sword espada = FindObjectOfType<Sword>();
        if (espada != null && espada.gameObject.activeInHierarchy)
        {
            raioCalculado *= multiplicadorComEspada;
        }

        transform.position = posCentroPlayer + (direcaoMira * raioCalculado);
        anguloMira = Mathf.Atan2(direcaoMira.y, direcaoMira.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, anguloMira);
    }

    private void AjustarFlipDoSprite()
    {
        if (spriteRenderer == null) return;

        float anguloZ = transform.eulerAngles.z;

        if (anguloZ > 90f && anguloZ < 270f)
        {
            spriteRenderer.flipY = true;
        }
        else
        {
            spriteRenderer.flipY = false;
        }
    }

    private Enemy ObterInimigoMaisProximo()
    {
        Enemy[] todosOsInimigos = FindObjectsOfType<Enemy>();
        Enemy inimigoMaisProximo = null;
        float menorDistancia = float.MaxValue;
        Vector2 posCentroPlayer = (Vector2)player.transform.position + centroDoPlayerOffset;

        foreach (Enemy inimigo in todosOsInimigos)
        {
            if (inimigo == null) continue;

            Collider2D col = inimigo.GetComponent<Collider2D>();
            if (col != null && !col.enabled) continue;

            float distancia = Vector2.Distance(posCentroPlayer, inimigo.transform.position);

            if (distancia <= distanciaMaximaAlvo && distancia < menorDistancia)
            {
                menorDistancia = distancia;
                inimigoMaisProximo = inimigo;
            }
        }

        return inimigoMaisProximo;
    }

    private void Atirar()
    {
        if (prefabProjetil == null || puntoDeDisparo == null) return;

        if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("Tiro pistola");

        float anguloAtualArma = transform.eulerAngles.z;

        if (quantidadeBalas <= 1)
        {
            InstanciarProjetil(Quaternion.Euler(0, 0, anguloAtualArma));
        }
        else
        {
            float anguloInicial = -anguloConeSpread / 2f;
            float passoAngulo = anguloConeSpread / (quantidadeBalas - 1);

            for (int i = 0; i < quantidadeBalas; i++)
            {
                float anguloOffset = anguloInicial + (passoAngulo * i);
                Quaternion rotacaoCalculada = Quaternion.Euler(0, 0, anguloAtualArma + anguloOffset);
                InstanciarProjetil(rotacaoCalculada);
            }
        }
    }

    private void InstanciarProjetil(Quaternion rotacao)
    {
        GameObject bala = Instantiate(prefabProjetil, puntoDeDisparo.position, rotacao);
        Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();

        if (rbBala != null)
        {
            rbBala.linearVelocity = bala.transform.right * velocidadeProjetil;
        }

        Bullet scriptBala = bala.GetComponent<Bullet>();
        if (scriptBala != null)
        {
            scriptBala.RicochetesRestantes = quantidadeRicochetes;
        }

        Destroy(bala, 3f);
    }

    public override void AtivarJackpot(Vector2 offset, float raio)
    {
        EstaEmModoJackpot = true;
        StartCoroutine(RotinaJackpotPistola(offset, raio));
    }

    private IEnumerator RotinaJackpotPistola(Vector2 offset, float raio)
    {
        int quantidadeBalasOriginal = quantidadeBalas;
        quantidadeBalas *= 2;

        float tempoEsperaTiroJackpot = tempoEntreTirosJackpot / 2f;

        float timerJackpot = duracaoJackpot;
        float timerTiro = 0f;
        float anguloGiro = 0f;

        while (timerJackpot > 0)
        {
            timerJackpot -= Time.deltaTime;
            timerTiro += Time.deltaTime;

            anguloGiro += velocidadeGiroJackpot * Time.deltaTime;
            float radianos = anguloGiro * Mathf.Deg2Rad;
            Vector2 direcaoOrbita = new Vector2(Mathf.Cos(radianos), Mathf.Sin(radianos));

            if (player != null)
            {
                Vector2 posCentroPlayer = (Vector2)player.transform.position + offset;
                transform.position = (Vector3)posCentroPlayer + (new Vector3(direcaoOrbita.x, direcaoOrbita.y, 0f) * raio);
            }

            transform.rotation = Quaternion.Euler(0, 0, anguloGiro);

            if (timerTiro >= tempoEsperaTiroJackpot)
            {
                timerTiro = 0f;
                Atirar();
            }

            yield return null;
        }

        quantidadeBalas = quantidadeBalasOriginal;
        EstaEmModoJackpot = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centro = player != null ? player.transform.position + (Vector3)centroDoPlayerOffset : transform.position;
        Gizmos.DrawWireSphere(centro, distanciaMaximaAlvo);
    }
}