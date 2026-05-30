using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
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
    [SerializeField] public int quantidadeSegmentosMeio = 3;
    [SerializeField] private float tamanhoDoSegmentoY = 0.5f;

    [Tooltip("Unidades extras para empurrar a ponta além do último gomo")]
    [SerializeField] private float deslocamentoDaPonta = 0.5f;

    [Header("Configurações de Hitbox Global")]
    [SerializeField] private float larguraDoCorte = 0.8f;
    [SerializeField] private LayerMask layerDosInimigos;

    private bool atacando = false;
    private List<GameObject> segmentosCriados = new List<GameObject>();
    private int quantidadeAnterior;

    private HashSet<Enemy> inimigosAtingidosNesteGolpe = new HashSet<Enemy>();

    public int QuantidadeSegmentosMeio => quantidadeSegmentosMeio;
    public bool EstaAtacando => atacando;
    public float AnguloInicial => anguloInicial;

    void Start()
    {
        quantidadeAnterior = quantidadeSegmentosMeio;
        ConstruirEspada();

        if (layerDosInimigos == 0)
        {
            layerDosInimigos = LayerMask.GetMask("Default");
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && quantidadeSegmentosMeio != quantidadeAnterior)
        {
            quantidadeAnterior = quantidadeSegmentosMeio;
            UnityEditor.EditorApplication.delayCall += SolicitacaoReconstrucaoEditor;
            ConstruirEspada();
        }
    }

    private void SolicitacaoReconstrucaoEditor()
    {
        UnityEditor.EditorApplication.delayCall -= SolicitacaoReconstrucaoEditor;
        if (this != null && Application.isPlaying)
        {
            ConstruirEspada();
        }
    }

    public void MudarQuantidadeSegmentos(int novaQuantidade)
    {
        quantidadeSegmentosMeio = novaQuantidade;
        quantidadeSegmentosMeio = Mathf.Max(0, quantidadeSegmentosMeio);
        quantidadeAnterior = quantidadeSegmentosMeio;
        ConstruirEspada();
    }

    private void ConstruirEspada()
    {
        foreach (GameObject segmento in segmentosCriados)
        {
            if (segmento != null) Destroy(segmento);
        }
        segmentosCriados.Clear();

        float posicaoYAtual = 0f;

        if (prefabCabo != null)
        {
            GameObject cabo = Instantiate(prefabCabo, transform);
            cabo.transform.localPosition = new Vector3(0, posicaoYAtual, 0);
            segmentosCriados.Add(cabo);
            posicaoYAtual += tamanhoDoSegmentoY;
        }

        for (int i = 0; i < quantidadeSegmentosMeio; i++)
        {
            GameObject meio = Instantiate(prefabMeioLamina, transform);
            meio.transform.localPosition = new Vector3(0, posicaoYAtual, 0);
            segmentosCriados.Add(meio);
            posicaoYAtual += tamanhoDoSegmentoY;
        }

        if (prefabPontaLamina != null)
        {
            GameObject ponta = Instantiate(prefabPontaLamina, transform);
            float baseDaPontaY = posicaoYAtual - tamanhoDoSegmentoY;
            ponta.transform.localPosition = new Vector3(0, baseDaPontaY + deslocamentoDaPonta, 0);
            segmentosCriados.Add(ponta);
        }
    }

    public void Atacar(float multiplicadorVelocidade, float anguloDoClique)
    {
        if (!atacando)
        {
            StartCoroutine(RotinaGolpe(multiplicadorVelocidade, anguloDoClique));
        }
    }

    private IEnumerator RotinaGolpe(float multiplicador, float anguloDoClique)
    {
        atacando = true;
        inimigosAtingidosNesteGolpe.Clear();

        float velAtaqueAtual = velocidadeAtaqueBase * multiplicador;

        bool estaNaEsquerda = anguloDoClique > 90f || anguloDoClique < -90f;

        float anguloBaseAtaque = anguloDoClique + 90f;

        float anguloLocalInicial;
        float anguloLocalFinal;

        if (estaNaEsquerda)
        {
            anguloLocalInicial = anguloBaseAtaque - anguloInicial;
            anguloLocalFinal = anguloBaseAtaque - anguloFinal;
        }
        else
        {
            anguloLocalInicial = anguloBaseAtaque + anguloInicial;
            anguloLocalFinal = anguloBaseAtaque + anguloFinal;
        }

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
            float distanciaAoLongoDaLamina = (comprimentoTotal / pontosDeChecagem) * i;
            Vector3 posicaoPonto = transform.TransformPoint(new Vector3(0, distanciaAoLongoDaLamina, 0));

            Collider2D[] colisoresEncontrados = Physics2D.OverlapCircleAll(posicaoPonto, larguraDoCorte / 2f, layerDosInimigos);

            foreach (Collider2D colisor in colisoresEncontrados)
            {
                Enemy inimigo = colisor.GetComponent<Enemy>();
                if (inimigo != null && !inimigosAtingidosNesteGolpe.Contains(inimigo))
                {
                    inimigo.TomarDano();
                    inimigosAtingidosNesteGolpe.Add(inimigo);
                }
            }
        }
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
