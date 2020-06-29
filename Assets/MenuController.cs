using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    bool loading = false;

    public void ToScene(int i)
    {
        if (loading)
            return;

        SceneManager.LoadSceneAsync(i);
    }
}
