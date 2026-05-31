using UnityEngine;
using UnityEngine.UI;
public class BarraDeVida : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private Slider slider; 
    [SerializeField] private PlayerController player; 

    void Start()
    {
        if (player == null)
        {
            player = Object.FindAnyObjectByType<PlayerController>();
        }

        if (player != null && slider != null)
        {
            slider.maxValue = player.maxDinheiro;
            slider.value = player.dinheiroAtual;
        }
    }

    void Update()
    {
        if (player != null && slider != null)
        {
            slider.value = player.dinheiroAtual;
        }
    }
}
