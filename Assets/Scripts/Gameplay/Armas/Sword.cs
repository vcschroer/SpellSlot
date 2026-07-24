using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : BaseWeapon
{
    [Header("Configuracoes de Dano")]
    [SerializeField] private int danoEspada = 15;

    [Header("Configuracoes de Posicao Fixa")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);

    [Tooltip("Distancia que a espada fica quando o player olha para a DIREITA")]
    [SerializeField] private float distanciaDireita = 0.8f;

    [Tooltip("Distancia que a espada fica quando o player olha para a ESQUERDA")]
    [SerializeField] private float distanciaEsquerda = 0.8f;

    [Tooltip("O angulo que a espada fica inclinada quando nao esta atacando")]
    [SerializeField] private float anguloEmRepouso = 0f;

    [Header("Configuracoes Base do Ataque")]
    [SerializeField] private float anguloInicial = 45f;
    [SerializeField] private float anguloFinal = -45f;
    [SerializeField] private float velocidadeAtaqueBase = 20f;
    [SerializeField] private float velocidadeRetornoBase = 5f;
    [SerializeField] private float anguloInicialMaximo = 260f; 

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

            StartCoroutine(RotinaGolpe(weaponAttackSpeed, anguloDirecao));
        }
    }

    private void AtualizarPosicaoFixa()
    {
        if (player == null) return;

        float direcaoX = ObterLadoDoAlvo();

        float distanciaAtual = direcaoX > 0 ? distanciaDireita : -distanciaEsquerda;

        transform.localPosition = new Vector3(centroDoPlayerOffset.x + distanciaAtual, centroDoPlayerOffset.y, 0f);

        transform.localScale = new Vector3(escalaOriginal.x * direcaoX, escalaOriginal.y, escalaOriginal.z);

        float anguloCalculado = direcaoX < 0 ? -anguloEmRepouso : anguloEmRepouso;
        transform.localRotation = Quaternion.Euler(0, 0, anguloCalculado);
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
        }
    }

    private IEnumerator RotinaGolpe(float multiplicador, float anguloBaseDirecao)
    {
        atacando = true;
        inimigosAtingidosNesteGolpe.Clear();

        float velAtaqueAtual = velocidadeAtaqueBase * multiplicador;
        float anguloBaseAtaque = anguloBaseDirecao + 90f;

        bool estaNaEsquerda = anguloBaseDirecao > 90f;

        float anguloLocalInicial = estaNaEsquerda ? (anguloBaseAtaque - anguloInicial) : (anguloBaseAtaque + anguloInicial);
        float anguloLocalFinal = estaNaEsquerda ? (anguloBaseAtaque - anguloFinal) : (anguloBaseAtaque + anguloFinal);

        float progresso = 0f;
        while (progresso < 1f)
        {
            progresso += velAtaqueAtual * Time.deltaTime;
            float zAtual = Mathf.LerpAngle(anguloLocalInicial, anguloLocalFinal, progresso);
            transform.localRotation = Quaternion.Euler(0, 0, zAtual);
            VerificarCorteEspada();
            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0, 0, anguloLocalFinal);

        yield return new WaitForSeconds(0.05f / multiplicador);

        progresso = 0f;
        float velRetornoAtual = velocidadeRetornoBase * multiplicador;
        while (progresso < 1f)
        {
            progresso += velRetornoAtual * Time.deltaTime;
            float zAtual = Mathf.LerpAngle(anguloLocalFinal, anguloLocalInicial, progresso);
            transform.localRotation = Quaternion.Euler(0, 0, zAtual);
            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0, 0, anguloLocalInicial);
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

        while (tempoJackpotRestante > 0)
        {
            tempoJackpotRestante -= Time.deltaTime;
            timerRehit += Time.deltaTime;

            if (timerRehit >= intervaloParaRehit)
            {
                inimigosAtingidosNesteGolpe.Clear();
                timerRehit = 0f;
            }

            anguloRotacaoJackpot += velocidadeGiroJackpot * Time.deltaTime;
            float radianos = anguloRotacaoJackpot * Mathf.Deg2Rad;
            Vector2 direcao = new Vector2(Mathf.Cos(radianos), Mathf.Sin(radianos));

            transform.localPosition = new Vector3(offset.x, offset.y, 0f) + (new Vector3(direcao.x, direcao.y, 0f) * raio);
            transform.localRotation = Quaternion.Euler(0, 0, anguloRotacaoJackpot - 90f);

            VerificarCorteEspada();
            yield return null;
        }

        atacando = false;
        EstaEmModoJackpot = false;
        if (MusicManager.Instance != null) MusicManager.Instance.PlayJackpotSound(false);
        transform.localRotation = Quaternion.identity;
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