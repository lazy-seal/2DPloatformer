using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeChangeUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Slider slider;

    public void FixedUpdate()
    {
        slider.value = playerMovement.GetMoveTime();
    }

    public void Update()
    {
        Vector3 localScale = transform.localScale;
        if (playerMovement.GetIsFacingRight() != localScale.x > 0)
        {
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }


}
