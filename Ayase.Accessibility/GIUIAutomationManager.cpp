#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include "GIUIAutomationManager.h"
#include <combaseapi.h>
#include <WinUser.h>
#include <UIAutomation.h>

IUIAutomation* g_pAutomation;

BOOL InitializeUIAutomation() {
    HRESULT hr;
    hr = CoInitialize(NULL);
    if (!SUCCEEDED(hr)) return (SUCCEEDED(hr));
    hr = CoCreateInstance(__uuidof(CUIAutomation), NULL, CLSCTX_INPROC_SERVER,
        __uuidof(IUIAutomation), (void**)&g_pAutomation);
    return (SUCCEEDED(hr));
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
        return S_FALSE; 
    }

    *name = varGet.bstrVal;
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
        return S_FALSE; 
    }
    SAFEARRAY *psa = varGet.parray;
    VariantClear(&varGet);
    if (psa == NULL) return S_FALSE;

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


HRESULT GetUIElement(IUIAutomationElement* element, GUIElement* result) {
    HRESULT hr;
    hr = GetElementName(element, &(result->name));
    if (!SUCCEEDED(hr)) return hr;

    hr = GetElementBounds(element, &(result->x), &(result->y), &(result->w), &(result->h));
    return hr;
}


wchar_t* GetForegroundWindowName() {
    IUIAutomationElement* window;
    HRESULT hr = GetForegroundWindowElement(&window);
    if (!SUCCEEDED(hr)) return NULL;

    BSTR name;
    hr = GetElementName(window, &name);
    if (!SUCCEEDED(hr)) return NULL;
    return name;
}


GUIElement* test() {
    IUIAutomationElement* window;
    HRESULT hr = GetForegroundWindowElement(&window);
    GUIElement *result = new GUIElement();
    hr = GetUIElement(window, result);
    return result;
}
