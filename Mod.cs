using p3ppc.guaranteedItemDrops.Configuration;
using p3ppc.guaranteedItemDrops.NuGet.templates.defaultPlus;
using p3ppc.guaranteedItemDrops.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p3ppc.guaranteedItemDrops;
/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private UnitStats** _unitStats;
    private IHook<GetEnemyItemDropDelegate> _getDropHook;
    private Random _random = new Random();
    private BitCheckDelegate BitCheck;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        Utils.Initialise(_logger, _configuration, _modLoader);

        Utils.SigScan("48 89 5C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 20 48 89 CB", "GetItemDrop", (address) =>
        {
            _getDropHook = _hooks.CreateHook<GetEnemyItemDropDelegate>(GetEnemyItemDrop, address).Activate();
        });

        Utils.SigScan("48 03 3D ?? ?? ?? ?? 4D 85 F6", "UnitTblPtr", (address) =>
        {
            _unitStats = (UnitStats**)Utils.GetGlobalAddress(address + 3);
            Utils.LogDebug($"Found UnitTbl at 0x{(nuint)_unitStats:X}");
        });

        Utils.SigScan("40 53 48 83 EC 20 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B C3 99", "BitCheck", (address) =>
        {
            BitCheck = _hooks.CreateWrapper<BitCheckDelegate>(address, out _);
        });

    }

    private short GetEnemyItemDrop(EnemyThing* info, uint* param_2)
    {
        var unitStats = (*_unitStats)[info->UnitId];
        if (BitCheck(unitStats.QuestFlag))
            return unitStats.QuestDrop.Item;

        var drop = _getDropHook.OriginalFunction(info, param_2);
        if (drop != 0) return drop;

        var potential = &unitStats.ItemDrops;
        int numDrops = 0;
        var drops = new short[4];
        
        for(int i = 0; i < 4; i++)
        {
            if (potential[i].Item != 0)
                drops[numDrops++] = potential[i].Item;
        }

        if (numDrops == 0) return 0;
        if (numDrops == 1) return drops[0];
        return drops[_random.Next(0, numDrops)];
    }

    private delegate short GetEnemyItemDropDelegate(EnemyThing* param_1, uint* param_2);

    private delegate bool BitCheckDelegate(int flag);

    [StructLayout(LayoutKind.Explicit)]
    struct EnemyThing
    {
        [FieldOffset(2)]
        internal short UnitId;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct UnitStats
    {
        [FieldOffset(0x22)]
        internal UnitDrop ItemDrops;

        [FieldOffset(0x32)]
        internal short QuestFlag;

        [FieldOffset(0x34)]
        internal UnitDrop QuestDrop;

        // Just here to make sure the struct is the right length
        [FieldOffset(0x3c)]
        short AttackType;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct UnitDrop
    {
        internal short Item;
        internal short DropRate;
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}