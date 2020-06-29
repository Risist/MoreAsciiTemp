using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideSpot : MonoBehaviour
{
    HideComplex complex;

    private void Start()
    {
        complex = GetComponentInParent<HideComplex>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            complex.PlayerEnter();

        var hidable = collision.GetComponentInChildren<HideAble2>();
        if (hidable)
        {

            hidable.SetVisibility(complex.GetDesiredVisibiity());
            hidable.SetVisibilityUi(complex.GetDesiredVisibiityUi());
            hidable.Hide();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        var hidable = collision.GetComponentInChildren<HideAble2>();
        if (hidable)
        {
            hidable.SetVisibility(complex.GetDesiredVisibiity());
            hidable.SetVisibilityUi(complex.GetDesiredVisibiityUi());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            complex.PlayerExit();

        var hidable = collision.GetComponentInChildren<HideAble2>();
        if (hidable)
        {
            hidable.SetVisibility(complex.GetDesiredVisibiity());
            hidable.SetVisibilityUi(complex.GetDesiredVisibiityUi());
            hidable.Show();
        }
    }
}
