using UnityEngine;

public class TesteSC2 : MonoBehaviour
{
    int SlotA = 1;
    int SlotB = 1;

    int V1 = 0;


    private void Awake()
    {
        switch (SlotA)
        {
            case 1:
                V1 = +1;
                break;

            case 2:

                break;
        }

        switch (SlotB)
        {
            case 1:
                V1 = +1;
                break;

            case 2:

                break;
        }

    }

}
