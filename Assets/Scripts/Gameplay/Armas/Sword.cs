using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sword : BaseWeapon
{
    [Header("Configurações de Órbita")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);
    [SerializeField] private float raioDaOrbita = 1.2f;

    [Header("Configurações Base do Ataque")]
    [SerializeField] private float anguloInicial = 45f;
    [SerializeField] private float anguloFinal = -45f;
    [SerializeField] private float velocidadeAtaqueBase = 20f;
    [SerializeField] private float velocidadeRetornoBase = 5f;

    [Header("Prefabs dos Segmentos")]
    [SerializeField] private GameObject prefabCabo;
    [SerializeField] private GameObject prefabMeioLamina;
    [SerializeField] private GameObject prefabPontaLamina;

    [Header("Configurações de Tamanho")]
    [SerializeField] private int quantidadeSegmentosMeio = 3;
    [SerializeField] private float tamanhoDoSegmentoY = 0.5f;
    [SerializeField] private float deslocamentoDaPonta = 0.5f;

    [Header("Configurações de Hitbox Global")]
    [SerializeField] private float larguraDoCorte = 0.8f;
    [SerializeField] private LayerMask layerDosInimigos;

    [Header("Configurações de Jackpot")]
    [SerializeField] public float duracaoJackpot = 5f;
    [SerializeField] public float velocidadeGiroJackpot = 360f;

    private float tempoJackpotRestante;
    private float anguloRotacaoJackpot = 0f;
    private bool atacando = false;
    private float anguloMiraMouse;
    private List<GameObject> segmentosCriados = new List<GameObject>();
    private bool deveReconstruir = false;
    private HashSet<Enemy> inimigosAtingidosNesteGolpe = new HashSet<Enemy>();

    public int QuantidadeSegmentosMeio => quantidadeSegmentosMeio;
    public bool EstaAtacando => atacando;
    public float AnguloInicial => anguloInicial;

    protected override void Start()
    {
        tipoArma = TipoArma.Espada; // Garante o tipo correto
        base.Start();
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
            AtualizarPosicaoOrbita();
        }
    }

    protected override void DispararAtaqueAutomatico()
    {
        if (!atacando && player != null)
        {
            if (MusicManager.Instance != null) MusicManager.Instance.PlaySFX("Atack espada");

            player.DispararAnimacaoAtaqueExterno();

            // 🌟 Passa o weaponAttackSpeed exclusivo da espada para a animação do golpe!
            StartCoroutine(RotinaGolpe(weaponAttackSpeed, anguloMiraMouse));
        }
    }

    private void AtualizarPosicaoOrbita()
    {
        if (player == null || Camera.main == null || Mouse.current == null) return;

        Vector2 posicaoMouseTela = Mouse.current.position.ReadValue();
        Vector3 posicaoMouseMundo = Camera.main.ScreenToWorldPoint(posicaoMouseTela);
        posicaoMouseMundo.z = 0f;

        Vector3 centroRealPlayer = player.transform.position + (Vector3)centroDoPlayerOffset;
        Vector2 direcao = (posicaoMouseMundo - centroRealPlayer).normalized;

        anguloMiraMouse = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        Vector3 deslocamento = (Vector3)direcao * raioDaOrbita;

        transform.localPosition = (Vector3)centroDoPlayerOffset + deslocamento;

        float ajusteAngulo = (anguloMiraMouse > 90f || anguloMiraMouse < -90f) ? -anguloInicial : anguloInicial;
        transform.localRotation = Quaternion.Euler(0, 0, anguloMiraMouse + 90f + ajusteAngulo);
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

    private IEnumerator RotinaGolpe(float multiplicador, float anguloDoClique)
    {
        atacando = true;
        inimigosAtingidosNesteGolpe.Clear();

        float velAtaqueAtual = velocidadeAtaqueBase * multiplicador;
        bool estaNaEsquerda = anguloDoClique > 90f || anguloDoClique < -90f;
        float anguloBaseAtaque = anguloDoClique + 90f;

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
                    inimigo.TomarDano();
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
}