#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include "GIUIAutomationManager.h"
#include <combaseapi.h>
#include <WinUser.h>
#include <UIAutomation.h>
#include <iostream>

IUIAutomation* g_pAutomation;

BOOL InitializeUIAutomation() {
    HRESULT hr = CoInitialize(NULL);
    if (!SUCCEEDED(hr)) return (SUCCEEDED(hr));
    hr = CoCreateInstance(__uuidof(CUIAutomation), NULL, CLSCTX_INPROC_SERVER,
        __uuidof(IUIAutomation), (void**)&g_pAutomation);
    return (SUCCEEDED(hr));
}


HRESULT GetForegroundWindowElement(IUIAutomationElement** foundWindow) {
    IUIAutomationElement* root;
    HRESULT hr = g_pAutomation->GetRootElement(&root);
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
    HRESULT hr = element->GetCurrentPropertyValue(UIA_NamePropertyId, &varGet);
    *name = varGet.bstrVal;
    VariantClear(&varGet);
    return hr;
}


HRESULT GetElementBounds(IUIAutomationElement* element) {

}


wchar_t* GetForeGroundWindowName() {
    IUIAutomationElement* window;
    GetForegroundWindowElement(&window);
    BSTR name;
    GetElementName(window, &name);
    return name;
}
