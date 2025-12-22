using DearImGuiInjection;

internal class SteamBypass
{
    internal static unsafe nint ResolveJmpChain(nint address, int maxDepth = 10)
    {
        for (var i = 0; i < maxDepth; i++)
        {
            var ptr = (byte*)address;

            // E9 xx xx xx xx = JMP rel32 (near jump)
            if (ptr[0] == 0xE9)
            {
                var offset = *(int*)(ptr + 1);
                address = address + 5 + offset; // 5 = size of JMP rel32 instruction
                DearImGuiInjectionLogger.Info($"Followed JMP rel32 to: 0x{address:X}");
                continue;
            }

            // EB xx = JMP rel8 (short jump)
            if (ptr[0] == 0xEB)
            {
                var offset = *(sbyte*)(ptr + 1);
                address = address + 2 + offset; // 2 = size of JMP rel8 instruction
                DearImGuiInjectionLogger.Info($"Followed JMP rel8 to: 0x{address:X}");
                continue;
            }

            // FF 25 xx xx xx xx = JMP [rip+rel32] (indirect jump, common in 64-bit)
            if (ptr[0] == 0xFF && ptr[1] == 0x25)
            {
                var offset = *(int*)(ptr + 2);
                var targetAddr = address + 6 + offset; // 6 = size of instruction
                address = *(nint*)targetAddr;
                DearImGuiInjectionLogger.Info($"Followed JMP indirect to: 0x{address:X}");
                continue;
            }

            // No more jumps, this is the real function
            break;
        }

        return address;
    }
}
