/// Credit mgear, SimonDarksideJ
/// Sourced from - https://forum.unity3d.com/threads/radial-slider-circle-slider.326392/#post-3143582
/// Updated to include lerping features and programmatic access to angle/value

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RadialSlider : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float ringWidth = 35;
    public float radius = 200;
    float radio;
    public RectTransform handle;
    private bool isPointerDown, isPointerReleased, isPointer;
    private Vector2 m_localPos, m_screenPos;
    private Camera m_eventCamera;
    RectTransform rectTransform;
    bool isClickRing;
    float maxDis, minDis, handleLength;
    public float normalizedValue;   

    [Tooltip("Event fired when value of control changes, outputs an INT angle value")]
    [SerializeField]
    private RadialSliderValueChangedEvent _onValueChanged = new RadialSliderValueChangedEvent();

    [Serializable]
    public class RadialSliderValueChangedEvent : UnityEvent<float> { }
    [Serializable]
    public class RadialSliderTextValueChangedEvent : UnityEvent<string> { }

    public RadialSliderValueChangedEvent onValueChanged
    {
        get { return _onValueChanged; }
        set { _onValueChanged = value; }
    }
    void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = transform.GetComponent<RectTransform>();
        radio = rectTransform.rect.width / 400;
        maxDis = radius * radio;
        minDis = (radius - ringWidth) * radio;
        handleLength = maxDis - ringWidth / 2;
    }

    private void Update()
    {
        if (isPointerDown)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, m_screenPos, m_eventCamera, out m_localPos);
            if(IsClickRing(m_localPos))
                isClickRing = true;
        }

        if(isClickRing&& isPointer)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, m_screenPos, m_eventCamera, out m_localPos);
            normalizedValue = GetAngleFromMousePoint();
            handle.anchoredPosition = m_localPos.normalized * handleLength;
            NotifyValueChanged();
        }

        if (isClickRing && isPointerReleased)
        {
            isClickRing = false;
        }
    }
    

    bool IsClickRing(Vector2 pos)
    {
        float dis = pos.magnitude;
        //print($"dis: {dis}   m_localPos:  {m_localPos}  maxDis: {maxDis}  minDis:  {minDis}");
        return dis < maxDis && dis > minDis;
    }

    private float GetAngleFromMousePoint()
    {
        float angle = (Mathf.Atan2(-m_localPos.y, m_localPos.x) * 180f / Mathf.PI + 90f);
        if (angle < 0)
            angle += 360;
        return angle / 360f;
    }

    private void NotifyValueChanged()
    {
        _onValueChanged.Invoke(normalizedValue);
    }

    //#if UNITY_EDITOR

    //        private void OnValidate()
    //        {
    //            if (LerpToTarget && LerpCurve.length < 2)
    //            {
    //                LerpToTarget = false;
    //                Debug.LogError("You need to define a Lerp Curve to enable 'Lerp To Target'");
    //            }
    //        }
    //#endif

    #region Interfaces
    // Called when the pointer enters our GUI component.
    // Start tracking the mouse
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_screenPos = eventData.position;
        m_eventCamera = eventData.enterEventCamera;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_screenPos = eventData.position;
        m_eventCamera = eventData.enterEventCamera;
        isPointerDown = true;
        isPointer = true;
        StartCoroutine(WaitForResetPointDown());
    }

    IEnumerator WaitForResetPointDown()
    {
        yield return new WaitForFixedUpdate();
        isPointerDown = false;
    }

    IEnumerator WaitForResetPointUp()
    {
        yield return new WaitForFixedUpdate();
        isPointerReleased = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_screenPos = Vector2.zero;
        isPointer = false;
        isPointerReleased = true;
        StartCoroutine(WaitForResetPointUp());

    }

    public void OnDrag(PointerEventData eventData)
    {
        m_screenPos = eventData.position;
    }
    #endregion
}
