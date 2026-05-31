using System.Collections;
using UnityEngine;

public class SpriteEffects : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Arraste aqui o objeto FILHO que tem o Sprite e o Animator. Se deixar vazio, o script pegará o SpriteRenderer automaticamente.")]
    [SerializeField] private Transform transformVisual;

    [Header("Configuração de Flash (Dano)")]
    [SerializeField] private Material materialFlashBranco;

    [Header("Configurações RGB")]
    [SerializeField] private float velocidadeRGB = 2f;

    private Vector3 escalaOriginal;
    private Color corOriginal;
    private Material materialOriginal;

    private Coroutine coroutineSquash;
    private Coroutine coroutineFlash;
    private bool executandoRGB = false;

    private Vector3 escalaAtualEfeito = Vector3.one;
    private bool aplicandoSquash = false;
    private bool aplicandoFlash = false;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (transformVisual == null)
        {
            if (spriteRenderer != null) transformVisual = spriteRenderer.transform;
            else transformVisual = transform;
        }

        escalaOriginal = transformVisual.localScale;

        if (spriteRenderer != null)
        {
            corOriginal = spriteRenderer.color;
            materialOriginal = spriteRenderer.material;
        }
    }

    void Update()
    {
        if (executandoRGB && spriteRenderer != null && !aplicandoFlash)
        {
            float h = (Time.time * velocidadeRGB) % 1f;
            spriteRenderer.color = Color.HSVToRGB(h, 0.8f, 1f);
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
            tempo += Time.deltaTime;
            escalaAtualEfeito = Vector3.Lerp(escalaOriginal, escalaDeformada, tempo / metadeDuracao);
            yield return null;
        }

        tempo = 0f;
        while (tempo < metadeDuracao)
        {
            tempo += Time.deltaTime;
            escalaAtualEfeito = Vector3.Lerp(escalaDeformada, escalaOriginal, tempo / metadeDuracao);
            yield return null;
        }

        escalaAtualEfeito = escalaOriginal;
        if (transformVisual != null) transformVisual.localScale = escalaOriginal;
        aplicandoSquash = false;
    }
    public void PlayFlash(float duracao)
    {
        if (!gameObject.activeInHierarchy) return;

        if (spriteRenderer == null) return;
        if (coroutineFlash != null) StopCoroutine(coroutineFlash);
        coroutineFlash = StartCoroutine(RotinaFlash(duracao));
    }

    private IEnumerator RotinaFlash(float duracao)
    {
        aplicandoFlash = true;
        yield return new WaitForSeconds(duracao);
        aplicandoFlash = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.material = materialOriginal;
            spriteRenderer.color = corOriginal;
        }
    }

    public void DefinirRGB(bool ligado)
    {
        executandoRGB = ligado;
        if (!ligado && spriteRenderer != null)
        {
            spriteRenderer.material = materialOriginal;
            spriteRenderer.color = corOriginal;
        }
    }
}