using UnityEngine;
using UnityEngine.UI;

public class SlotMachineVisual : MonoBehaviour
{
    [Header("Componentes de Imagem da UI")]
    [SerializeField] private Image exibicaoSlot1;
    [SerializeField] private Image exibicaoSlot2;
    [SerializeField] private Image exibicaoSlot3;

    [Header("Sprites das Recompensas")]
    [SerializeField] private Sprite spriteDinheiro;
    [SerializeField] private Sprite spriteTamanhoEspada;
    [SerializeField] private Sprite spriteVelocidadeAtaque;
    [SerializeField] private Sprite spriteVelocidadePlayer;
    [SerializeField] private Sprite spriteVazio; 

    public void AtualizarVisualDosSlots(SlotMachine.TipoRecompensa s1, SlotMachine.TipoRecompensa s2, SlotMachine.TipoRecompensa s3)
    {
        if (exibicaoSlot1 != null) exibicaoSlot1.sprite = RetornarSpriteCorrespondente(s1);
        if (exibicaoSlot2 != null) exibicaoSlot2.sprite = RetornarSpriteCorrespondente(s2);
        if (exibicaoSlot3 != null) exibicaoSlot3.sprite = RetornarSpriteCorrespondente(s3);
    }

    private Sprite RetornarSpriteCorrespondente(SlotMachine.TipoRecompensa tipo)
    {
        return tipo switch
        {
            SlotMachine.TipoRecompensa.Dinheiro => spriteDinheiro,
            SlotMachine.TipoRecompensa.TamanhoEspada => spriteTamanhoEspada,
            SlotMachine.TipoRecompensa.VelocidadeAtaque => spriteVelocidadeAtaque,
            SlotMachine.TipoRecompensa.VelocidadePlayer => spriteVelocidadePlayer,
            SlotMachine.TipoRecompensa.Vazio => spriteVazio,
            _ => null
        };
    }
}
