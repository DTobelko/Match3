using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;



public class InputManager : MonoBehaviour
{
    public EventHandler<int>  OnChipSelected;


    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                int chipIndex = Int32.Parse(hit.collider.gameObject.name.Substring(4));
                OnChipSelected?.Invoke(this, chipIndex);
            }

        }
    }


}

