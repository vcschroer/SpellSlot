using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private Sword scriptEspada;

    [Header("Scripts Auxiliares")]
    [SerializeField] private AnimPlayer scriptAnimacao;

    private SpriteRenderer spritePlayer;

    [Header("Sistema de Economia")]
    [SerializeField] public int maxDinheiro = 100;
    [SerializeField] public int dinheiroAtual = 100;

    public int vidaAtual => dinheiroAtual;

    [Header("Taxa de Sobrevivência")]
    [SerializeField] private int custoPorTempo = 5;
    [SerializeField] private float intervaloTempo = 5f;

    [Header("Atributos de Combate")]
    public float attackSpeed = 1f;

    [Header("Configurações de Órbita Livre da Espada")]
    [SerializeField] private Vector2 centroDoPlayerOffset = new Vector2(0f, 0.2f);
    [SerializeField] private float raioDaOrbita = 1.2f;

    private Rigidbody2D rb;
    private Vector2 inputsMovimento;
    private bool olhandoParaDireita = true;
    private float anguloMiraMouse;

    private bool derrotaDisparada = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        spritePlayer = GetComponent<SpriteRenderer>();
        if (spritePlayer == null)
        {
            spritePlayer = GetComponentInChildren<SpriteRenderer>();
        }

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
        if (derrotaDisparada) return;
        inputsMovimento = value.Get<Vector2>();
    }

    public void OnAttack()
    {
        if (derrotaDisparada) return;

        if (scriptEspada != null)
        {
            if (!scriptEspada.EstaAtacando)
            {
                scriptEspada.Atacar(attackSpeed, anguloMiraMouse);

                if (scriptAnimacao != null)
                {
                    scriptAnimacao.DispararAnimacaoAtaque();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (derrotaDisparada)
        {
            inputsMovimento = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + inputsMovimento * velocidade * Time.fixedDeltaTime);

        VerificarFlip();
        AtualizarPosicaoDaEspada();

        if (scriptAnimacao != null)
        {
            float velocidadFisica = inputsMovimento.magnitude;
            scriptAnimacao.AtualizarMovimento(velocidadFisica);

            if (scriptEspada != null)
            {
                scriptAnimacao.SetarModoJackpot(scriptEspada.EstaEmModoJackpot);
            }
        }
    }

    private void AtualizarPosicaoDaEspada()
    {
        if (scriptEspada == null) return;
        if (scriptEspada.EstaAtacando || scriptEspada.EstaEmModoJackpot) return;
        if (Camera.main == null || Mouse.current == null) return;

        Vector2 posicaoMouseTela = Mouse.current.position.ReadValue();
        Vector3 posicaoMouseMundo = Camera.main.ScreenToWorldPoint(posicaoMouseTela);
        posicaoMouseMundo.z = 0f;

        Vector3 centroRealPlayer = transform.position + new Vector3(centroDoPlayerOffset.x, centroDoPlayerOffset.y, 0f);
        Vector2 irreversibleDirecao = (posicaoMouseMundo - centroRealPlayer).normalized;

        anguloMiraMouse = Mathf.Atan2(irreversibleDirecao.y, irreversibleDirecao.x) * Mathf.Rad2Deg;
        Vector3 deslocamentoOrbitaLocal = new Vector3(irreversibleDirecao.x, irreversibleDirecao.y, 0f) * raioDaOrbita;

        scriptEspada.transform.localPosition = new Vector3(centroDoPlayerOffset.x, centroDoPlayerOffset.y, 0f) + deslocamentoOrbitaLocal;

        if (anguloMiraMouse > 90f || anguloMiraMouse < -90f)
        {
            float anguloEsquerda = anguloMiraMouse + 90f - scriptEspada.AnguloInicial;
            scriptEspada.transform.localRotation = Quaternion.Euler(0, 0, anguloEsquerda);
        }
        else
        {
            float anguloDireita = anguloMiraMouse + 90f + scriptEspada.AnguloInicial;
            scriptEspada.transform.localRotation = Quaternion.Euler(0, 0, anguloDireita);
        }

        Vector3 escalaEspada = scriptEspada.transform.localScale;
        escalaEspada.y = Mathf.Abs(escalaEspada.y);
        scriptEspada.transform.localScale = escalaEspada;
    }

    private IEnumerator RotinaPerdaDeDinheiro()
    {
        while (dinheiroAtual > 0 && !derrotaDisparada)
        {
            yield return new WaitForSeconds(intervaloTempo);
            PerderDinheiro(custoPorTempo);
        }
    }

    public void PerderDinheiro(int quantity)
    {
        if (derrotaDisparada) return;

        dinheiroAtual -= quantity;
        if (dinheiroAtual < 0) dinheiroAtual = 0;

        if (dinheiroAtual <= 0)
        {
            Gamover();
        }
    }

    public void TomarDano(int dano)
    {
        if (scriptEspada != null && scriptEspada.EstaEmModoJackpot)
        {
            return;
        }

        PerderDinheiro(dano);
    }

    public void GanharDinheiro(int quantity)
    {
        if (derrotaDisparada) return;
        dinheiroAtual += quantity;
        if (dinheiroAtual > maxDinheiro) dinheiroAtual = maxDinheiro;
    }

    private void Gamover()
    {
        if (derrotaDisparada) return;
        derrotaDisparada = true;

        if (scriptAnimacao != null)
        {
            scriptAnimacao.AtualizarMovimento(0f);
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.CarregarCena("Defeat");
        }
        else
        {
            Debug.LogWarning("[AVISO]: Prefab do TransitionManager não encontrado! Carregando derrota sem efeito.");
            SceneManager.LoadScene("Defeat");
        }
    }

    private void VerificarFlip()
    {
        if (inputsMovimento.x < 0 && olhandoParaDireita) Flipar();
        else if (inputsMovimento.x > 0 && !olhandoParaDireita) Flipar();
    }

    private void Flipar()
    {
        olhandoParaDireita = !olhandoParaDireita;

        if (spritePlayer != null)
        {
            spritePlayer.flipX = !olhandoParaDireita;
        }
        else
        {
            Vector3 escalaLocal = transform.localScale;
            escalaLocal.x = olhandoParaDireita ? Mathf.Abs(escalaLocal.x) : -Mathf.Abs(escalaLocal.x);
        }
    }

    public void AumentarVelocidade(float quantidade)
    {
        velocidade += quantidade;
    }
}