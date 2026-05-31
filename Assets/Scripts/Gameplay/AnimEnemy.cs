using UnityEngine;

public class AnimEnemy : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public void AtualizarMovimento(float velocidadeAtual)
    {
        if (animator == null) return;
        animator.SetFloat("Velocidade", velocidadeAtual);
    }

    public void DispararMorte()
    {
        if (animator == null) return;
        animator.SetTrigger("Morrer");
    }
}
