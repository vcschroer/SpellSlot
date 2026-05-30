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

    [Header("Configurações de Posição da Espada")]
    [Tooltip("Ajuste fino para definir onde fica o peito/mão do player (o centro real da órbita)")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);

    [Tooltip("Distância que a espada se afasta para os lados (X) ao mover-se na horizontal/diagonal")]
    [SerializeField] private float afastamentoHorizontal = 0.2f;

    [Tooltip("Distância que a espada se afasta para cima/baixo (Y) ao mover-se nas diagonais")]
    [SerializeField] private float afastamentoVertical = 0.15f;

    private Rigidbody2D rb;
    private Vector2 inputsMovimento;
    private bool olhandoParaDireita = true;

    private Vector2 direcaoPosicaoEspada = Vector2.right;

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

        if (inputsMovimento.sqrMagnitude > 0.01f)
        {
            Vector2 direcaoNormalizada = inputsMovimento.normalized;

            if (Mathf.Abs(direcaoNormalizada.x) > 0.3f)
            {
                direcaoPosicaoEspada = direcaoNormalizada;
            }
            else
            {
                float ladoX = olhandoParaDireita ? 1f : -1f;
                direcaoPosicaoEspada = new Vector2(ladoX, direcaoNormalizada.y).normalized;
            }
        }
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
        AtualizarPosicaoDaEspada();

        if (scriptAnimacao != null)
        {
            float velocidadeFisica = inputsMovimento.magnitude;
            scriptAnimacao.AtualizarMovimento(velocidadeFisica);
        }
    }

    private void AtualizarPosicaoDaEspada()
    {
        if (scriptEspada == null) return;

        if (scriptEspada.EstaAtacando) return;

        scriptEspada.transform.localRotation = Quaternion.identity;

        Vector3 novaPosicao = new Vector3(
            centroDoPlayerOffset.x + (direcaoPosicaoEspada.x * afastamentoHorizontal),
            centroDoPlayerOffset.y + (direcaoPosicaoEspada.y * afastamentoVertical),
            0f
        );

        if (!olhandoParaDireita)
        {
            novaPosicao.x = -novaPosicao.x;
        }

        scriptEspada.transform.localPosition = novaPosicao;
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
        if (scriptEspada != null && scriptEspada.EstaAtacando) return;

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
