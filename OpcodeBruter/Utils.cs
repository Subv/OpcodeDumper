using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpcodeBruter
{
    /// <summary>
    /// Description of Utils.
    /// </summary>
    public static class Utils
    {
        const uint PAGE_EXECUTE_READWRITE = 0x40;
        const uint MEM_COMMIT = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        public delegate byte ByteReturner(int _this, int a2, int a3, ushort opcode, int a5);
        public static ByteReturner AsDelegate(this byte[] body)
        {
            IntPtr buf = VirtualAlloc(IntPtr.Zero, (uint)body.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(body, 0, buf, body.Length);
            
            return (ByteReturner)Marshal.GetDelegateForFunctionPointer(buf, typeof(ByteReturner));
        }
        
        public static Regex IfNonZero = new Regex(@"if \( (v[0-9]+) \)", RegexOptions.IgnoreCase);
        public static Regex SubCall = new Regex(@" +((?:.+)(?:sub_([0-9A-F]+)(?:.+);))", RegexOptions.IgnoreCase);
        public static string GenerateHandlerFromHexrays(string hexrays)
        {
            StringBuilder builder = new StringBuilder();
            var lines = hexrays.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            
            var returnValues = new string[2];
            var insertIndex  = 0;
            Match regexpMatches = null;

            foreach (var line in lines)
            {
                // Ignore the function prototype.
                if (line.Contains("call sub_"))
                    continue;
                
                if ((regexpMatches = SubCall.Match(line)).Success) {
                    if (insertIndex == 2)
                        continue;
                    returnValues[insertIndex] = regexpMatches.Groups[2].Value;
                    ++insertIndex;
                    continue;
                }
                else if (line.Contains("&v")) // Can't be before as it'll ignore the sub calls.
                    continue;
                else if (line.Contains("return 1;")) {
                    builder.AppendLine(line.Replace("return 1;", "return new uint[] { 0x"+returnValues[0]+", 0x"+returnValues[1]+" };"));
                    returnValues[0] = returnValues[1] = String.Empty;
                    insertIndex = 0;
                    continue;
                }
                else if (line.Contains("return 0;")) {
                    builder.AppendLine(line.Replace("return 0;", "return new uint[2];"));
                    returnValues[0] = returnValues[1] = String.Empty;
                    insertIndex = 0;
                    continue;
                }
                else if (line.Contains("goto LABEL_")) {
                    builder.AppendLine("return new uint[] { 0x"+returnValues[0]+", 0x"+returnValues[1]+" };");
                    returnValues[0] = returnValues[1] = String.Empty;
                    insertIndex = 0;
                    continue;
                }
                else if (line.Contains("LABEL_") || line.Contains("char *") || line.Contains("[sp+"))
                    continue;
                
                var lineCopy = line;
                var typesArray = new string[] { " signed int v", " int v", " char v", " __int16 v" };
                foreach (var tstr in typesArray)
                    lineCopy = lineCopy.Replace(tstr, "uint v");
                lineCopy = lineCopy.Replace("a4", "opcode");
                lineCopy = lineCopy.Replace("if ( !v", "if ( 0 == v"); // Yoda condition!
                if (IfNonZero.Match(lineCopy).Success)
                    lineCopy = lineCopy.Replace(" )", " != 0 )");
                builder.AppendLine(lineCopy);
            }
            return builder.ToString();
        }

        public static unsafe List<uint> FindPattern(this byte[] Haystack, byte[] Needle, byte wildcard)
        {
            List<uint> Indexes = new List<uint>();
            fixed (byte* H = Haystack) fixed (byte* N = Needle)
            {
                uint i = 0;
                for (byte* hNext = H, hEnd = H + Haystack.LongLength; hNext < hEnd; i++, hNext++)
                {
                    bool Found = true;
                    for (byte* hInc = hNext, nInc = N, nEnd = N + Needle.LongLength; Found && nInc < nEnd; Found = *nInc == wildcard || *nInc == *hInc, nInc++, hInc++) ;
                    if (Found) Indexes.Add(i);
                }
                return Indexes;
            }
        }
    }
}
