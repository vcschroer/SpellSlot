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
    [SerializeField] private int quantidadeSegmentosMeio = 3;
    [SerializeField] private float tamanhoDoSegmentoY = 0.5f;

    private bool atacando = false;
    private List<GameObject> segmentosCriados = new List<GameObject>();
    private List<Collider2D> colisoresLamina = new List<Collider2D>();
    private int quantidadeAnterior; 

    public bool EstaAtacando => atacando;

    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, anguloInicial);
        quantidadeAnterior = quantidadeSegmentosMeio;
        ConstruirEspada();
    }

    private void OnValidate()
    {
        if (Application.isPlaying && quantidadeSegmentosMeio != quantidadeAnterior)
        {
            if (quantidadeSegmentosMeio < 0) quantidadeSegmentosMeio = 0;
            quantidadeAnterior = quantidadeSegmentosMeio;
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
        colisoresLamina.Clear();

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
            ConfigurarColisor(meio);
            segmentosCriados.Add(meio);
            posicaoYAtual += tamanhoDoSegmentoY;
        }

        if (prefabPontaLamina != null)
        {
            GameObject ponta = Instantiate(prefabPontaLamina, transform);
            ponta.transform.localPosition = new Vector3(0, posicaoYAtual, 0);
            ConfigurarColisor(ponta);
            segmentosCriados.Add(ponta);
        }

        AlternarColisores(false);
    }

    private void ConfigurarColisor(GameObject gomo)
    {
        BoxCollider2D colisor = gomo.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;

        SwordCheck detector = gomo.AddComponent<SwordCheck>();
        detector.Configurar(this);

        colisoresLamina.Add(colisor);
    }

    private void AlternarColisores(bool estado)
    {
        foreach (Collider2D colisor in colisoresLamina)
        {
            if (colisor != null) colisor.enabled = estado;
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
        AlternarColisores(true);

        float velAtaqueAtual = velocidadeAtaqueBase * multiplicador;

        while (Mathf.Abs(transform.localEulerAngles.z - (anguloFinal + 360) % 360) > 1f &&
               Mathf.Abs(transform.localEulerAngles.z - anguloFinal) > 1f)
        {
            Quaternion rotacaoAlvo = Quaternion.Euler(0, 0, anguloFinal);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotacaoAlvo, velAtaqueAtual * 100 * Time.deltaTime);
            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0, 0, anguloFinal);

        AlternarColisores(false);

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
}
