using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackpotEffect : MonoBehaviour
{
    [Header("Referências Necessárias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Sword espadaOriginal;

    [Header("Configurações do Efeito")]
    [SerializeField] private float duracaoDoJackpot = 5f;
    [SerializeField] private float velocidadeDeRotacaoEspada = 360f;

    [Header("Configurações de Órbita (Devem bater com o PlayerController)")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);
    [SerializeField] private float raioDaOrbita = 1.2f;

    [Header("Prefabs da Lâmina do Jackpot")]
    [SerializeField] private GameObject prefabCaboOverride;
    [SerializeField] private GameObject prefabMeioOverride;
    [SerializeField] private GameObject prefabPontaOverride;

    private GameObject containerEspadaJackpot;
    private List<GameObject> segmentosCriados = new List<GameObject>();
    private bool jackpotAtivo = false;
    private float anguloRotacaoAtual = 0f;

    void Start()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (espadaOriginal == null) espadaOriginal = GetComponentInChildren<Sword>();
    }

    void Update()
    {
        if (jackpotAtivo && containerEspadaJackpot != null)
        {
            CalcularGiroContinuo();
        }
    }

    public void AtivarModoJackpot()
    {
        if (jackpotAtivo) return;

        StartCoroutine(RotinaEfeitoJackpot());
    }

    private IEnumerator RotinaEfeitoJackpot()
    {
        jackpotAtivo = true;

        float velocidadeAtual = ObterVelocidadeAtualPlayer();
        playerController.AumentarVelocidade(velocidadeAtual);

        if (espadaOriginal != null)
        {
            espadaOriginal.gameObject.SetActive(false);
        }

        ConstruirEspadaJackpot();

        yield return new WaitForSeconds(duracaoDoJackpot);

        if (containerEspadaJackpot != null)
        {
            Destroy(containerEspadaJackpot);
        }
        segmentosCriados.Clear();

        playerController.AumentarVelocidade(-velocidadeAtual);

        if (espadaOriginal != null)
        {
            espadaOriginal.gameObject.SetActive(true);
        }

        jackpotAtivo = false;
    }

    private void CalcularGiroContinuo()
    {
        anguloRotacaoAtual += velocidadeDeRotacaoEspada * Time.deltaTime;

        anguloRotacaoAtual %= 360f;

        float radianos = anguloRotacaoAtual * Mathf.Deg2Rad;
        Vector2 direcao = new Vector2(Mathf.Cos(radianos), Mathf.Sin(radianos));

        Vector3 deslocamentoOrbitaLocal = new Vector3(direcao.x, direcao.y, 0f) * raioDaOrbita;
        containerEspadaJackpot.transform.localPosition = new Vector3(centroDoPlayerOffset.x, centroDoPlayerOffset.y, 0f) + deslocamentoOrbitaLocal;

        containerEspadaJackpot.transform.localRotation = Quaternion.Euler(0, 0, anguloRotacaoAtual - 90f);
    }

    private void ConstruirEspadaJackpot()
    {
        containerEspadaJackpot = new GameObject("Espada_Jackpot_Turbo");
        containerEspadaJackpot.transform.SetParent(this.transform);
        containerEspadaJackpot.transform.localPosition = Vector3.zero;
        containerEspadaJackpot.transform.localRotation = Quaternion.identity;

        int gomosParaCriar = (espadaOriginal != null) ? espadaOriginal.quantidadeSegmentosMeio : 3;

        GameObject cabo = prefabCaboOverride != null ? prefabCaboOverride : ObterPrefabOriginal(0);
        GameObject meio = prefabMeioOverride != null ? prefabMeioOverride : ObterPrefabOriginal(1);
        GameObject ponta = prefabPontaOverride != null ? prefabPontaOverride : ObterPrefabOriginal(2);

        float posicaoYAtual = 0f;
        float tamanhoGomoY = 0.5f;

        if (cabo != null)
        {
            GameObject gomoCabo = Instantiate(cabo, containerEspadaJackpot.transform);
            gomoCabo.transform.localPosition = new Vector3(0, posicaoYAtual, 0);
            segmentosCriados.Add(gomoCabo);
            posicaoYAtual += tamanhoGomoY;
        }

        for (int i = 0; i < gomosParaCriar; i++)
        {
            if (meio != null)
            {
                GameObject gomoMeio = Instantiate(meio, containerEspadaJackpot.transform);
                gomoMeio.transform.localPosition = new Vector3(0, posicaoYAtual, 0);
                segmentosCriados.Add(gomoMeio);
                posicaoYAtual += tamanhoGomoY;
            }
        }

        if (ponta != null)
        {
            GameObject gomoPonta = Instantiate(ponta, containerEspadaJackpot.transform);
            float baseDaPontaY = posicaoYAtual - tamanhoGomoY;
            gomoPonta.transform.localPosition = new Vector3(0, baseDaPontaY + 0.5f, 0);
            segmentosCriados.Add(gomoPonta);
        }

    }

    private GameObject ObterPrefabOriginal(int tipo)
    {
        if (espadaOriginal == null) return null;

        string nomeCampo = tipo == 0 ? "prefabCabo" : (tipo == 1 ? "prefabMeioLamina" : "prefabPontaLamina");
        var campo = typeof(Sword).GetField(nomeCampo, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return campo != null ? (GameObject)campo.GetValue(espadaOriginal) : null;
    }

    private float ObterVelocidadeAtualPlayer()
    {
        var campo = typeof(PlayerController).GetField("velocidade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return campo != null ? (float)campo.GetValue(playerController) : 5f;
    }
}