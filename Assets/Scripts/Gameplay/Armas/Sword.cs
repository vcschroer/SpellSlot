using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : BaseWeapon
{
    [Header("Configuracoes de Dano")]
    [SerializeField] private int danoEspada = 15;

    [Header("Configuracoes de Posicao Fixa")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);
    [SerializeField] private float distanciaDireita = 0.8f;
    [SerializeField] private float distanciaEsquerda = 0.8f;
    [SerializeField] private float anguloEmRepouso = 0f;

    [Header("Configuracoes de Suavizacao e Flutuacao")]
    [SerializeField] private float velocidadeSuavizacaoPosicao = 10f;
    [SerializeField] private float velocidadeSuavizacaoRotacao = 12f;
    [SerializeField] private float amplitudeFlutuacao = 0.12f;
    [SerializeField] private float velocidadeFlutuacao = 3f;

    [Header("Efeito Squash & Stretch no Ataque")]
    [SerializeField] private float stretchAtaqueY = 1.35f;
    [SerializeField] private float squashAtaqueX = 0.7f;

    [Header("Configuracoes Base do Ataque")]
    [SerializeField] private float anguloInicial = 45f;
    [SerializeField] private float anguloFinal = -45f;
    [SerializeField] private float velocidadeAtaqueBase = 20f;
    [SerializeField] private float velocidadeRetornoBase = 5f;
    [SerializeField] private float anguloInicialMaximo = 260f;

    [Header("Efeito de Rastro (Trail)")]
    [SerializeField] private TrailRenderer rastroEspada;
    [SerializeField] private float larguraBaseTrail = 0.4f;
    [SerializeField] private float incrementoLarguraPorSegmento = 0.25f;

    [Header("Prefabs dos Segmentos")]
    [SerializeField] private GameObject prefabCabo;
    [SerializeField] private GameObject prefabMeioLamina;
    [SerializeField] private GameObject prefabPontaLamina;

    [Header("Configuracoes de Tamanho")]
    [SerializeField] private int quantidadeSegmentosMeio = 3;
    [SerializeField] private float tamanhoDoSegmentoY = 0.5f;
    [SerializeField] private float deslocamentoDaPonta = 0.5f;

    [Header("Configuracoes de Hitbox Global")]
    [SerializeField] private float larguraDoCorte = 0.8f;
    [SerializeField] private LayerMask layerDosInimigos;

    [Header("Configuracoes de Jackpot")]
    [SerializeField] public float duracaoJackpot = 5f;
    [SerializeField] public float velocidadeGiroJackpot = 360f;

    private float tempoJackpotRestante;
    private float anguloRotacaoJackpot = 0f;
    private bool atacando = false;
    private List<GameObject> segmentosCriados = new List<GameObject>();
    private bool deveReconstruir = false;
    private HashSet<Enemy> inimigosAtingidosNesteGolpe = new HashSet<Enemy>();

    private Vector3 escalaOriginal;

    public int QuantidadeSegmentosMeio => quantidadeSegmentosMeio;
    public bool EstaAtacando => atacando;
    public float AnguloInicial => anguloInicial;

    protected override void Start()
    {
        tipoArma = TipoArma.Espada;
        base.Start();

        escalaOriginal = transform.localScale;

        ConstruirEspada();

        if (rastroEspada != null)
        {
            rastroEspada.emitting = false;
        }

        if (layerDosInimigos == 0) layerDosInimigos = LayerMask.GetMask("Default");
    }

    private void OnValidate() => deveReconstruir = true;

    protected override void Update()
    {
        base.Update();

        if (deveReconstruir)
        {
            deveReconstruir = false;
            ConstruirEspada();
        }

        if (!atacando && !EstaEmModoJackpot)
        {
            AtualizarPosicaoFixa();
        }
    }

    protected override void DispararAtaqueAutomatico()
    {
        if (!atacando && player != null)
        {
            if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("Atack espada");

            player.DispararAnimacaoAtaqueExterno();

            float direcaoX = ObterLadoDoAlvo();
            float anguloDirecao = direcaoX < 0 ? 180f : 0f;

            float distanciaAtual = direcaoX > 0 ? distanciaDireita : -distanciaEsquerda;
            transform.localPosition = new Vector3(centroDoPlayerOffset.x + distanciaAtual, centroDoPlayerOffset.y, 0f);
            transform.localScale = new Vector3(escalaOriginal.x * direcaoX, escalaOriginal.y, escalaOriginal.z);

            StartCoroutine(RotinaGolpe(weaponAttackSpeed, anguloDirecao, direcaoX));
        }
    }

    private void AtualizarPosicaoFixa()
    {
        if (player == null) return;

        float direcaoX = ObterLadoDoAlvo();
        float distanciaAtual = direcaoX > 0 ? distanciaDireita : -distanciaEsquerda;

        float deslocamentoFlutuanteY = Mathf.Sin(UnityEngine.Time.time * velocidadeFlutuacao) * amplitudeFlutuacao;
        float inclinacaoFlutuanteZ = Mathf.Cos(UnityEngine.Time.time * velocidadeFlutuacao * 0.5f) * 3f;

        Vector3 posAlvo = new Vector3(
            centroDoPlayerOffset.x + distanciaAtual,
            centroDoPlayerOffset.y + deslocamentoFlutuanteY,
            0f
        );
        transform.localPosition = Vector3.Lerp(transform.localPosition, posAlvo, UnityEngine.Time.deltaTime * velocidadeSuavizacaoPosicao);

        Vector3 escalaAlvo = new Vector3(escalaOriginal.x * direcaoX, escalaOriginal.y, escalaOriginal.z);
        transform.localScale = Vector3.Lerp(transform.localScale, escalaAlvo, UnityEngine.Time.deltaTime * velocidadeSuavizacaoPosicao);

        float anguloCalculado = (direcaoX < 0 ? -anguloEmRepouso : anguloEmRepouso) + inclinacaoFlutuanteZ;
        Quaternion rotAlvo = Quaternion.Euler(0, 0, anguloCalculado);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, rotAlvo, UnityEngine.Time.deltaTime * velocidadeSuavizacaoRotacao);
    }

    private float ObterLadoDoAlvo()
    {
        Transform inimigoMaisProximo = ObterPosicaoInimigoMaisProximo();

        if (inimigoMaisProximo != null)
        {
            return (inimigoMaisProximo.position.x < player.transform.position.x) ? -1f : 1f;
        }

        return ObterDirecaoOlharPlayer();
    }

    private Transform ObterPosicaoInimigoMaisProximo()
    {
        GameObject[] inimigos = GameObject.FindGameObjectsWithTag("Enemy");
        Transform maisProximo = null;
        float menorDistancia = float.MaxValue;
        Vector3 posicaoPlayer = player != null ? player.transform.position : transform.position;

        foreach (GameObject inimigoObj in inimigos)
        {
            if (inimigoObj == null) continue;

            float distancia = Vector3.Distance(posicaoPlayer, inimigoObj.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = inimigoObj.transform;
            }
        }

        return maisProximo;
    }

    private float ObterDirecaoOlharPlayer()
    {
        if (player == null) return 1f;

        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.flipX) return -1f;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.x < -0.1f) return -1f;

        if (player.transform.localScale.x < 0f) return -1f;

        return 1f;
    }

    public void MudarQuantidadeSegmentos(int novaQuantidade)
    {
        quantidadeSegmentosMeio = Mathf.Max(0, novaQuantidade);
        ConstruirEspada();
    }

    private void ConstruirEspada()
    {
        foreach (GameObject seg in segmentosCriados)
        {
            if (seg != null)
            {
                if (Application.isPlaying) Destroy(seg);
                else DestroyImmediate(seg);
            }
        }
        segmentosCriados.Clear();

        float posY = 0f;
        if (prefabCabo != null)
        {
            GameObject cabo = Instantiate(prefabCabo, transform);
            cabo.transform.localPosition = new Vector3(0, posY, 0);
            segmentosCriados.Add(cabo);
            posY += tamanhoDoSegmentoY;
        }

        for (int i = 0; i < quantidadeSegmentosMeio; i++)
        {
            GameObject meio = Instantiate(prefabMeioLamina, transform);
            meio.transform.localPosition = new Vector3(0, posY, 0);
            segmentosCriados.Add(meio);
            posY += tamanhoDoSegmentoY;
        }

        if (prefabPontaLamina != null)
        {
            GameObject ponta = Instantiate(prefabPontaLamina, transform);
            ponta.transform.localPosition = new Vector3(0, (posY - tamanhoDoSegmentoY) + deslocamentoDaPonta, 0);
            segmentosCriados.Add(ponta);

            TrailRenderer trailNaPonta = ponta.GetComponentInChildren<TrailRenderer>();
            if (trailNaPonta != null)
            {
                rastroEspada = trailNaPonta;
                rastroEspada.emitting = false;
            }
        }

        AtualizarLarguraDoRastro();
    }

    private void AtualizarLarguraDoRastro()
    {
        if (rastroEspada != null)
        {
            float larguraCalculada = larguraBaseTrail + (quantidadeSegmentosMeio * incrementoLarguraPorSegmento);
            rastroEspada.widthMultiplier = larguraCalculada;
        }
    }

    private IEnumerator RotinaGolpe(float multiplicador, float anguloBaseDirecao, float direcaoX)
    {
        atacando = true;
        inimigosAtingidosNesteGolpe.Clear();

        float velAtaqueAtual = velocidadeAtaqueBase * multiplicador;
        float anguloBaseAtaque = anguloBaseDirecao + 90f;

        bool estaNaEsquerda = anguloBaseDirecao > 90f;

        float anguloLocalInicial = estaNaEsquerda ? (anguloBaseAtaque - anguloInicial) : (anguloBaseAtaque + anguloInicial);
        float anguloLocalFinal = estaNaEsquerda ? (anguloBaseAtaque - anguloFinal) : (anguloBaseAtaque + anguloFinal);

        if (rastroEspada != null)
        {
            rastroEspada.Clear();
            rastroEspada.emitting = true;
        }

        float progresso = 0f;
        while (progresso < 1f)
        {
            progresso += velAtaqueAtual * UnityEngine.Time.deltaTime;

            float zAtual = Mathf.LerpAngle(anguloLocalInicial, anguloLocalFinal, progresso);
            transform.localRotation = Quaternion.Euler(0, 0, zAtual);

            float fatorCurva = Mathf.Sin(progresso * Mathf.PI);
            float novoX = Mathf.Lerp(escalaOriginal.x, escalaOriginal.x * squashAtaqueX, fatorCurva) * direcaoX;
            float novoY = Mathf.Lerp(escalaOriginal.y, escalaOriginal.y * stretchAtaqueY, fatorCurva);
            transform.localScale = new Vector3(novoX, novoY, escalaOriginal.z);

            VerificarCorteEspada();
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0, 0, anguloLocalFinal);
        transform.localScale = new Vector3(escalaOriginal.x * direcaoX, escalaOriginal.y, escalaOriginal.z);

        if (rastroEspada != null)
        {
            rastroEspada.emitting = false;
        }

        yield return new WaitForSeconds(0.05f / multiplicador);

        progresso = 0f;
        float velRetornoAtual = velocidadeRetornoBase * multiplicador;
        while (progresso < 1f)
        {
            progresso += velRetornoAtual * UnityEngine.Time.deltaTime;

            float zAtual = Mathf.LerpAngle(anguloLocalFinal, anguloLocalInicial, progresso);
            transform.localRotation = Quaternion.Euler(0, 0, zAtual);

            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0, 0, anguloLocalInicial);
        transform.localScale = new Vector3(escalaOriginal.x * direcaoX, escalaOriginal.y, escalaOriginal.z);
        atacando = false;
    }

    private void VerificarCorteEspada()
    {
        float comprimentoTotal = (1 + quantidadeSegmentosMeio) * tamanhoDoSegmentoY + deslocamentoDaPonta;
        int pontosDeChecagem = Mathf.CeilToInt(comprimentoTotal / (larguraDoCorte * 0.4f));

        for (int i = 0; i <= pontosDeChecagem; i++)
        {
            float dist = (comprimentoTotal / pontosDeChecagem) * i;
            Vector3 pos = transform.TransformPoint(new Vector3(0, dist, 0));
            Collider2D[] colisores = Physics2D.OverlapCircleAll(pos, larguraDoCorte / 2f, layerDosInimigos);

            foreach (Collider2D col in colisores)
            {
                Enemy inimigo = col.GetComponent<Enemy>();
                if (inimigo != null && !inimigosAtingidosNesteGolpe.Contains(inimigo))
                {
                    inimigo.TomarDano(danoEspada);
                    inimigosAtingidosNesteGolpe.Add(inimigo);
                }
            }
        }
    }

    public override void AtivarJackpot(Vector2 offset, float raio)
    {
        EstaEmModoJackpot = true;
        if (MusicManager.Instance != null) MusicManager.Instance.PlayJackpotSound(true);
        tempoJackpotRestante = duracaoJackpot;
        StartCoroutine(RotinaJackpot(offset, raio));
    }

    private IEnumerator RotinaJackpot(Vector2 offset, float raio)
    {
        atacando = true;
        float intervaloParaRehit = 0.5f;
        float timerRehit = 0f;

        if (rastroEspada != null)
        {
            rastroEspada.Clear();
            rastroEspada.emitting = true;
        }

        while (tempoJackpotRestante > 0)
        {
            tempoJackpotRestante -= UnityEngine.Time.deltaTime;
            timerRehit += UnityEngine.Time.deltaTime;

            if (timerRehit >= intervaloParaRehit)
            {
                inimigosAtingidosNesteGolpe.Clear();
                timerRehit = 0f;
            }

            anguloRotacaoJackpot += velocidadeGiroJackpot * UnityEngine.Time.deltaTime;
            float radianos = anguloRotacaoJackpot * Mathf.Deg2Rad;
            Vector2 direcao = new Vector2(Mathf.Cos(radianos), Mathf.Sin(radianos));

            transform.localPosition = new Vector3(offset.x, offset.y, 0f) + (new Vector3(direcao.x, direcao.y, 0f) * raio);
            transform.localRotation = Quaternion.Euler(0, 0, anguloRotacaoJackpot - 90f);

            VerificarCorteEspada();
            yield return null;
        }

        if (rastroEspada != null)
        {
            rastroEspada.emitting = false;
        }

        atacando = false;
        EstaEmModoJackpot = false;
        if (MusicManager.Instance != null) MusicManager.Instance.PlayJackpotSound(false);
        transform.localRotation = Quaternion.identity;
        transform.localScale = escalaOriginal;
        inimigosAtingidosNesteGolpe.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!atacando) return;
        Gizmos.color = Color.red;
        float comprimentoTotal = (1 + quantidadeSegmentosMeio) * tamanhoDoSegmentoY + deslocamentoDaPonta;
        int pontosDeChecagem = Mathf.CeilToInt(comprimentoTotal / (larguraDoCorte * 0.4f));

        for (int i = 0; i <= pontosDeChecagem; i++)
        {
            float dist = (comprimentoTotal / pontosDeChecagem) * i;
            Vector3 pos = transform.TransformPoint(new Vector3(0, dist, 0));
            Gizmos.DrawWireSphere(pos, larguraDoCorte / 2f);
        }
    }

    public void AumentarAnguloCorte(float incremento)
    {
        anguloInicial = Mathf.Min(anguloInicial + incremento, anguloInicialMaximo);
        anguloFinal = -anguloInicial;
    }
}