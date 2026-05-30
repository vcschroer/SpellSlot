using UnityEngine;

public class AnimPlayer : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void AtualizarMovimento(float velocidadeAtual)
    {
        if (animator == null) return;

        animator.SetFloat("Velocidade", velocidadeAtual);
    }

    public void DispararAnimacaoAtaque()
    {
        if (animator == null) return;

        animator.SetTrigger("Atacar");
    }
}
