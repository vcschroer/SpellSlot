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

    [Tooltip("Unidades extras para empurrar a ponta além do último gomo (Ex: 0.5 deixa colado, 0.7 deixa um espaço)")]
    [SerializeField] private float deslocamentoDaPonta = 0.5f;

    [Header("Configurações de Hitbox Global")]
    [Tooltip("Largura da área de corte da espada")]
    [SerializeField] private float larguraDoCorte = 0.8f;
    [SerializeField] private LayerMask layerDosInimigos;

    private bool atacando = false;
    private List<GameObject> segmentosCriados = new List<GameObject>();
    private int quantidadeAnterior;

    private HashSet<Enemy> inimigosAtingidosNesteGolpe = new HashSet<Enemy>();

    public int QuantidadeSegmentosMeio => quantidadeSegmentosMeio;
    public bool EstaAtacando => atacando;

    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, anguloInicial);
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
            if (quantidadeSegmentosMeio < 0) quantidadeSegmentosMeio = 0;
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
        if (quantidadeSegmentosMeio < 0) quantidadeSegmentosMeio = 0;
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

    public void Atacar(float multiplicadorVelocidade)
    {
        if (!atacando)
        {
            StartCoroutine(RotinaGolpe(multiplicadorVelocidade));
        }
    }

    private IEnumerator RotinaGolpe(float multiplicador)
    {
        atacando = true;
        inimigosAtingidosNesteGolpe.Clear();

        float velAtaqueAtual = velocidadeAtaqueBase * multiplicador;

        while (Mathf.Abs(transform.localEulerAngles.z - (anguloFinal + 360) % 360) > 1f &&
               Mathf.Abs(transform.localEulerAngles.z - anguloFinal) > 1f)
        {
            Quaternion rotacaoAlvo = Quaternion.Euler(0, 0, anguloFinal);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotacaoAlvo, velAtaqueAtual * 100 * Time.deltaTime);

            VerificarCorteEspada();

            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0, 0, anguloFinal);

        yield return new WaitForSeconds(0.05f / multiplicador);

        float velRetornoAtual = velocidadeRetornoBase * multiplicador;
        Quaternion posicaoInicialRot = Quaternion.Euler(0, 0, anguloInicial);

        while (Quaternion.Angle(transform.localRotation, posicaoInicialRot) > 1f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, posicaoInicialRot, velRetornoAtual * 100 * Time.deltaTime);
            yield return null;
        }
        transform.localRotation = posicaoInicialRot;

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
