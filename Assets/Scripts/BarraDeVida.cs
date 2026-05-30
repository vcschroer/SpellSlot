using UnityEngine;
using UnityEngine.UI; // necessário para Slider/Image

public class BarraDeVida : MonoBehaviour
{
    public PlayerController playerController; // arraste o Player aqui no Inspector
    public Slider sliderVida;                 // arraste o Slider da UI aqui

    void Update()   
    {
        sliderVida.value = playerController.dinheiroAtual;
    }
}