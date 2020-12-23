﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obsidian.API;
using Obsidian.API.Crafting;
using Obsidian.Entities;
using Obsidian.Items;
using Obsidian.Net.Packets.Play.Clientbound;
using Obsidian.Util.Converters;
using Obsidian.Util.Extensions;
using Obsidian.Util.Registry.Codecs;
using Obsidian.Util.Registry.Codecs.Biomes;
using Obsidian.Util.Registry.Codecs.Dimensions;
using Obsidian.Util.Registry.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Util.Registry
{
    public class Registry
    {
        internal static ILogger Logger { get; set; }

        public static Dictionary<Materials, Item> Items = new Dictionary<Materials, Item>();
        public static Dictionary<string, IRecipe> Recipes = new Dictionary<string, IRecipe>();
        public static readonly string[] Blocks = new string[763];

        public static Dictionary<string, List<Tag>> Tags = new Dictionary<string, List<Tag>>();

        public static readonly MatchTarget[] StateToMatch = new MatchTarget[17112];
        public static readonly short[] NumericToBase = new short[763];

        internal static CodecCollection<int, DimensionCodec> DefaultDimensions { get; } = new CodecCollection<int, DimensionCodec>("minecraft:dimension_type");

        internal static CodecCollection<string, BiomeCodec> DefaultBiomes { get; } = new CodecCollection<string, BiomeCodec>("minecraft:worldgen/biome");

        private readonly static JsonSerializer recipeSerializer = new JsonSerializer();

        private readonly static string mainDomain = "Obsidian.Assets";

        static Registry()
        {
            recipeSerializer.Converters.Add(new IngredientConverter());
            recipeSerializer.Converters.Add(new IngredientsConverter());
            recipeSerializer.Converters.Add(new CraftingKeyConverter());
        }

        public static async Task RegisterBlocksAsync()
        {
            using Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{mainDomain}.blocks.json");

            using var read = new StreamReader(fs, new UTF8Encoding(false));

            string json = await read.ReadToEndAsync();

            int registered = 0;

            var type = JObject.Parse(json);

            using (var enumerator = type.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var (blockName, token) = enumerator.Current;

                    var name = blockName.Split(":")[1];

                    var states = JsonConvert.DeserializeObject<BlockJson>(token.ToString(), Globals.JsonSettings);

                    if (!Enum.TryParse(name.Replace("_", ""), true, out Materials material))
                        continue;

                    if (states.States.Length <= 0)
                        continue;

                    int id = 0;
                    foreach (var state in states.States)
                        id = state.Default ? state.Id : states.States.First().Id;

                    var baseId = (short)states.States.Min(state => state.Id);
                    NumericToBase[(int)material] = baseId;

                    Blocks[(int)material] = "minecraft:" + name;

                    foreach (var state in states.States)
                    {
                        StateToMatch[state.Id] = new MatchTarget(baseId, (short)material);

                        if (id == state.Id)
                            continue;
                    }
                    registered++;
                }
            }

            Logger?.LogDebug($"Successfully registered {registered} blocks..");
        }

        public static async Task RegisterItemsAsync()
        {
            using Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{mainDomain}.items.json");

            using var read = new StreamReader(fs, new UTF8Encoding(false));

            var json = await read.ReadToEndAsync();

            var type = JObject.Parse(json);

            using var enumerator = type.GetEnumerator();
            int registered = 0;

            while (enumerator.MoveNext())
            {
                var (name, token) = enumerator.Current;

                var itemName = name.Split(":")[1];

                var item = JsonConvert.DeserializeObject<BaseRegistryJson>(token.ToString());

                if (!Enum.TryParse(itemName.Replace("_", ""), true, out Materials material))
                    continue;

                Items.Add(material, new Item((short)item.ProtocolId, name, material));
                registered++;
            }

            Logger?.LogDebug($"Successfully registered {registered} items..");
        }

        public static async Task RegisterBiomesAsync()
        {
            using Stream cfs = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{mainDomain}.biome_dimension_codec.json");

            using var cread = new StreamReader(cfs, new UTF8Encoding(false));

            var json = await cread.ReadToEndAsync();

            var type = JObject.Parse(json);

            using var cenumerator = type.GetEnumerator();

            int registered = 0;
            while (cenumerator.MoveNext())
            {
                var (name, token) = cenumerator.Current;

                foreach (var obj in token)
                {
                    var val = obj.ToString();
                    var codec = JsonConvert.DeserializeObject<BiomeCodec>(val, Globals.JsonSettings);

                    DefaultBiomes.TryAdd(codec.Name, codec);

                    registered++;
                }
            }
            Logger?.LogDebug($"Successfully registered {registered} codec biomes");
        }

        public static async Task RegisterDimensionsAsync()
        {
            using Stream cfs = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{mainDomain}.default_dimensions.json");

            using var cread = new StreamReader(cfs, new UTF8Encoding(false));

            var json = await cread.ReadToEndAsync();

            var type = JObject.Parse(json);

            using var cenumerator = type.GetEnumerator();

            int registered = 0;
            while (cenumerator.MoveNext())
            {
                var (name, token) = cenumerator.Current;

                foreach (var obj in token)
                {
                    var val = obj.ToString();
                    var codec = JsonConvert.DeserializeObject<DimensionCodec>(val, Globals.JsonSettings);

                    DefaultDimensions.TryAdd(codec.Id, codec);

                    Logger?.LogDebug($"Added codec: {codec.Name}:{codec.Id}");
                    registered++;
                }
            }
            Logger?.LogDebug($"Successfully registered {registered} codec dimensions");
        }

        public static async Task RegisterTagsAsync()
        {
            int registered = 0;

            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{mainDomain}.tags.json");

            using var reader = new StreamReader(stream, new UTF8Encoding(false));

            var element = JObject.Parse(await reader.ReadToEndAsync());

            using var enu = element.GetEnumerator();

            static void addValues(string tagBase, Tag tag, List<string> values)
            {
                foreach (var value in values)
                {
                    switch (tagBase)
                    {
                        case "items":
                            var item = GetItem(value);

                            tag.Entries.Add(item.Id);
                            break;
                        case "blocks":
                            var block = GetBlock(value);

                            tag.Entries.Add(block.Id);
                            break;
                        case "entity_types":
                            Enum.TryParse<EntityType>(value.Replace("minecraft:", "").ToCamelCase().ToLower(), true, out var type);
                            tag.Entries.Add((int)type);
                            break;
                        case "fluids":
                            Enum.TryParse<Fluids>(value.Replace("minecraft:", "").ToCamelCase().ToLower(), true, out var fluid);
                            tag.Entries.Add((int)fluid);
                            break;
                        default:
                            break;
                    }
                }
            }

            while (enu.MoveNext())
            {
                var (name, token) = enu.Current;

                var split = name.Split('/');

                var tagBase = split[0];
                var tagName = split[1];

                if (Tags.ContainsKey(tagBase))
                {
                    var tag = new Tag
                    {
                        Type = tagBase,
                        Name = tagName
                    };

                    var array = token.Value<JArray>("values");
                    var values = array.ToObject<List<string>>();

                    addValues(tagBase, tag, values);

                    Logger?.LogDebug($"Registered tag: {name} with {tag.Count} entries");

                    Tags[tagBase].Add(tag);
                }
                else
                {
                    var tag = new Tag
                    {
                        Type = tagBase,
                        Name = tagName
                    };

                    var array = token.Value<JArray>("values");
                    var values = array.ToObject<List<string>>();

                    addValues(tagBase, tag, values);

                    Logger?.LogDebug($"Registered tag: {name} with {tag.Count} entries");

                    Tags[tagBase] = new List<Tag> { tag };
                }

                registered++;
            }

            Logger?.LogDebug($"Registered {registered} tags");
        }

        public static async Task RegisterRecipesAsync()
        {
            using Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{mainDomain}.recipes.json");

            using var sw = new StreamReader(fs, new UTF8Encoding(false));

            var json = await sw.ReadToEndAsync();

            var jObject = JObject.Parse(json);

            var enu = jObject.GetEnumerator();

            while (enu.MoveNext())
            {
                var (name, element) = enu.Current;

                var type = element.Value<string>("type").Replace("minecraft:", "").ToCamelCase().ToLower();

                if (Enum.TryParse<CraftingType>(type, true, out var result))
                {
                    switch (result)
                    {
                        case CraftingType.CraftingShaped:
                            Recipes.Add(name, element.ToObject<ShapedRecipe>(recipeSerializer));
                            break;
                        case CraftingType.CraftingShapeless:
                            Recipes.Add(name, element.ToObject<ShapelessRecipe>(recipeSerializer));
                            break;
                        case CraftingType.CraftingSpecialArmordye:
                        case CraftingType.CraftingSpecialBookcloning:
                        case CraftingType.CraftingSpecialMapcloning:
                        case CraftingType.CraftingSpecialMapextending:
                        case CraftingType.CraftingSpecialFireworkRocket:
                        case CraftingType.CraftingSpecialFireworkStar:
                        case CraftingType.CraftingSpecialFireworkStarFade:
                        case CraftingType.CraftingSpecialTippedarrow:
                        case CraftingType.CraftingSpecialBannerduplicate:
                        case CraftingType.CraftingSpecialShielddecoration:
                        case CraftingType.CraftingSpecialShulkerboxcoloring:
                        case CraftingType.CraftingSpecialSuspiciousstew:
                        case CraftingType.CraftingSpecialRepairitem:
                            break;
                        case CraftingType.Smelting:
                        case CraftingType.Blasting:
                        case CraftingType.Smoking:
                        case CraftingType.CampfireCooking:
                            Recipes.Add(name, element.ToObject<SmeltingRecipe>(recipeSerializer));
                            break;
                        case CraftingType.Stonecutting:
                            Recipes.Add(name, element.ToObject<CuttingRecipe>(recipeSerializer));
                            break;
                        case CraftingType.Smithing:
                            Recipes.Add(name, element.ToObject<SmithingRecipe>(recipeSerializer));
                            break;
                        default:
                            break;
                    }
                }
            }

            Logger?.LogDebug($"Registered {Recipes.Count} recipes");
        }

        public static Block GetBlock(Materials material) => new Block(material);

        public static Block GetBlock(int id) => new Block(id);

        public static Block GetBlock(string unlocalizedName) =>
            new Block(NumericToBase[Array.IndexOf(Blocks, unlocalizedName)]);

        public static Item GetItem(int id) => Items.Values.SingleOrDefault(x => x.Id == id);
        public static Item GetItem(Materials mat) => Items.GetValueOrDefault(mat);
        public static Item GetItem(string unlocalizedName) =>
            Items.Values.SingleOrDefault(x => x.UnlocalizedName.EqualsIgnoreCase(unlocalizedName));

        public static ItemStack GetSingleItem(Materials mat, ItemMeta? meta = null) => new ItemStack(mat, 1, meta);

        public static ItemStack GetSingleItem(string unlocalizedName, ItemMeta? meta = null) => new ItemStack(GetItem(unlocalizedName).Type, 1, meta);

        private class BaseRegistryJson
        {
            [JsonProperty("protocol_id")]
            public int ProtocolId { get; set; }
        }
    }

    public class DomainTag
    {
        public string TagName { get; set; }
        public string BaseTagName { get; set; }
    }

    public struct MatchTarget
    {
        public short @base;
        public short numeric;

        public MatchTarget(short @base, short numeric)
        {
            this.@base = @base;
            this.numeric = numeric;
        }
    }
}
