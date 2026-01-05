using System;
using UnityEngine;

public class MoveUpWards : MonoBehaviour
{
    private void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime);
    }
}
