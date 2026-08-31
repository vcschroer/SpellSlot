using System.Collections;
using UnityEngine;

public class SpriteEffects : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform transformVisual;

    [Header("Configuração de Materiais")]
    [SerializeField] private Material materialPadrao;
    [SerializeField] private Material materialFlashBranco;

    private Vector3 escalaOriginal;
    private Color corOriginal;

    private Coroutine coroutineSquash;
    private Coroutine coroutineFlash;

    private Vector3 escalaAtualEfeito = Vector3.one;
    private bool aplicandoSquash = false;
    private bool aplicandoFlash = false;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (transformVisual == null)
        {
            if (spriteRenderer != null) transformVisual = spriteRenderer.transform;
            else transformVisual = transform;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError($"[SpriteEffects] ERRO: Nenhum SpriteRenderer foi encontrado em '{gameObject.name}' ou nos seus filhos!");
            return;
        }

        escalaOriginal = transformVisual.localScale;
        corOriginal = spriteRenderer.color;

        if (materialPadrao == null)
        {
            materialPadrao = spriteRenderer.material;
        }
    }

    void LateUpdate()
    {
        if (aplicandoSquash && transformVisual != null)
        {
            transformVisual.localScale = escalaAtualEfeito;
        }

        if (aplicandoFlash && spriteRenderer != null)
        {
            if (materialFlashBranco != null)
            {
                spriteRenderer.material = materialFlashBranco;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = Color.red;
            }
        }
    }

    public void PlaySquashAndStretch(float forcaX, float forcaY, float duracao)
    {
        if (!gameObject.activeInHierarchy) return;

        if (coroutineSquash != null) StopCoroutine(coroutineSquash);
        coroutineSquash = StartCoroutine(RotinaSquash(forcaX, forcaY, duracao));
    }

    private IEnumerator RotinaSquash(float forcaX, float forcaY, float duracao)
    {
        aplicandoSquash = true;
        Vector3 escalaDeformada = new Vector3(escalaOriginal.x * forcaX, escalaOriginal.y * forcaY, escalaOriginal.z);
        float metadeDuracao = duracao / 2f;

        float tempo = 0f;
        while (tempo < metadeDuracao)
        {
            tempo += UnityEngine.Time.deltaTime;
            escalaAtualEfeito = Vector3.Lerp(escalaOriginal, escalaDeformada, tempo / metadeDuracao);
            yield return null;
        }

        tempo = 0f;
        while (tempo < metadeDuracao)
        {
            tempo += UnityEngine.Time.deltaTime;
            escalaAtualEfeito = Vector3.Lerp(escalaDeformada, escalaOriginal, tempo / metadeDuracao);
            yield return null;
        }

        escalaAtualEfeito = escalaOriginal;
        if (transformVisual != null) transformVisual.localScale = escalaOriginal;
        aplicandoSquash = false;
    }

    public void PlayFlash(float duracao)
    {
        if (!gameObject.activeInHierarchy || spriteRenderer == null) return;

        if (coroutineFlash != null) StopCoroutine(coroutineFlash);
        coroutineFlash = StartCoroutine(RotinaFlash(duracao));
    }

    private IEnumerator RotinaFlash(float duracao)
    {
        aplicandoFlash = true;
        yield return new WaitForSeconds(duracao);
        aplicandoFlash = false;

        AtualizarMaterial();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = corOriginal;
        }
    }

    private void AtualizarMaterial()
    {
        if (spriteRenderer == null || aplicandoFlash) return;

        if (materialPadrao != null)
        {
            spriteRenderer.material = materialPadrao;
        }
    }
}