#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include "GIUIAutomationManager.h"
#include <combaseapi.h>
#include <comdef.h>
#include <comutil.h>
#include <WinUser.h>
#include <UIAutomation.h>

IUIAutomation* g_pAutomation;
IUIAutomationCondition* onScreenCondition;
IUIAutomationCondition* isControlCondition;
IUIAutomationCondition* childrenCondition;

BOOL InitializeUIAutomation() {
    HRESULT hr;
    hr = CoInitialize(NULL);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);
    hr = CoCreateInstance(__uuidof(CUIAutomation), NULL, CLSCTX_INPROC_SERVER,
        __uuidof(IUIAutomation), (void**)&g_pAutomation);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);

    VARIANT varIsOffScreen;
    VariantInit(&varIsOffScreen);
    varIsOffScreen.vt = VT_BOOL;
    varIsOffScreen.boolVal = false;
    hr = g_pAutomation->CreatePropertyCondition(UIA_IsOffscreenPropertyId, varIsOffScreen, &onScreenCondition);
    VariantClear(&varIsOffScreen);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);
    
    /*
    VARIANT varIsControl;
    VariantInit(&varIsControl);
    varIsControl.vt = VT_BOOL;
    varIsControl.boolVal = true;
    hr = g_pAutomation->CreatePropertyCondition(UIA_IsControlElementPropertyId, varIsControl, &isControlCondition);
    VariantClear(&varIsControl);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);

    hr = g_pAutomation->CreateAndCondition(onScreenCondition, isControlCondition, &childrenCondition);
    */
    return SUCCEEDED(hr);
}


HRESULT GetForegroundWindowElement(IUIAutomationElement** foundWindow) {
    IUIAutomationElement* root;
    HRESULT hr;
    hr = g_pAutomation->GetRootElement(&root);
    if (!SUCCEEDED(hr)) return hr;
    
    HWND hwnd = GetForegroundWindow();
    DWORD processId;
    GetWindowThreadProcessId(hwnd, &processId);

    VARIANT varProcessId;
    VariantInit(&varProcessId);
    varProcessId.vt = VT_I4;
    varProcessId.uintVal = processId;

    IUIAutomationCondition* condition;
    hr = g_pAutomation->CreatePropertyCondition(UIA_ProcessIdPropertyId, varProcessId, &condition);
    VariantClear(&varProcessId);
    if (!SUCCEEDED(hr)) return hr;

    hr = root->FindFirst(TreeScope_Children, condition, foundWindow);
    return hr;
}


HRESULT GetElementName(IUIAutomationElement* element, BSTR* name) {
    VARIANT varGet;
    HRESULT hr;
    hr = element->GetCurrentPropertyValue(UIA_NamePropertyId, &varGet);

    if (!SUCCEEDED(hr)) { 
        VariantClear(&varGet);
        return hr; 
    }
    if (varGet.vt != VT_BSTR) { 
        VariantClear(&varGet);
        return E_FAIL; 
    }
    
    *name = _bstr_t(varGet.bstrVal, false).copy(true);
    VariantClear(&varGet);
    return hr;
}


HRESULT GetElementBounds(IUIAutomationElement* element, 
    DOUBLE* x, DOUBLE* y, DOUBLE* w, DOUBLE* h) {
    VARIANT varGet;
    HRESULT hr;
    hr = element->GetCurrentPropertyValue(UIA_BoundingRectanglePropertyId, &varGet);
    
    if (!SUCCEEDED(hr)) { 
        VariantClear(&varGet);
        return hr; 
    }
    if (varGet.vt != (VT_R8 | VT_ARRAY)) { 
        VariantClear(&varGet); 
        return E_FAIL; 
    }
    SAFEARRAY *psa = varGet.parray;
    VariantClear(&varGet);
    if (psa == NULL) return E_FAIL;

    hr = SafeArrayLock(psa);
    if (!SUCCEEDED(hr)) return hr;

    DOUBLE* pData = static_cast<DOUBLE*>(psa->pvData);
    *x = pData[0];
    *y = pData[1];
    *w = pData[2];
    *h = pData[3];

    SafeArrayUnlock(psa);
    return S_OK;
}


HRESULT GetGUIElement(IUIAutomationElement* element, GUIElement* result) {
    HRESULT hr;
    hr = GetElementName(element, &(result->name));
    if (!SUCCEEDED(hr)) return hr;

    hr = GetElementBounds(element, &(result->x), &(result->y), &(result->w), &(result->h));
    return hr;
}


HRESULT GetLeafElements(IUIAutomationElement* element, std::vector<GUIElement*>* leafElements, 
    DOUBLE x, DOUBLE y, DOUBLE w, DOUBLE h) {
    IUIAutomationElementArray* children;
    HRESULT hr;
    hr = element->FindAll(TreeScope_Children, onScreenCondition, &children);
    if (!SUCCEEDED(hr)) return hr;
    
    int childrenCount = 0;
    hr = children->get_Length(&childrenCount);
    if (!SUCCEEDED(hr)) return hr;

    if (childrenCount == 0) {
        GUIElement* resElement = new GUIElement();
        resElement->x = x;
        resElement->y = y;
        resElement->w = w;
        resElement->h = h;
        hr = GetElementName(element, &(resElement->name));
        if (!SUCCEEDED(hr)) { 
            delete resElement;
            return hr;
        }
        leafElements->push_back(resElement);
        return S_OK;
    }

    for (int i = 0; i < childrenCount; i++) {
        IUIAutomationElement* child;
        hr = children->GetElement(i, &child);
        if (!SUCCEEDED(hr)) continue;

        DOUBLE cx = 0, cy = 0, cw = 0, ch = 0;
        hr = GetElementBounds(child, &cx, &cy, &cw, &ch);
        if (!SUCCEEDED(hr)) continue;

        if (ch == 0 || cw == 0) continue;
        if (ch + cy <= y || cw + cx <= x) continue;
        if (cy >= h + y || cx >= w + x) continue;

        GetLeafElements(child, leafElements, cx, cy, cw, ch);
    }
    return S_OK;
}


BOOL GetLeafElementsFromForegroundWindow(std::vector<GUIElement*>** leafElements, int* elementCount) {
    std::vector<GUIElement*>* result = new std::vector<GUIElement*>();
    IUIAutomationElement* window;
    HRESULT hr;
    hr = GetForegroundWindowElement(&window);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);
    
    DOUBLE x = 0, y = 0, w = 0, h = 0;
    hr = GetElementBounds(window, &x, &y, &w, &h);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);

    hr = GetLeafElements(window, result, x, y, w, h);
    if (!SUCCEEDED(hr)) return SUCCEEDED(hr);

    *leafElements = result;
    *elementCount = result->size();
    return SUCCEEDED(S_OK);
}


BOOL DeleteLeafElements(std::vector<GUIElement*>* leafElements) {
    for (GUIElement* element : *leafElements) {
        delete element;
    }
    delete leafElements;
    return SUCCEEDED(S_OK);
}


BOOL GetGUIElement(std::vector<GUIElement*>* leafElements, int index, GUIElement** element) {
    *element = (*leafElements)[index];
    return SUCCEEDED(S_OK);
}
