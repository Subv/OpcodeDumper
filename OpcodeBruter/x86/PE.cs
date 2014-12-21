using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Linq;
using OpcodeBruter;

namespace x86
{
    // Reads in the header information of the Portable Executable format.
    // Provides information such as the date the assembly was compiled.
    public class PeHeaderReader
    {
        private const UInt16 IMAGE_FILE_32BIT_MACHINE = 0x0100;

        #region File Header Structures
        public struct IMAGE_DOS_HEADER // DOS .EXE header
        {
            public UInt16 e_magic; // Magic number
            public UInt16 e_cblp; // Bytes on last page of file
            public UInt16 e_cp; // Pages in file
            public UInt16 e_crlc; // Relocations
            public UInt16 e_cparhdr; // Size of header in paragraphs
            public UInt16 e_minalloc; // Minimum extra paragraphs needed
            public UInt16 e_maxalloc; // Maximum extra paragraphs needed
            public UInt16 e_ss; // Initial (relative) SS value
            public UInt16 e_sp; // Initial SP value
            public UInt16 e_csum; // Checksum
            public UInt16 e_ip; // Initial IP value
            public UInt16 e_cs; // Initial (relative) CS value
            public UInt16 e_lfarlc; // File address of relocation table
            public UInt16 e_ovno; // Overlay number
            public UInt16 e_res_0; // Reserved words
            public UInt16 e_res_1; // Reserved words
            public UInt16 e_res_2; // Reserved words
            public UInt16 e_res_3; // Reserved words
            public UInt16 e_oemid; // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo; // OEM information; e_oemid specific
            public UInt16 e_res2_0; // Reserved words
            public UInt16 e_res2_1; // Reserved words
            public UInt16 e_res2_2; // Reserved words
            public UInt16 e_res2_3; // Reserved words
            public UInt16 e_res2_4; // Reserved words
            public UInt16 e_res2_5; // Reserved words
            public UInt16 e_res2_6; // Reserved words
            public UInt16 e_res2_7; // Reserved words
            public UInt16 e_res2_8; // Reserved words
            public UInt16 e_res2_9; // Reserved words
            public UInt32 e_lfanew; // File address of new exe header
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public IMAGE_OPTIONAL_HEADER_MAGIC Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public IMAGE_OPTIONAL_HEADER_SUBSYSTEM Subsystem;
            public IMAGE_OPTIONAL_HEADER_DLL_CHARACTERISTICS DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16 * 8)]
            public Byte[] DataDirectory;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public IMAGE_OPTIONAL_HEADER_MAGIC Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt64 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public IMAGE_OPTIONAL_HEADER_SUBSYSTEM Subsystem;
            public IMAGE_OPTIONAL_HEADER_DLL_CHARACTERISTICS DllCharacteristics;
            public UInt64 SizeOfStackReserve;
            public UInt64 SizeOfStackCommit;
            public UInt64 SizeOfHeapReserve;
            public UInt64 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        public struct IMAGE_NT_HEADERS
        {
            public UInt32 Signature;
            public IMAGE_FILE_HEADER FileHeader;
            // public IMAGE_OPTIONAL_HEADER OptionalHeaders; // 32 or 64
        }

        public struct IMAGE_FILE_HEADER
        {
            public IMAGE_FILE_HEADER_MACHINE_TYPES Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public IMAGE_FILE_HEADER_CHARACTERISTICS Characteristics;
        }

        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_SECTION_HEADER
        {
            // Reordered, don't ask why. It just works.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] Name;
            public UInt32 VirtualSize;
            public UInt32 VirtualAddress;
            public UInt32 SizeOfRawData;
            public UInt32 PhysicalAddress;
            public UInt32 PointerToRawData;
            public UInt32 PointerToRelocations;
            public UInt32 PointerToLinenumbers;
            public UInt16 NumberOfRelocations;
            public UInt16 NumberOfLinenumbers;
            public UInt32 Characteristics;

            public string NameString { get { return Encoding.UTF8.GetString(Name); } }
        }
        #endregion File Header Structures

        #region Public Fields
        public IMAGE_DOS_HEADER DosHeader { get; private set; }
        // public IMAGE_NT_HEADERS NtHeader { get; private set; }
        public IMAGE_FILE_HEADER FileHeader { get; private set; }
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader32 { get; private set; }
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader64 { get; private set; }
        public List<IMAGE_SECTION_HEADER> SectionHeaders { get; private set; }
        private BinaryReader Reader;

        public IMAGE_SECTION_HEADER RDATA { get {
                return SectionHeaders.First(i => i.NameString.Contains(".rdata"));
            }
        }
        public IMAGE_SECTION_HEADER RODATA { get {
                return SectionHeaders.First(i => i.NameString.Contains(".rodata"));
            }
        }
        public IMAGE_SECTION_HEADER DATA { get {
                return SectionHeaders.First(i => i.NameString.Contains(".data"));
            }
        }
        public IMAGE_SECTION_HEADER TEXT { get {
                return SectionHeaders.First(i => i.NameString.Contains(".text"));
            }
        }
        #endregion

        #region Public Methods
        public PeHeaderReader(string filePath)
        {
            SectionHeaders = new List<IMAGE_SECTION_HEADER>();
            // Read in the DLL or EXE and get the timestamp
            Reader = new BinaryReader(new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read));
            DosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(Reader);

            // Seek to New Exe Header + 4 (skip signature)
            Reader.BaseStream.Seek(DosHeader.e_lfanew + 4, SeekOrigin.Begin);

            FileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(Reader);
            /*if (NtHeader.Signature != 0x00004550) // PE\0\0
                return;*/

            if (FileHeader.SizeOfOptionalHeader != 0)
            {
                if (this.Is32BitHeader)
                    OptionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(Reader);
                else
                    OptionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(Reader);
            }

            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms680313(v=vs.85).aspx
            ParseFileHeader(FileHeader);
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms680339(v=vs.85).aspx
            ParseOptionalHeader();
        }

        // Reads in a block from a file and converts it to the struct
        // type specified by the template parameter
        public static T FromBinaryReader<T>(BinaryReader reader) where T : struct
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }
        #endregion Public Methods

        #region Properties
        // Gets if the file header is 32 bit or not
        public bool Is32BitHeader
        {
            get
            {
                return (IMAGE_FILE_32BIT_MACHINE & (ushort)FileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
            }
        }

        // Gets the timestamp from the file header
        public DateTime TimeStamp
        {
            get
            {
                // Timestamp is a date offset from 1970
                DateTime returnValue = new DateTime(1970, 1, 1, 0, 0, 0);

                // Add in the number of seconds since 1970/1/1
                returnValue = returnValue.AddSeconds(FileHeader.TimeDateStamp);
                // Adjust to local timezone
                returnValue += TimeZone.CurrentTimeZone.GetUtcOffset(returnValue);

                return returnValue;
            }
        }
        #endregion Properties

        public uint GetPhysicalAddress(string sectionName)
        {
            return SectionHeaders.First(i => i.NameString.Contains(sectionName)).PhysicalAddress;
        }

        public uint GetPhysicalAddressEnd(string sectionName)
        {
            return SectionHeaders.Where(i => i.NameString.Contains(sectionName)).Select(o => o.SizeOfRawData + o.PhysicalAddress).First();
        }

        private void ParseFileHeader(IMAGE_FILE_HEADER fileHeader)
        {
            LogStructure(fileHeader);
        }

        private void ParseOptionalHeader()
        {
            if (this.Is32BitHeader)
                ParseOptionalHeader(OptionalHeader32);
            else
                ParseOptionalHeader(OptionalHeader64);
        }

        private void ParseOptionalHeader(IMAGE_OPTIONAL_HEADER32 optHeader)
        {
            LogStructure(optHeader);
            /*for (uint i = 0; i < optHeader.NumberOfRvaAndSizes; ++i)
                ParseDataDirectory(optHeader.DataDirectory[i], i);*/
            for (uint i = 0; i < FileHeader.NumberOfSections; ++i)
                ParseImageSectionHeader();
        }

        private void ParseOptionalHeader(IMAGE_OPTIONAL_HEADER64 optHeader)
        {
            LogStructure(optHeader);
            /*for (uint i = 0; i < optHeader.NumberOfRvaAndSizes; ++i)
                ParseDataDirectory(optHeader.DataDirectory[i], i);*/
            for (uint i = 0; i < FileHeader.NumberOfSections; ++i)
                ParseImageSectionHeader();
        }

        private void ParseDataDirectory(IMAGE_DATA_DIRECTORY directory, uint position)
        {
            LogStructure(directory);
        }

        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms680341(v=vs.85).aspx
        private void ParseImageSectionHeader()
        {
            var header = FromBinaryReader<IMAGE_SECTION_HEADER>(Reader);
            SectionHeaders.Add(header);
            LogStructure(header);
            Reader.BaseStream.Seek(-4, SeekOrigin.Current); // The fuck? Why is this needed?
        }

        private void LogStructure(object obj)
        {
            if (!Config.Debug)
                return;

            Console.WriteLine(obj.GetType().Name);
            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute(typeof(MarshalAsAttribute), false) != null)
                {
                    if (field.FieldType.GetElementType().IsPrimitive)
                        Logger.WriteConsoleLine("-> {0}: {1}", field.Name.PadRight(40), Encoding.UTF8.GetString((byte[])field.GetValue(obj)));
                    else
                        LogStructure(field.GetValue(obj));
                }
                else if (field.FieldType.IsPrimitive)
                    Logger.WriteConsoleLine("-> {0}: 0x{1:X8} ({1})", field.Name.PadRight(40), field.GetValue(obj));
                else foreach (var subField in field.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                    Logger.WriteConsoleLine("-> {0}: 0x{1:X8} ({1})", String.Format("{0}.{1}", field.Name, subField.Name).PadRight(40), subField.GetValue(field));
            }
        }
    }
}
