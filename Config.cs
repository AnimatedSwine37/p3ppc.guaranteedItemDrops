using p3ppc.guaranteedItemDrops.Template.Configuration;
using System.ComponentModel;

namespace p3ppc.guaranteedItemDrops.Configuration;
public class Config : Configurable<Config>
{
    [DisplayName("Guarantee Quest Drops")]
    [Description("If enabled when you have a quest active enemies will always drop the item for it." +
        "\nOtherwise they will have their normal chance to drop.")]
    [DefaultValue(true)]
    public bool GuaranteeQuestDrops { get; set; } = true;

    [DisplayName("Guarantee Regular Drops")]
    [Description("If enabled enemies will always drop an item if they can, " +
        "if there are multiple potential drops there is an equal chance of getting any one.")]
    [DefaultValue(true)]
    public bool GuaranteeNormalDrops { get; set; } = true;

    [DisplayName("Debug Mode")]
    [Description("Logs additional information to the console that is useful for debugging.")]
    [DefaultValue(false)]
    public bool DebugEnabled { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}