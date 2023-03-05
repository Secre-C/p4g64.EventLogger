using p4g64.EventLogger.Configuration;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.Drawing;

namespace p4g64.EventLogger
{
    internal unsafe class EventLogger
    {
        public delegate long FUN_1401f9b70(long a1);
        public IHook<FUN_1401f9b70> _fUN_1401f9b70;

        public delegate long RunEventCommands(uint a1, long a2, uint a3);
        public IHook<RunEventCommands> _runEventCommandsHook;
        public EventLogger(IReloadedHooks hooks, ILogger logger, IModLoader modLoader, Config config)
        {
            modLoader.GetController<IStartupScanner>().TryGetTarget(out var scanner);

            using var thisProcess = Process.GetCurrentProcess();
            long baseAddress = thisProcess.MainModule.BaseAddress.ToInt64();

            int currentFrame = -1;

            scanner.AddMainModuleScan("48 89 6C 24 ?? 56 57 41 56 48 83 EC 30 89 CD", (result) =>
            {
                if (!result.Found)
                {
                    logger.WriteLine("Could not find Function RunEventCommands");
                    return;
                }

                long address = baseAddress + result.Offset;
                logger.WriteLine($"Found Function RunEventCommands at 0x{address:X}");

                _runEventCommandsHook = hooks.CreateHook<RunEventCommands>((uint a1, long a2, uint a3) =>
                {
                    currentFrame = (int)a1;

                    return _runEventCommandsHook.OriginalFunction(a1, a2, a3);
                }, address).Activate();
            });

            scanner.AddMainModuleScan("40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F BE 53 ??", (result) =>
            {
                if (!result.Found)
                {
                    logger.WriteLine("Could not find Function FUN_1401f9b70");
                    return;
                }

                long address = baseAddress + result.Offset;
                logger.WriteLine($"Found Function FUN_1401f9b70 at 0x{address:X}");

                _fUN_1401f9b70 = hooks.CreateHook<FUN_1401f9b70>((long a1) =>
                {
                    long result = _fUN_1401f9b70.OriginalFunction(a1);

                    int command = *(int*)(a1 + 0x38);
                    CommandObjType commandName = (CommandObjType)command;

                    int frame = *(ushort*)a1;

                    if (currentFrame == frame)
                    {
                        logger.WriteLineAsync($"[EventLogger] Command: " + $"{commandName}".PadRight(13) + $" || Frame: {frame:D5} ||", Color.PaleGreen);
                    }
                    
                    return result;
                }, address).Activate();
            });
        }

        enum CommandObjType
        {
            STAGE = 0,
            UNIT = 1,
            CAMERA = 2,
            EFFECT = 3,
            MESSAGE = 4,
            SE = 5,
            FADE = 6,
            QUAKE = 7,
            BLUR = 8,
            LIGHT = 9,
            SLIGHT = 10,
            SFOG = 11,
            SKY = 12,
            BLUR2 = 13,
            MBLUR = 14,
            DBLUR = 15,
            FILTER = 16,
            MFILTER = 17,
            BED = 18,
            BGM = 19,
            MG1 = 20,
            MG2 = 21,
            FB = 22,
            RBLUR = 23,

            TMX = 24,

            EPL = 26,
            HBLUR = 27,
            PADACT = 28,
            MOVIE = 29,
            TIMEI = 30,
            RENDERTEX = 31,
            BISTA = 32,
            CTLCAM = 33,
            WAIT = 34,
            B_UP = 35,
            CUTIN = 36,
            EVENT_EFFECT = 37,
            JUMP = 38,
            KEYFREE = 39,
            RANDOMJUMP = 40,
            CUSTOMEVENT = 41,
            CONDJUMP = 42,
            COND_ON = 43,
            COMULVJUMP = 44,
            COUNTJUMP = 45,
            HOLYJUMP = 46,
            FIELDOBJ = 47,
            PACKMODEL = 48,
            FIELDEFF = 49,
            SPUSE = 50,
            SCRIPT = 51,
            BLURFILTER = 52,
            FOG = 53,
            ENV = 54,
            FLDSKY = 55,
            FLDNOISE = 56,
            CAMERA_STATE = 57
        }
    }
}
