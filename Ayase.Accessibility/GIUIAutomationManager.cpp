#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include "GIUIAutomationManager.h"

IUIAutomation* g_pAutomation;
IUIAutomationCondition* onScreenCondition;
IUIAutomationCondition* isControlCondition;
IUIAutomationCondition* childrenCondition;

double windowArea = 0;

HRESULT InitializeUIAutomation() {
    HRESULT hr;
    hr = CoInitialize(NULL);
    if (!SUCCEEDED(hr)) return hr;
    hr = CoCreateInstance(__uuidof(CUIAutomation), NULL, CLSCTX_INPROC_SERVER,
        __uuidof(IUIAutomation), (void**)&g_pAutomation);
    if (!SUCCEEDED(hr)) return hr;

    VARIANT varIsOffScreen;
    VariantInit(&varIsOffScreen);
    varIsOffScreen.vt = VT_BOOL;
    varIsOffScreen.boolVal = false;
    hr = g_pAutomation->CreatePropertyCondition(UIA_IsOffscreenPropertyId, varIsOffScreen, &onScreenCondition);
    VariantClear(&varIsOffScreen);
    if (!SUCCEEDED(hr)) return hr;
    
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
    return hr;
}


// TODO: Modify
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
    if (element == NULL) return E_FAIL;
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
    if (element == NULL) return E_FAIL;
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


HRESULT GetLeafElements(IUIAutomationElement* element, std::vector<GUIElement*>* leafElements, 
    DOUBLE x, DOUBLE y, DOUBLE w, DOUBLE h, int* stopFlag) {
    if (*stopFlag > 0) return E_FAIL;
    if (element == NULL) return E_FAIL;
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
        if (w * h > 0.9 * windowArea && !wcscmp(resElement->name, L"")) {
            delete resElement;
            return S_OK;
        }
        leafElements->push_back(resElement);
        return S_OK;
    }

    for (int i = 0; i < childrenCount; i++) {
        if (*stopFlag > 0) return E_FAIL;

        IUIAutomationElement* child;
        hr = children->GetElement(i, &child);
        if (!SUCCEEDED(hr)) continue;
        if (child == NULL) continue;

        DOUBLE cx = 0, cy = 0, cw = 0, ch = 0;
        hr = GetElementBounds(child, &cx, &cy, &cw, &ch);
        if (!SUCCEEDED(hr)) continue;

        if (ch == 0 || cw == 0) continue;
        if (ch + cy <= y || cw + cx <= x) continue;
        if (cy >= h + y || cx >= w + x) continue;

        GetLeafElements(child, leafElements, cx, cy, cw, ch, stopFlag);
    }
    return S_OK;
}


HRESULT GetLeafElementsFromWindow(IUIAutomationElement* window, std::vector<GUIElement*>** leafElements, int* elementCount, int* stopFlag) {
    if (window == NULL) return E_FAIL;

    std::vector<GUIElement*>* result = new std::vector<GUIElement*>();
    
    HRESULT hr;
    DOUBLE x = 0, y = 0, w = 0, h = 0;
    hr = GetElementBounds(window, &x, &y, &w, &h);
    if (!SUCCEEDED(hr)) return hr;

    windowArea = w * h;
    hr = GetLeafElements(window, result, x, y, w, h, stopFlag);
    if (*stopFlag > 0) return E_FAIL;
    
    if (!SUCCEEDED(hr)) return hr;

    std::sort(result->begin(), result->end(), Compare);

    *leafElements = result;
    *elementCount = result->size();
    return S_OK;
}


HRESULT DeleteLeafElements(std::vector<GUIElement*>* leafElements) {
    for (GUIElement* element : *leafElements) {
        delete element;
    }
    delete leafElements;
    return S_OK;
}


HRESULT GetGUIElement(std::vector<GUIElement*>* leafElements, int index, GUIElement** element) {
    *element = (*leafElements)[index];
    return S_OK;
}
