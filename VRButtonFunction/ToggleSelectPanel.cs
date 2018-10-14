using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ExitUIPanelにアタッチ
public class ToggleSelectPanel : VRButtonController {
	[SerializeField] GameObject panel;
	SelectButtonPanelController selectButton;

	override protected void Awake() 
	{
		base.Awake();

		selectButton = panel.GetComponent<SelectButtonPanelController>();
	}
	
	// override protected void FixedUpdate() 
	// {
	// 	    base.FixeUpdate();	
	// }

	override protected void ButtonFunction()
	{
		if(!SelectButtonPanelController.currentPanel)
		{
			selectButton.Activate();
		}
		else
		{
			if(SelectButtonPanelController.currentPanel == panel)
			{
				selectButton.Deactivate();
			}
			else
			{
                SelectButtonPanelController.currentPanel.GetComponent<SelectButtonPanelController>().Deactivate();
                selectButton.Activate();
			}
		}
	}

}
