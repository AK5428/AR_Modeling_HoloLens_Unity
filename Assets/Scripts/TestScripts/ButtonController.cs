using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header ("GameObjects")]
    [SerializeField] private GameObject AirPainterObj;
    [SerializeField] private GameObject ModelObjs;

    [Header("Buttons")]
    [SerializeField] private GameObject PaintBtn;
    [SerializeField] private GameObject ModelBtn;

    [Header("Other parameters")]
    [SerializeField] private float modelDistance = 1.5f;

    private bool modelFirstAwake = true;

    private void Start()
    {
        StartCoroutine(WaitAndSetDeactive(AirPainterObj, 1.1f));
        ModelObjs.SetActive(false);
    }

    public void OnPaintBtnPressed()
    {
        bool isToggled = PaintBtn.GetComponent<Interactable>().IsToggled;

        if(isToggled) 
        {
            AirPainterObj.SetActive(true);
            ModelObjs.SetActive(false);
        }
        else AirPainterObj.SetActive(false);

        ModelBtn.GetComponent<Interactable>().IsToggled = false;
    }

    public void OnModelBtnPressed()
    {
        bool isToggled = ModelBtn.GetComponent<Interactable>().IsToggled;

        if(isToggled) 
        {
            ModelObjs.SetActive(true);
            AirPainterObj.SetActive(false);
        }
        else ModelObjs.SetActive(false);

        PaintBtn.GetComponent<Interactable>().IsToggled = false;

        if(modelFirstAwake)
        {
            modelFirstAwake = false;
            UpdateModelPos();
        }
    }

    private void UpdateModelPos()
    {
        Vector3 userPos = Camera.main.transform.position;
        Vector3 userForward = Camera.main.transform.forward;

        Vector3 modelPos = userPos + userForward * modelDistance;

        ModelObjs.transform.position = modelPos;
    }

    private IEnumerator WaitAndSetDeactive(GameObject obj, float time = 0.3f)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
