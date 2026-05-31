using System.Collections;
using UnityEngine;

public class SlotMachineLever : MonoBehaviour
{
    [Header("Configurações de Rotação")]
    [Tooltip("Ângulo inicial da alavanca (normalmente 0)")]
    [SerializeField] private float anguloInicial = 0f;

    [Tooltip("Ângulo máximo para baixo. Se ela girar para o lado errado, mude o sinal do número (ex: se -60 foi para trás, use 60)")]
    [SerializeField] private float anguloMaximo = -60f;

    [Header("Tempos da Animação")]
    [SerializeField] private float duracaoPuxada = 0.15f;
    [SerializeField] private float duracaoRetorno = 0.25f;

    [Header("Efeito Elástico (Mola)")]
    [SerializeField] private bool usarEfeitoMola = true;
    [SerializeField] private float forcaDoRebote = 8f;

    private RectTransform rectTransform;
    private Coroutine coroutineAlavanca;
    private bool estaAnimando = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        AjustarPivotSemMover(new Vector2(0f, 0f));
    }

    public void PuxarAlavanca()
    {
        if (estaAnimando) return;

        if (coroutineAlavanca != null) StopCoroutine(coroutineAlavanca);
        coroutineAlavanca = StartCoroutine(RotinaPuxar());
    }

    private IEnumerator RotinaPuxar()
    {
        estaAnimando = true;

        float tempo = 0f;
        while (tempo < duracaoPuxada)
        {
            tempo += Time.deltaTime;
            float anguloAtual = Mathf.Lerp(anguloInicial, anguloMaximo, tempo / duracaoPuxada);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloAtual);
            yield return null;
        }
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloMaximo);

        yield return new WaitForSeconds(0.05f);

        tempo = 0f;
        while (tempo < duracaoRetorno)
        {
            tempo += Time.deltaTime;
            float anguloAtual = Mathf.Lerp(anguloMaximo, anguloInicial, tempo / duracaoRetorno);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloAtual);
            yield return null;
        }

        if (usarEfeitoMola)
        {
            tempo = 0f;
            float duracaoRebote = 0.08f;

            float direcaoRebote = anguloMaximo < 0 ? forcaDoRebote : -forcaDoRebote;

            while (tempo < duracaoRebote)
            {
                tempo += Time.deltaTime;
                float anguloAtual = Mathf.Lerp(anguloInicial, direcaoRebote, tempo / duracaoRebote);
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloAtual);
                yield return null;
            }
            tempo = 0f;
            while (tempo < duracaoRebote)
            {
                tempo += Time.deltaTime;
                float anguloAtual = Mathf.Lerp(direcaoRebote, anguloInicial, tempo / duracaoRebote);
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloAtual);
                yield return null;
            }
        }

        rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloInicial);
        estaAnimando = false;
    }

    private void AjustarPivotSemMover(Vector2 novoPivot)
    {
        Vector2 pivotAntigo = rectTransform.pivot;
        Vector2 diferenca = novoPivot - pivotAntigo;

        Vector3 deslocamento = new Vector3(diferenca.x * rectTransform.rect.width, diferenca.y * rectTransform.rect.height, 0f);

        rectTransform.pivot = novoPivot;
        rectTransform.localPosition += rectTransform.localRotation * deslocamento;
    }
}
