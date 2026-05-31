using UnityEngine;
using UnityEngine.UI;

public class BarraDeVida : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private Slider slider;
    [SerializeField] private PlayerController player;

    [Header("Configurações RGB (Jackpot)")]
    [Tooltip("Arraste aqui a imagem de preenchimento (Fill) do Slider.")]
    [SerializeField] private Image imagemPreenchimento;
    [SerializeField] private float velocidadeRGB = 2f;

    private Color corOriginalFill = Color.red;

    void Start()
    {
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();

        if (player != null && slider != null)
        {
            slider.maxValue = player.maxDinheiro;
            slider.value = player.dinheiroAtual;
        }

        if (imagemPreenchimento == null && slider != null && slider.fillRect != null)
        {
            imagemPreenchimento = slider.fillRect.GetComponent<Image>();
        }

        if (imagemPreenchimento != null) corOriginalFill = imagemPreenchimento.color;
    }

    void Update()
    {
        if (player == null || slider == null) return;

        slider.value = player.dinheiroAtual;

        if (player.JackpotAtivo)
        {
            if (imagemPreenchimento != null)
            {
                float h = (Time.time * velocidadeRGB) % 1f;
                imagemPreenchimento.color = Color.HSVToRGB(h, 0.75f, 1f);
            }
        }
        else
        {
            if (imagemPreenchimento != null && imagemPreenchimento.color != corOriginalFill)
            {
                imagemPreenchimento.color = corOriginalFill;
            }
        }
    }
}