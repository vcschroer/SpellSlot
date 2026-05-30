using System.Collections; 
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private Sword scriptEspada;

    [Header("Scripts Auxiliares")]
    [SerializeField] private AnimPlayer scriptAnimacao; 

    [Header("Sistema de Economia (Vida)")]
    public int maxDinheiro = 100;
    [SerializeField] private int dinheiroAtual = 100;

    [Header("Taxa de Sobrevivência")]
    [SerializeField] private int custoPorTempo = 5;
    [SerializeField] private float intervaloTempo = 5f;

    [Header("Atributos de Combate")]
    public float attackSpeed = 1f;

    private Rigidbody2D rb;
    private Vector2 inputsMovimento;
    private bool olhandoParaDireita = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (scriptEspada == null)
        {
            scriptEspada = GetComponentInChildren<Sword>();
        }

        if (scriptAnimacao == null)
        {
            scriptAnimacao = GetComponent<AnimPlayer>();
        }

        dinheiroAtual = Mathf.Clamp(dinheiroAtual, 0, maxDinheiro);
        StartCoroutine(RotinaPerdaDeDinheiro());
    }

    public void OnMove(InputValue value)
    {
        inputsMovimento = value.Get<Vector2>();
    }

    public void OnAttack()
    {
        if (scriptEspada != null)
        {
            if (!scriptEspada.EstaAtacando)
            {
                scriptEspada.Atacar(attackSpeed);

                if (scriptAnimacao != null)
                {
                    scriptAnimacao.DispararAnimacaoAtaque();
                }
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + inputsMovimento * velocidade * Time.fixedDeltaTime);
        VerificarFlip();

        if (scriptAnimacao != null)
        {
            float velocidadeFisica = inputsMovimento.magnitude;
            scriptAnimacao.AtualizarMovimento(velocidadeFisica);
        }
    }

    private IEnumerator RotinaPerdaDeDinheiro()
    {
        while (dinheiroAtual > 0)
        {
            yield return new WaitForSeconds(intervaloTempo);
            PerderDinheiro(custoPorTempo);
        }
    }

    public void PerderDinheiro(int quantity)
    {
        dinheiroAtual -= quantity;
        if (dinheiroAtual < 0) dinheiroAtual = 0;

        if (CameraShake.Instancia != null)
        {
            CameraShake.Instancia.Tremer(0.2f, 0.4f);
        }

        if (dinheiroAtual <= 0)
        {
            Gamover();
        }
    }

    public void GanharDinheiro(int quantity)
    {
        dinheiroAtual += quantity;
        if (dinheiroAtual > maxDinheiro) dinheiroAtual = maxDinheiro;
    }

    private void Gamover()
    {
        gameObject.SetActive(false);
    }

    private void VerificarFlip()
    {
        if (inputsMovimento.x < 0 && olhandoParaDireita) Flipar();
        else if (inputsMovimento.x > 0 && !olhandoParaDireita) Flipar();
    }

    private void Flipar()
    {
        olhandoParaDireita = !olhandoParaDireita;
        Vector3 rotacaoAtual = transform.eulerAngles;
        rotacaoAtual.y = olhandoParaDireita ? 0f : 180f;
        transform.eulerAngles = rotacaoAtual;
    }
}
