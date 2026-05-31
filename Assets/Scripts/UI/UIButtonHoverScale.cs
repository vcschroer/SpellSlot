using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configurações de Escala")]
    [Tooltip("Multiplicador do tamanho quando o mouse está em cima (ex: 1.15 significa que cresce 15%)")]
    [SerializeField] private float escalaHover = 1.12f;

    [Tooltip("Tempo em segundos para a transição de tamanho terminar")]
    [SerializeField] private float duracaoTransicao = 0.1f;

    private Vector3 escalaOriginal;
    private Coroutine coroutineEscala;

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MudarEscala(escalaOriginal * escalaHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MudarEscala(escalaOriginal);
    }

    private void MudarEscala(Vector3 escalaAlvo)
    {
        if (coroutineEscala != null) StopCoroutine(coroutineEscala);
        coroutineEscala = StartCoroutine(RotinaMudarEscala(escalaAlvo));
    }

    private IEnumerator RotinaMudarEscala(Vector3 escalaAlvo)
    {
        Vector3 escalaInicial = transform.localScale;
        float tempo = 0f;

        while (tempo < duracaoTransicao)
        {
            tempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaAlvo, tempo / duracaoTransicao);
            yield return null;
        }

        transform.localScale = escalaAlvo;
    }

    void OnDisable()
    {
        if (coroutineEscala != null) StopCoroutine(coroutineEscala);
        transform.localScale = escalaOriginal;
    }
}