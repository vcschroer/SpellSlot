using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimPlayer : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Animator animator;

    [Header("Configurações de Transição")]
    [SerializeField] private float dampTimeMovimento = 0.1f;

    private static readonly int HashVelocidade = Animator.StringToHash("Velocidade");
    private static readonly int HashJackpot = Animator.StringToHash("Jackpot");
    private static readonly int HashReceberDano = Animator.StringToHash("ReceberDano");
    private static readonly int HashMorrer = Animator.StringToHash("Morrer");

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"[AnimPlayer] Nenhum componente Animator encontrado em {gameObject.name}.", this);
        }
    }


    public void AtualizarMovimento(float velocidadeAtual, bool suave = true)
    {
        if (animator == null) return;

        if (suave)
        {
            animator.SetFloat(HashVelocidade, velocidadeAtual, dampTimeMovimento, UnityEngine.Time.deltaTime);
        }
        else
        {
            animator.SetFloat(HashVelocidade, velocidadeAtual);
        }
    }


    public void SetarModoJackpot(bool estaAtivo)
    {
        if (animator == null) return;
        animator.SetBool(HashJackpot, estaAtivo);
    }
}