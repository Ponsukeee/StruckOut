using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//RedoUIPanelにアタッチ
public class RedoGame : VRButtonController
{

    // override protected void Awake() 
    // {
    // 		base.Awake();
    // }

    // override protected void FixedUpdate () 
    // {
    // 		base.FixeUpdate();	
    // }

    override protected void ButtonFunction()
    {
        SceneManager.LoadScene("StruckOut_ver3");
    }
}
