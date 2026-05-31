using UnityEngine;

public class UIWobble : MonoBehaviour
{
    [Header("Controle")]
    [Tooltip("Marque ou desmarque no Inspector para ativar/desativar o balanço")]
    [SerializeField] private bool seMexendo = true;

    [Header("Configurações do Balanço")]
    [Tooltip("Velocidade do movimento de vai e vem")]
    [SerializeField] private float velocidade = 4f;

    [Tooltip("Ângulo máximo que ela vai inclinar para cada lado")]
    [SerializeField] private float anguloMaximo = 8f;

    private RectTransform rectTransform;
    private Quaternion rotacaoInicial;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rotacaoInicial = rectTransform.localRotation;
    }

    void Update()
    {
        if (seMexendo)
        {
            float angulo = Mathf.Sin(Time.time * velocidade) * anguloMaximo;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, angulo);
        }
        else
        {
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, rotacaoInicial, Time.deltaTime * 10f);
        }
    }
}