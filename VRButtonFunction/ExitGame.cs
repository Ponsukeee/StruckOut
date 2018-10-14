using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ExitUIPanelにアタッチ
public class ExitGame : VRButtonController
{

    // override protected void Awake() 
    // {
    // 		base.Awake();
    // }

    // override protected void FixedUpdate () 
    // {
    // 	    base.FixeUpdate();	
    // }

    override protected void ButtonFunction()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
		Application.Quit();
        #endif
    }

}