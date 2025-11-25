using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.PE;

namespace WinAPI.Helper
{

    public unsafe class ReflectiveLoaderHelper
    {
        /// <summary>
        /// Fonction principale : Calcule l'offset RVA d'une fonction exportée dans un buffer DLL (via IntPtr).
        /// Gère PE32 (x86) et PE32+ (x64) automatiquement via FileHeader.Machine.
        /// Retourne 0 en cas d'erreur.
        /// </summary>
        /// <param name="dllBufferPtr">IntPtr vers le buffer de la DLL.</param>
        /// <param name="exportName">Nom de la fonction à trouver (e.g., "ReflectiveDllMain").</param>
        /// <returns>Offset RVA de la fonction, ou 0 si non trouvé/invalide.</returns>
        public static unsafe uint GetReflectiveFunctionOffset(IntPtr dllBufferPtr, string exportName = "ReflectiveDllMain")
        {
            if (dllBufferPtr == IntPtr.Zero) return 0;

            IMAGE_DOS_HEADER* dosHeader = (IMAGE_DOS_HEADER*)dllBufferPtr;
            if (dosHeader->e_magic != 0x5A4D) return 0;  // 'MZ'

            IntPtr ntHeadersPtr = dllBufferPtr + dosHeader->e_lfanew;
            uint signature = *(uint*)ntHeadersPtr;
            if (signature != 0x4550) return 0;  // 'PE\0\0'

            // Vérifier l'architecture via Machine
            IMAGE_FILE_HEADER* fileHeader = (IMAGE_FILE_HEADER*)(ntHeadersPtr + 4);  // Offset fixe après Signature
            bool is64Bit = (fileHeader->Machine == 0x8664);  // x64
            bool is32Bit = (fileHeader->Machine == 0x014c);  // x86
            if (!is32Bit && !is64Bit) return 0;  // Architecture non supportée

            // Obtenir Export RVA en fonction de l'architecture
            uint exportRva;
            ushort numberOfSections = fileHeader->NumberOfSections;
            ushort sizeOfOptionalHeader = fileHeader->SizeOfOptionalHeader;
            if (is32Bit)
            {
                IMAGE_NT_HEADERS32* ntHeaders32 = (IMAGE_NT_HEADERS32*)ntHeadersPtr;
                exportRva = ntHeaders32->OptionalHeader.ExportDirectory.VirtualAddress;
                
            }
            else  // x64
            {
                IMAGE_NT_HEADERS64* ntHeaders64 = (IMAGE_NT_HEADERS64*)ntHeadersPtr;
                exportRva = ntHeaders64->OptionalHeader.ExportDirectory.VirtualAddress;
                
            }
            if (exportRva == 0) return 0;


            IMAGE_EXPORT_DIRECTORY* exportDir  = (IMAGE_EXPORT_DIRECTORY*)(dllBufferPtr + (int)RVA2Offset(exportRva, dllBufferPtr));
            uint* functionNameArray = (uint*)(dllBufferPtr + (int)RVA2Offset(exportDir->AddressOfNames, dllBufferPtr));
            uint* functionAddressArray = (uint*)(dllBufferPtr + (int)RVA2Offset(exportDir->AddressOfFunctions, dllBufferPtr));
            ushort* functionOrdinalArray = (ushort*)(dllBufferPtr + (int)RVA2Offset(exportDir->AddressOfNameOrdinals, dllBufferPtr));
            for (uint i = 0; i < exportDir->NumberOfNames; i++)
            {
                uint nameRva = functionNameArray[i];
                uint nameOffset = RVA2Offset(nameRva, dllBufferPtr);
                if (nameOffset == 0) continue;

                IntPtr functionNamePtr = dllBufferPtr + (int)nameOffset;

                // Convertit le nom en string (ASCII null-terminated)
                string functionName = Marshal.PtrToStringAnsi(functionNamePtr);

                if (string.Equals(functionName, exportName, StringComparison.Ordinal))
                {
                    uint funcRva = functionAddressArray[functionOrdinalArray[i]];
                    return RVA2Offset(funcRva, dllBufferPtr);
                }
            }

            return 0;  // Non trouvé
        }

        /// <summary>
        /// Méthode encapsulante : Prend un byte[], le convertit en IntPtr (avec pinning), et appelle GetReflectiveFunctionOffset.
        /// Gère le nettoyage automatiquement.
        /// </summary>
        public static uint GetReflectiveFunctionOffset(byte[] dllBytes, string exportName = "ReflectiveDllMain")
        {
            if (dllBytes == null || dllBytes.Length < 128) return 0;  // Check plus strict pour PE headers

            GCHandle handle = GCHandle.Alloc(dllBytes, GCHandleType.Pinned);
            IntPtr dllBufferPtr = handle.AddrOfPinnedObject();

            try
            {
                return GetReflectiveFunctionOffset(dllBufferPtr, exportName);
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Converts a RVA to a raw file offset in the PE buffer.
        /// Handles both PE32 (x86) and PE32+ (x64) dynamically.
        /// Returns 0 on error (invalid PE or RVA not found).
        /// </summary>
        /// <param name="dwRVA">The RVA to convert.</param>
        /// <param name="pBaseAddress">IntPtr to the base of the PE buffer.</param>
        /// <returns>Raw file offset, or 0 on error.</returns>
        public static unsafe uint RVA2Offset(uint dwRVA, IntPtr pBaseAddress)
        {
            if (pBaseAddress == IntPtr.Zero) return 0;

            IMAGE_DOS_HEADER* dosHeader = (IMAGE_DOS_HEADER*)pBaseAddress;
            if (dosHeader->e_magic != 0x5A4D) return 0;  // 'MZ'

            IntPtr ntHeadersPtr = new IntPtr(pBaseAddress.ToInt64() + dosHeader->e_lfanew);
            uint signature = *(uint*)ntHeadersPtr;
            if (signature != 0x4550) return 0;  // 'PE\0\0'

            // Get FileHeader (common to both 32/64)
            IMAGE_FILE_HEADER* fileHeader = (IMAGE_FILE_HEADER*)(ntHeadersPtr + 4);  // Offset after Signature

            // Determine architecture
            bool is64Bit = (fileHeader->Machine == 0x8664);  // IMAGE_FILE_MACHINE_AMD64
            bool is32Bit = (fileHeader->Machine == 0x014c);  // IMAGE_FILE_MACHINE_I386
            if (!is32Bit && !is64Bit) return 0;  // Unsupported architecture

            // Get SizeOfOptionalHeader (differs between 32/64)
            ushort sizeOfOptionalHeader = fileHeader->SizeOfOptionalHeader;

            // Calculate sections pointer: after FileHeader + SizeOfOptionalHeader
            // FileHeader is at offset 4 (after Signature), size 20 bytes, then OptionalHeader
            long sectionsOffset = 4 + 20 + (long)sizeOfOptionalHeader;  // Signature (4) + FileHeader (20) + OptionalHeader
            IntPtr pImgSectionHdr = new IntPtr(ntHeadersPtr.ToInt64() + sectionsOffset);

            // Get NumberOfSections
            ushort numberOfSections = fileHeader->NumberOfSections;

            // Iterates through the PE sections
            for (int i = 0; i < numberOfSections; i++)
            {
                long currentSectionOffset = (long)(i * sizeof(IMAGE_SECTION_HEADER));  // Assuming sizeof(IMAGE_SECTION_HEADER) == 40
                IntPtr currentSectionPtr = new IntPtr(pImgSectionHdr.ToInt64() + currentSectionOffset);
                IMAGE_SECTION_HEADER* section = (IMAGE_SECTION_HEADER*)currentSectionPtr;

                // If the RVA is located inside the "i" PE section
                if (dwRVA >= section->VirtualAddress && dwRVA < (section->VirtualAddress + section->VirtualSize))
                {
                    // Calculate the delta and add it to the raw pointer
                    return (dwRVA - section->VirtualAddress) + section->PointerToRawData;
                }
            }

            return 0;
        }
    }

}
