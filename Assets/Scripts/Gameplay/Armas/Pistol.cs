using System.Collections;
using UnityEngine;

public class Pistol : BaseWeapon
{
    [Header("Configuracoes da Pistola")]
    [SerializeField] private GameObject prefabProjetil;
    [SerializeField] private Transform puntoDeDisparo;
    [SerializeField] private float velocidadeProjetil = 12f;

    [Header("Configuracoes de Upgrade")]
    [SerializeField] private int quantidadeBalas = 1;
    [SerializeField] private float anguloConeSpread = 30f;

    [Header("Configuracoes de Posicionamento e Auto-Mira")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);
    [SerializeField] private float raioPosicionamento = 0.8f;
    [SerializeField] private float distanciaMaximaAlvo = 7f;

    [Header("Anti-Sobreposicao com a Espada")]
    [SerializeField] private float anguloMinimoSeparacaoEspada = 90f;

    [Header("Configuracoes de Suavizacao e Flutuacao")]
    [SerializeField] private float velocidadeSuavizacaoPosicao = 10f;
    [SerializeField] private float velocidadeSuavizacaoRotacao = 12f;
    [SerializeField] private float amplitudeFlutuacao = 0.08f;
    [SerializeField] private float velocidadeFlutuacao = 2.5f;

    [Header("Configuracoes de Jackpot")]
    [SerializeField] private bool testarJackpotNoInspector = false;
    [SerializeField] private float duracaoJackpot = 5f;
    [SerializeField] private float velocidadeGiroJackpot = 720f;
    [SerializeField] private float tempoEntreTirosJackpot = 0.08f;
    [SerializeField] private Vector2 offsetJackpot = new Vector2(0f, 1f);
    [SerializeField] private float raioGiroJackpot = 1.5f;

    [Header("Configuracao de Ricochete Base")]
    [SerializeField] private int quantidadeRicochetes = 0;

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

        if (Application.isPlaying)
        {
            if (testarJackpotNoInspector && !EstaEmModoJackpot)
            {
                AtivarJackpot(offsetJackpot, raioGiroJackpot);
            }
        }

        if (!EstaEmModoJackpot && player != null)
        {
            ConfigurarMiraEPosicaoSuave();
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

    private void ConfigurarMiraEPosicaoSuave()
    {
        Vector2 posCentroPlayer = (Vector2)player.transform.position + centroDoPlayerOffset;
        Enemy inimigoAlvo = ObterInimigoMaisProximo();
        Vector2 direcaoAlvo;

        if (inimigoAlvo != null)
        {
            direcaoAlvo = ((Vector2)inimigoAlvo.transform.position - posCentroPlayer).normalized;
        }
        else
        {
            direcaoAlvo = Vector2.right;
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.flipX) direcaoAlvo = Vector2.left;
            else if (player.transform.localScale.x < 0f) direcaoAlvo = Vector2.left;
        }

        float anguloAlvo = Mathf.Atan2(direcaoAlvo.y, direcaoAlvo.x) * Mathf.Rad2Deg;

        Sword espada = FindObjectOfType<Sword>();
        if (espada != null && espada.gameObject.activeInHierarchy)
        {
            Vector2 posEspadaRelativa = (Vector2)espada.transform.position - posCentroPlayer;
            float anguloEspada = Mathf.Atan2(posEspadaRelativa.y, posEspadaRelativa.x) * Mathf.Rad2Deg;

            float diferencaAngulo = Mathf.DeltaAngle(anguloEspada, anguloAlvo);

            if (Mathf.Abs(diferencaAngulo) < anguloMinimoSeparacaoEspada)
            {
                float direcaoDesvio = diferencaAngulo >= 0 ? 1f : -1f;
                anguloAlvo = anguloEspada + (anguloMinimoSeparacaoEspada * direcaoDesvio);
            }
        }

        float radianoAlvo = anguloAlvo * Mathf.Deg2Rad;
        Vector2 direcaoOrbitaFinal = new Vector2(Mathf.Cos(radianoAlvo), Mathf.Sin(radianoAlvo));

        float flutuacaoY = Mathf.Sin(UnityEngine.Time.time * velocidadeFlutuacao) * amplitudeFlutuacao;
        Vector3 posAlvo = posCentroPlayer + (direcaoOrbitaFinal * raioPosicionamento) + new Vector2(0f, flutuacaoY);

        transform.position = Vector3.Lerp(transform.position, posAlvo, UnityEngine.Time.deltaTime * velocidadeSuavizacaoPosicao);

        Quaternion rotAlvo = Quaternion.Euler(0, 0, anguloAlvo);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotAlvo, UnityEngine.Time.deltaTime * velocidadeSuavizacaoRotacao);
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
        Vector2 offsetFinal = offsetJackpot;
        float raioFinal = raioGiroJackpot;

        if (offset != Vector2.zero) offsetFinal = offset;
        if (raio > 0f) raioFinal = raio;

        EstaEmModoJackpot = true;
        StartCoroutine(RotinaJackpotPistola(offsetFinal, raioFinal));
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
            timerJackpot -= UnityEngine.Time.deltaTime;
            timerTiro += UnityEngine.Time.deltaTime;

            anguloGiro += velocidadeGiroJackpot * UnityEngine.Time.deltaTime;
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
        testarJackpotNoInspector = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centro = player != null ? player.transform.position + (Vector3)centroDoPlayerOffset : transform.position;
        Gizmos.DrawWireSphere(centro, distanciaMaximaAlvo);
    }
}