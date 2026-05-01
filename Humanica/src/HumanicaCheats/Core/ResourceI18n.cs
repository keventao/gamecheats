using System.Collections.Generic;

namespace HumanicaCheats.Core
{
    // Il2Cpp.ResourceIndex 枚举名 → 中文显示名。
    // 启动 dump (2026-05-02) 共 143 项;此表覆盖非 ___DEPRECATED 项。
    // 缺失项 fallback 到 enum 名本身,不 crash。
    public static class ResourceI18n
    {
        public static readonly Dictionary<string, string> Zh = new()
        {
            // 原料
            ["STICKS"]              = "树枝",
            ["LOG"]                 = "原木",
            ["COBBLESTONES"]        = "鹅卵石",
            ["STONE_BRICKS"]        = "石砖",
            ["CLAY"]                = "黏土",
            ["CLAY_POT"]            = "陶罐",
            ["COAL"]                = "煤炭",
            ["STRAW"]               = "稻草",
            ["FLAX"]                = "亚麻",
            ["WOOL"]                = "羊毛",
            ["WOOL_YARN"]           = "毛纱",
            ["LINEN_FABRIC"]        = "亚麻布",
            ["LEATHER"]             = "皮革",
            ["BONES"]               = "骨头",
            ["TOOL_HANDLE"]         = "工具柄",
            ["BASKET"]              = "篮子",
            ["WICKER_BACKPACK"]     = "柳条背包",
            ["COMPOST"]             = "堆肥",

            // 矿/锭
            ["COPPER_ORE"]          = "铜矿石",
            ["COPPER_INGOT"]        = "铜锭",
            ["TIN_ORE"]             = "锡矿石",
            ["TIN_INGOT"]           = "锡锭",
            ["BRONZE_INGOT"]        = "青铜锭",
            ["IRON_ORE"]            = "铁矿石",
            ["IRON_INGOT"]          = "铁锭",
            ["BRONZE_PLATE"]        = "青铜板",
            ["IRON_PLATE"]          = "铁板",

            // 动物 / 生品
            ["RAW_PELT"]            = "生皮",
            ["PELT"]                = "鞣制皮",
            ["PELT_CLOTHING"]       = "皮毛衣物",
            ["LEATHER_ARMOR"]       = "皮甲",
            ["LINEN_CLOTHING"]      = "亚麻衣物",
            ["WOOLEN_CLOTHING"]     = "羊毛衣物",
            ["CHICKEN"]             = "鸡",
            ["GOAT"]                = "山羊",
            ["SHEEP"]               = "绵羊",

            // 食物 / 饮品
            ["RAW_MEAT"]            = "生肉",
            ["JERKY"]               = "肉干",
            ["FISH"]                = "鱼",
            ["DRIED_FISH"]          = "鱼干",
            ["EGGS"]                = "鸡蛋",
            ["MILK"]                = "牛奶",
            ["CHEESE"]              = "奶酪",
            ["HONEY"]               = "蜂蜜",
            ["BREAD"]               = "面包",
            ["FLOUR"]               = "面粉",
            ["BEER"]                = "啤酒",
            ["DRIED_FRUITS"]        = "果干",

            // 农作物 / 水果 / 种子
            ["WHEAT"]               = "小麦",
            ["WHEAT_SEEDS"]         = "小麦种子",
            ["BEANS"]               = "豆子",
            ["BEAN_SEEDS"]          = "豆种子",
            ["PEAS"]                = "豌豆",
            ["PEAS_SEEDS"]          = "豌豆种子",
            ["CABBAGE"]             = "卷心菜",
            ["CABBAGE_SEEDS"]       = "卷心菜种子",
            ["CARROT"]              = "胡萝卜",
            ["CARROT_SEEDS"]        = "胡萝卜种子",
            ["BEET"]                = "甜菜",
            ["BEET_SEEDS"]          = "甜菜种子",
            ["TURNIP"]              = "芜菁",
            ["TURNIP_SEEDS"]        = "芜菁种子",
            ["FLAX_SEEDS"]          = "亚麻种子",
            ["NUTS"]                = "坚果",
            ["WILD_BERRIES"]        = "野莓",
            ["APPLE"]               = "苹果",
            ["APPLE_SEEDS"]         = "苹果种子",
            ["PEARS"]               = "梨",
            ["PEAR_SEEDS"]          = "梨种子",
            ["CHERRY"]              = "樱桃",
            ["CHERRY_SEEDS"]        = "樱桃种子",
            ["PLUM"]                = "李子",
            ["PLUM_SEEDS"]          = "李子种子",

            // 武器 / 工具(石器)
            ["SLING"]               = "投石索",
            ["SHORT_BOW"]           = "短弓",
            ["LONG_BOW"]            = "长弓",
            ["BONE_SPEAR"]          = "骨矛",
            ["BONE_KNIFE"]          = "骨刀",
            ["BONE_HARPOON"]        = "骨鱼叉",
            ["BONE_SICKLE"]         = "骨镰",
            ["BONE_HOE"]            = "骨锄",
            ["STONE_SPEAR"]         = "石矛",
            ["STONE_KNIFE"]         = "石刀",
            ["STONE_AXE"]           = "石斧",
            ["STONE_SICKLE"]        = "石镰",
            ["STONE_HOE"]           = "石锄",
            ["STONE_HAMMER"]        = "石锤",
            ["STONE_SHOVEL"]        = "石铲",

            // 武器 / 工具(铜器)
            ["COPPER_KNIFE"]        = "铜刀",
            ["COPPER_AXE"]          = "铜斧",
            ["COPPER_PICKAXE"]      = "铜镐",
            ["COPPER_SICKLE"]       = "铜镰",
            ["COPPER_HOE"]          = "铜锄",
            ["COPPER_HAMMER"]       = "铜锤",
            ["COPPER_SHOVEL"]       = "铜铲",
            ["COPPER_SPEAR"]        = "铜矛",
            ["COPPER_SWORD"]        = "铜剑",

            // 武器 / 工具(青铜)
            ["BRONZE_KNIFE"]        = "青铜刀",
            ["BRONZE_AXE"]          = "青铜斧",
            ["BRONZE_PICKAXE"]      = "青铜镐",
            ["BRONZE_SICKLE"]       = "青铜镰",
            ["BRONZE_HOE"]          = "青铜锄",
            ["BRONZE_HAMMER"]       = "青铜锤",
            ["BRONZE_SHOVEL"]       = "青铜铲",
            ["BRONZE_SPEAR"]        = "青铜矛",
            ["BRONZE_SWORD"]        = "青铜剑",
            ["BRONZE_ARMOR"]        = "青铜甲",

            // 武器 / 工具(铁器)
            ["IRON_KNIFE"]          = "铁刀",
            ["IRON_AXE"]            = "铁斧",
            ["IRON_PICKAXE"]        = "铁镐",
            ["IRON_SICKLE"]         = "铁镰",
            ["IRON_HOE"]            = "铁锄",
            ["IRON_HAMMER"]         = "铁锤",
            ["IRON_SHOVEL"]         = "铁铲",
            ["IRON_SPEAR"]          = "铁矛",
            ["IRON_SWORD"]          = "铁剑",
            ["IRON_ARMOR"]          = "铁甲",

            // 科技 / 蓝图
            ["TECHNOLOGY_KNOWLEDGE"]              = "科技知识",
            ["ANNUAL_PLAN_BLUEPRINT_MESOLITHIC"]  = "年度蓝图(中石器)",
            ["ANNUAL_PLAN_BLUEPRINT_NEOLITHIC"]   = "年度蓝图(新石器)",
            ["ANNUAL_PLAN_BLUEPRINT_BRONZE_AGE"]  = "年度蓝图(青铜)",
            ["ANNUAL_PLAN_BLUEPRINT_IRON_AGE"]    = "年度蓝图(铁器)",
        };

        // 显示名:有翻译就用 "EN / 中文",没就用 EN
        public static string Display(string enumName)
            => Zh.TryGetValue(enumName, out var zh) ? $"{enumName} / {zh}" : enumName;
    }
}
