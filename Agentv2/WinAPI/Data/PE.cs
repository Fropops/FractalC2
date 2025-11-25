using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Runtime.InteropServices;

namespace WinAPI.Data.PE
{
    // Structures PE standard (adaptées de WinAPI pour 32/64 bits)
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_DOS_HEADER
    {
        [FieldOffset(0)] public ushort e_magic;    // 'MZ'
        [FieldOffset(60)] public int e_lfanew;     // Offset vers NT Headers
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_FILE_HEADER
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DATA_DIRECTORY
    {
        public uint VirtualAddress;
        public uint Size;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_OPTIONAL_HEADER32
    {
        [FieldOffset(0)] public ushort Magic;  // 0x10b pour PE32
                                               // ... (autres champs)
        [FieldOffset(96)] public IMAGE_DATA_DIRECTORY ExportDirectory;  // Offset 96 pour Export en PE32
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_OPTIONAL_HEADER64
    {
        [FieldOffset(0)] public ushort Magic;  // 0x20b pour PE32+
                                               // ... (autres champs)
        [FieldOffset(112)] public IMAGE_DATA_DIRECTORY ExportDirectory;  // Offset 112 pour Export en PE32+
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_NT_HEADERS32
    {
        [FieldOffset(0)] public uint Signature;  // 'PE\0\0'
        [FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;
        [FieldOffset(24)] public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_NT_HEADERS64
    {
        [FieldOffset(0)] public uint Signature;  // 'PE\0\0'
        [FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;
        [FieldOffset(24)] public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_EXPORT_DIRECTORY
    {
        [FieldOffset(0)]
        public uint Characteristics;  // Flags (souvent 0, reserved)

        [FieldOffset(4)]
        public uint TimeDateStamp;  // Timestamp de la création (secondes depuis 1970)

        [FieldOffset(8)]
        public ushort MajorVersion;  // Version majeure (souvent 0)

        [FieldOffset(10)]
        public ushort MinorVersion;  // Version mineure (souvent 0)

        [FieldOffset(12)]
        public uint Name;  // RVA du nom de la DLL (string ASCII)

        [FieldOffset(16)]
        public uint Base;  // Ordinal de base (généralement 1)

        [FieldOffset(20)]
        public uint NumberOfFunctions;  // Nombre total de fonctions exportées

        [FieldOffset(24)]
        public uint NumberOfNames;  // Nombre de noms exportés (peut être < NumberOfFunctions si exports par ordinal)

        [FieldOffset(28)]
        public uint AddressOfFunctions;  // RVA du tableau des RVAs des fonctions (uint[] de taille NumberOfFunctions)

        [FieldOffset(32)]
        public uint AddressOfNames;  // RVA du tableau des RVAs des noms (uint[] de taille NumberOfNames)

        [FieldOffset(36)]
        public uint AddressOfNameOrdinals;  // RVA du tableau des ordinals (ushort[] de taille NumberOfNames)
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IMAGE_SECTION_HEADER
    {
        [FieldOffset(0)]
        public fixed byte Name[8];  // Nom de la section (8 bytes, ASCII, non null-terminated)

        // Union Misc (VirtualSize ou PhysicalAddress)
        [FieldOffset(8)]
        public uint VirtualSize;  // Taille virtuelle de la section (en mémoire)

        [FieldOffset(12)]
        public uint VirtualAddress;  // RVA (Relative Virtual Address) de la section

        [FieldOffset(16)]
        public uint SizeOfRawData;  // Taille des données brutes sur disque (multiple de FileAlignment)

        [FieldOffset(20)]
        public uint PointerToRawData;  // Offset des données brutes dans le fichier

        [FieldOffset(24)]
        public uint PointerToRelocations;  // Offset des relocations (0 pour la plupart des exécutables)

        [FieldOffset(28)]
        public uint PointerToLinenumbers;  // Offset des numéros de ligne (pour debug, souvent 0)

        [FieldOffset(32)]
        public ushort NumberOfRelocations;  // Nombre de relocations

        [FieldOffset(34)]
        public ushort NumberOfLinenumbers;  // Nombre de numéros de ligne

        [FieldOffset(36)]
        public uint Characteristics;  // Flags de la section (e.g., IMAGE_SCN_CNT_CODE, IMAGE_SCN_MEM_READ, etc.)
    }
}


