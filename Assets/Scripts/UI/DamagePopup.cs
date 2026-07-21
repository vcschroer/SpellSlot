using System.Collections;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private TextMeshPro textMesh;

    [Header("Configuracoes da Animacao")]
    [SerializeField] private float velocidadeSubida = 1.2f;
    [SerializeField] private float duracaoPopIn = 0.12f;
    [SerializeField] private float duracaoPopOut = 0.08f;
    [SerializeField] private float duracaoFadeOut = 0.4f;

    private Color corOriginal;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        if (textMesh != null)
            corOriginal = textMesh.color;
    }

    public void Setup(int dano)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = dano.ToString();
        StartCoroutine(RotinaAnimacao());
    }

    private IEnumerator RotinaAnimacao()
    {
        Vector3 escalaFinal = Vector3.one;
        Vector3 escalaPico = Vector3.one * 1.4f; 

        transform.localScale = Vector3.zero;

        float tempo = 0f;
        while (tempo < duracaoPopIn)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoPopIn;
            transform.localScale = Vector3.Lerp(Vector3.zero, escalaPico, t);
            transform.position += Vector3.up * (velocidadeSubida * Time.deltaTime);
            yield return null;
        }

        tempo = 0f;
        while (tempo < duracaoPopOut)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoPopOut;
            transform.localScale = Vector3.Lerp(escalaPico, escalaFinal, t);
            transform.position += Vector3.up * (velocidadeSubida * Time.deltaTime);
            yield return null;
        }

        tempo = 0f;
        Color corAtual = textMesh.color;

        while (tempo < duracaoFadeOut)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoFadeOut;

            transform.position += Vector3.up * (velocidadeSubida * 0.4f * Time.deltaTime);

            corAtual.a = Mathf.Lerp(1f, 0f, t);
            textMesh.color = corAtual;

            yield return null;
        }

        Destroy(gameObject);
    }
}