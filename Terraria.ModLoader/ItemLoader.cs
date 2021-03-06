﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader {
public static class ItemLoader
{
    private static int nextItem = ItemID.Count;
    internal static readonly IDictionary<int, ModItem> items = new Dictionary<int, ModItem>();
    internal static readonly IList<int> animations = new List<int>();

    internal static int ReserveItemID()
    {
        int reserveID = nextItem;
        nextItem++;
        return reserveID;
    }

    public static ModItem GetItem(int type)
    {
        if (items.ContainsKey(type))
        {
            return items[type];
        }
        else
        {
            return null;
        }
    }

    internal static void ResizeArrays()
    {
        Array.Resize(ref Main.itemTexture, nextItem);
        Array.Resize(ref Main.itemName, nextItem);
        Array.Resize(ref Main.itemFlameLoaded, nextItem);
        Array.Resize(ref Main.itemFlameTexture, nextItem);
        Array.Resize(ref Main.itemAnimations, nextItem);
        Array.Resize(ref Item.staff, nextItem);
        Array.Resize(ref Item.claw, nextItem);
        Array.Resize(ref ItemID.Sets.Deprecated, nextItem);
        Array.Resize(ref ItemID.Sets.NeverShiny, nextItem);
        Array.Resize(ref ItemID.Sets.ItemIconPulse, nextItem);
        Array.Resize(ref ItemID.Sets.ItemNoGravity, nextItem);
        Array.Resize(ref ItemID.Sets.ExtractinatorMode, nextItem);
        Array.Resize(ref ItemID.Sets.StaffMinionSlotsRequired, nextItem);
        Array.Resize(ref ItemID.Sets.ExoticPlantsForDyeTrade, nextItem);
        Array.Resize(ref ItemID.Sets.NebulaPickup, nextItem);
        Array.Resize(ref ItemID.Sets.AnimatesAsSoul, nextItem);
        Array.Resize(ref ItemID.Sets.gunProj, nextItem);
    }

    internal static void Unload()
    {
        items.Clear();
        nextItem = ItemID.Count;
        animations.Clear();
    }

    internal static bool IsModItem(Item item)
    {
        return item.type >= ItemID.Count;
    }

    //in Terraria.Item.SetDefaults get rid of type-too-high check
    //add near end of Terraria.Item.SetDefaults before setting netID
    internal static void SetupItem(Item item)
    {
        if (IsModItem(item))
        {
            GetItem(item.type).SetupItem(item);
        }
        GlobalSetDefaults(item);
    }

    internal static void GlobalSetDefaults(Item item)
    {
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.SetDefaults(item);
            }
        }
    }

    //near end of Terraria.Main.DrawItem before default drawing call
    //  if(ItemLoader.animations.Contains(item.type))
    //  { ItemLoader.DrawAnimatedItem(item, color, alpha, rotation, scale); return; }
    internal static void DrawAnimatedItem(Item item, Color color, Color alpha, float rotation, float scale)
    {
        int frameCount = Main.itemAnimations[item.type].FrameCount;
        int frameDuration = Main.itemAnimations[item.type].TicksPerFrame;
        Main.itemFrameCounter[item.whoAmI]++;
        if (Main.itemFrameCounter[item.whoAmI] >= frameDuration)
        {
            Main.itemFrameCounter[item.whoAmI] = 0;
            Main.itemFrame[item.whoAmI]++;
        }
        if (Main.itemFrame[item.whoAmI] >= frameCount)
        {
            Main.itemFrame[item.whoAmI] = 0;
        }
        Rectangle frame = Main.itemTexture[item.type].Frame(1, frameCount, 0, Main.itemFrame[item.whoAmI]);
        float offX = (float)(item.width / 2 - frame.Width / 2);
        float offY = (float)(item.height - frame.Height);
        Main.spriteBatch.Draw(Main.itemTexture[item.type], new Vector2(item.position.X - Main.screenPosition.X + (float)(frame.Width / 2) + offX, item.position.Y - Main.screenPosition.Y + (float)(frame.Height / 2) + offY), new Rectangle?(frame), alpha, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
        if(item.color != default(Color))
        {
            Main.spriteBatch.Draw(Main.itemTexture[item.type], new Vector2(item.position.X - Main.screenPosition.X + (float)(frame.Width / 2) + offX, item.position.Y - Main.screenPosition.Y + (float)(frame.Height / 2) + offY), new Rectangle?(frame), item.GetColor(color), rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }

    //in Terraria.Player.ItemCheck
    //  inside block if (this.controlUseItem && this.itemAnimation == 0 && this.releaseUseItem && item.useStyle > 0)
    //  set initial flag2 to ItemLoader.CanUseItem(item, this)
    internal static bool CanUseItem(Item item, Player player)
    {
        bool flag = true;
        if(IsModItem(item))
        {
            flag = flag && item.modItem.CanUseItem(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                flag = flag && mod.globalItem.CanUseItem(item, player);
            }
        }
        return flag;
    }

    //in Terraria.Player.ItemCheck after useStyle if/else chain call ItemLoader.UseStyle(item, this)
    internal static void UseStyle(Item item, Player player)
    {
        if(IsModItem(item))
        {
            item.modItem.UseStyle(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.UseStyle(item, player);
            }
        }
    }

    //in Terraria.Player.ItemCheck after holdStyle if/else chain call ItemLoader.HoldStyle(item, this)
    internal static void HoldStyle(Item item, Player player)
    {
        if(!player.pulley && player.itemAnimation <= 0)
        {
            if(IsModItem(item))
            {
                item.modItem.HoldStyle(player);
            }
            foreach(Mod mod in ModLoader.mods.Values)
            {
                if(mod.globalItem != null)
                {
                    mod.globalItem.HoldStyle(item, player);
                }
            }
        }
    }

    //in Terraria.Player.ItemCheck before this.controlUseItem setting this.releaseUseItem call ItemLoader.HoldItem(item, this)
    internal static void HoldItem(Item item, Player player)
    {
        if(IsModItem(item))
        {
            item.modItem.HoldItem(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.HoldItem(item, player);
            }
        }
    }

    //in Terraria.Player.PickAmmo before flag2 is checked add
    //  if(!ItemLoader.ConsumeAmmo(sItem, item, this)) { flag2 = true; }
    internal static bool ConsumeAmmo(Item item, Item ammo, Player player)
    {
        if(IsModItem(item) && !item.modItem.ConsumeAmmo(player))
        {
            return false;
        }
        if(IsModItem(ammo) && !ammo.modItem.ConsumeAmmo(player))
        {
            return false;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                if(!mod.globalItem.ConsumeAmmo(item, player) || !mod.globalItem.ConsumeAmmo(ammo, player))
                {
                    return false;
                }
            }
        }
        return true;
    }

    //in Terraria.Player.ItemCheck at end of if/else chain for shooting place if on last else
    //  if(ItemLoader.Shoot(item, this, ref vector2, ref num78, ref num79, ref num71, ref num73, ref num74))
    internal static bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
    {
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && !mod.globalItem.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack))
            {
                return false;
            }
        }
        if(IsModItem(item))
        {
            if(!item.modItem.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack))
            {
                return false;
            }
        }
        return true;
    }

    //in Terraria.Player.ItemCheck after end of useStyle if/else chain for melee hitbox
    //  call ItemLoader.UseItemHitbox(item, this, ref r2, ref flag17)
    internal static void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if(IsModItem(item))
        {
            item.modItem.UseItemHitbox(player, ref hitbox, ref noHitbox);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
            }
        }
    }

    //in Terraria.Player.ItemCheck after magma stone dust effect for melee weapons
    //  call ItemLoader.MeleeEffects(item, this, r2)
    internal static void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        if(IsModItem(item))
        {
            item.modItem.MeleeEffects(player, hitbox);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.MeleeEffects(item, player, hitbox);
            }
        }
    }

    //in Terraria.Player.ItemCheck for melee attacks between crit determination and banner damage
    //  call ItemLoader.ModifyHitNPC(item, this, Main.npc[num292], ref num282, ref num283, ref flag18)
    internal static void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
    {
        if(IsModItem(item))
        {
            item.modItem.ModifyHitNPC(player, target, ref damage, ref knockBack, ref crit);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.ModifyHitNPC(item, player, target, ref damage, ref knockBack, ref crit);
            }
        }
    }

    //in Terraria.Player.ItemCheck for melee attacks before updating informational accessories
    //  call ItemLoader.OnHitNPC(item, this, Main.npc[num292], num295, num283, flag18)
    internal static void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
    {
        if(IsModItem(item))
        {
            item.modItem.OnHitNPC(player, target, damage, knockBack, crit);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.OnHitNPC(item, player, target, damage, knockBack, crit);
            }
        }
    }

    //in Terraria.Player.ItemCheck for pvp melee attacks between crit determination and damage var
    //  call ItemLoader.ModifyHitPvp(item, this, Main.player[num302], ref num282, ref flag20)
    internal static void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit)
    {
        if(IsModItem(item))
        {
            item.modItem.ModifyHitPvp(player, target, ref damage, ref crit);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.ModifyHitPvp(item, player, target, ref damage, ref crit);
            }
        }
    }

    //in Terraria.Player.ItemCheck for pvp melee attacks before NetMessage stuff
    //  call ItemLoader.OnHitPvp(item, this, Main.player[num302], num304, flag20)
    internal static void OnHitPvp(Item item, Player player, Player target, int damage, bool crit)
    {
        if(IsModItem(item))
        {
            item.modItem.OnHitPvp(player, target, damage, crit);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.OnHitPvp(item, player, target, damage, crit);
            }
        }
    }

    //in Terraria.Player.ItemCheck inside block if (this.itemTime == 0 && this.itemAnimation > 0) before hairDye
    //  call ItemLoader.UseItem(item, this)
    internal static void UseItem(Item item, Player player)
    {
        if(IsModItem(item) && item.modItem.UseItem(player))
        {
            player.itemTime = item.useTime;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && mod.globalItem.UseItem(item, player))
            {
                player.itemTime = item.useTime;
            }
        }
    }

    //earn end of Terraria.Player.ItemCheck before consumable item is consumed
    //  call ItemLoader.ConsumeItem(item, this, ref flag22)
    internal static void ConsumeItem(Item item, Player player, ref bool consume)
    {
        if(IsModItem(item) && !item.modItem.ConsumeItem(player))
        {
            consume = false;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && !mod.globalItem.ConsumeItem(item, player))
            {
                consume = false;
            }
        }
    }

    //in Terraria.Player.PlayerFrame at end of useStyle if/else chain
    //  call if(ItemLoader.UseItemFrame(this.inventory[this.selectedItem], this)) { return; }
    internal static bool UseItemFrame(Item item, Player player)
    {
        if(IsModItem(item) && item.modItem.UseItemFrame(player))
        {
            return true;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && mod.globalItem.UseItemFrame(item, player))
            {
                return true;
            }
        }
        return false;
    }

    //in Terraria.Player.PlayerFrame at end of holdStyle if statements
    //  call if(ItemLoader.HoldItemFrame(this.inventory[this.selectedItem], this)) { return; }
    internal static bool HoldItemFrame(Item item, Player player)
    {
        if(IsModItem(item) && item.modItem.HoldItemFrame(player))
        {
            return true;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && mod.globalItem.HoldItemFrame(item, player))
            {
                return true;
            }
        }
        return false;
    }

    //place at end of first for loop in Terraria.Player.UpdateEquips
    //  call ItemLoader.UpdateInventory(this.inventory[j], this)
    internal static void UpdateInventory(Item item, Player player)
    {
        if(IsModItem(item))
        {
            item.modItem.UpdateInventory(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.UpdateInventory(item, player);
            }
        }
    }

    //place in second for loop of Terraria.Player.UpdateEquips before prefix checking
    //  call ItemLoader.UpdateEquip(this.armor[k], this)
    internal static void UpdateEquip(Item item, Player player)
    {
        if(IsModItem(item))
        {
            item.modItem.UpdateEquip(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.UpdateEquip(item, player);
            }
        }
    }

    //place at end of third for loop of Terraria.Player.UpdateEquips
    //  call ItemLoader.UpdateAccessory(this.armor[l], this)
    internal static void UpdateAccessory(Item item, Player player)
    {
        if(IsModItem(item))
        {
            item.modItem.UpdateAccessory(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.UpdateAccessory(item, player);
            }
        }
    }

    //at end of Terraria.Player.UpdateArmorSets call ItemLoader.UpdateArmorSet(this, this.armor[0], this.armor[1], this.armor[2])
    internal static void UpdateArmorSet(Player player, Item head, Item body, Item legs)
    {
        if(IsModItem(head) && head.modItem.IsArmorSet(head, body, legs))
        {
            head.modItem.UpdateArmorSet(player);
        }
        if(IsModItem(body) && body.modItem.IsArmorSet(head, body, legs))
        {
            body.modItem.UpdateArmorSet(player);
        }
        if(IsModItem(legs) && legs.modItem.IsArmorSet(head, body, legs))
        {
            legs.modItem.UpdateArmorSet(player);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                string set = mod.globalItem.IsArmorSet(head, body, legs);
                if(set.Length > 0)
                {
                    mod.globalItem.UpdateArmorSet(player, set);
                }
            }
        }
    }

    //in Terraria.UI.ItemSlot.RightClick in end of item-opening if/else chain before final else
    //  make else if(ItemLoader.CanRightClick(inv[slot]))
    internal static bool CanRightClick(Item item)
    {
        if(IsModItem(item) && item.modItem.CanRightClick())
        {
            return Main.mouseRight;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && mod.globalItem.CanRightClick(item))
            {
                return Main.mouseRight;
            }
        }
        return false;
    }

    //in Terraria.UI.ItemSlot in block from CanRightClick call ItemLoader.RightClick(inv[slot], player)
    internal static void RightClick(Item item, Player player)
    {
        if (Main.mouseRightRelease)
        {
            if(IsModItem(item))
            {
                item.modItem.RightClick(player);
            }
            foreach(Mod mod in ModLoader.mods.Values)
            {
                if(mod.globalItem != null)
                {
                    mod.globalItem.RightClick(item, player);
                }
            }
            item.stack--;
            if (item.stack == 0)
            {
                item.SetDefaults(0, false);
            }
            Main.PlaySound(7, -1, -1, 1);
            Main.stackSplit = 30;
            Main.mouseRightRelease = false;
            Recipe.FindRecipes();
        }
    }

    //in Terraria.Main.DrawPlayerHead after if statement that sets flag2 to true
    //  call ItemLoader.DrawHair(drawPlayer, ref flag, ref flag2)
    //in Terraria.Main.DrawPlayer after if statement that sets flag5 to true
    //  call ItemLoader.DrawHair(drawPlayer, ref flag4, ref flag5)
    internal static void DrawHair(Player player, ref bool drawHair, ref bool drawAltHair)
    {
        Item item = player.armor[10].headSlot >= 0 ? player.armor[10] : player.armor[0];
        if(IsModItem(item))
        {
            item.modItem.DrawHair(ref drawHair, ref drawAltHair);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.DrawHair(item, ref drawHair, ref drawAltHair);
            }
        }
    }

    //in Terraria.Main.DrawPlayerHead in if statement after ItemLoader.DrawHair
    //and in Terraria.Main.DrawPlayer in if (!drawPlayer.invis && drawPlayer.head != 38 && drawPlayer.head != 135)
    //  use && with ItemLoader.DrawHead(drawPlayer.armor[0])
    internal static bool DrawHead(Player player)
    {
        Item item = player.armor[10].headSlot >= 0 ? player.armor[10] : player.armor[0];
        if(IsModItem(item) && !item.modItem.DrawHead())
        {
            return false;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && !mod.globalItem.DrawHead(item))
            {
                return false;
            }
        }
        return true;
    }

    private static Item GetWing(Player player)
    {
        Item item = null;
        for(int k = 3; k < 8 + player.extraAccessorySlots; k++)
        {
            if (player.armor[k].wingSlot > 0)
            {
                item = player.armor[k];
            }
        }
        return item;
    }

    //in Terraria.Player.WingMovement after if statements that set num1-5
    //  call ItemLoader.VerticalWingSpeeds(this, ref num2, ref num5, ref num4, ref num3, ref num)
    internal static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
       ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        Item item = GetWing(player);
        if(item == null)
        {
            return;
        }
        if(IsModItem(item))
        {
            item.modItem.VerticalWingSpeeds(ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
                ref maxAscentMultiplier, ref constantAscend);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.VerticalWingSpeeds(item, ref ascentWhenFalling, ref ascentWhenRising,
                    ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
            }
        }
    }

    //in Terraria.Player.Update after wingsLogic if statements modifying accRunSpeed and runAcceleration
    //  call ItemLoader.HorizontalWingSpeeds(this)
    internal static void HorizontalWingSpeeds(Player player)
    {
        Item item = GetWing(player);
        if(item == null)
        {
            return;
        }
        if(IsModItem(item))
        {
            item.modItem.HorizontalWingSpeeds(ref player.accRunSpeed, ref player.runAcceleration);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.HorizontalWingSpeeds(item, ref player.accRunSpeed, ref player.runAcceleration);
            }
        }
    }

    //in Terraria.Item.UpdateItem before item movement (denoted by ItemID.Sets.ItemNoGravity)
    //  call ItemLoader.Update(this, ref num, ref num2)
    internal static void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if(IsModItem(item))
        {
            item.modItem.Update(ref gravity, ref maxFallSpeed);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.Update(item, ref gravity, ref maxFallSpeed);
            }
        }
    }

    //in Terraria.UI.ItemSlot.GetItemLight remove type too high check
    //in beginning of Terraria.Item.GetAlpha call
    //  Color? modColor = ItemLoader.GetAlpha(this, newColor);
    //  if(modColor.HasValue) { return modColor.Value; }
    internal static Color? GetAlpha(Item item, Color lightColor)
    {
        foreach (Mod mod in ModLoader.mods.Values)
        {
            if (mod.globalItem != null)
            {
                Color? color = mod.globalItem.GetAlpha(item, lightColor);
                if (color.HasValue)
                {
                    return color;
                }
            }
        }
        if (IsModItem(item))
        {
            return item.modItem.GetAlpha(lightColor);
        }
        return null;
    }

    //in Terraria.Main.DrawItem after ItemSlot.GetItemLight call
    //  if(!ItemLoader.PreDrawInWorld(item, Main.spriteBatch, color, alpha, ref rotation, ref scale)) { return; }
    internal static bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale)
    {
        bool flag = true;
        if(IsModItem(item) && !item.modItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale))
        {
            flag = false;
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null && !mod.globalItem.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale))
            {
                flag = false;
            }
        }
        return flag;
    }

    //in Terraria.Main.DrawItem before every return (including for PreDrawInWorld) and at end of method call
    //  ItemLoader.PostDrawInWorld(item, Main.spriteBatch, color, alpha, rotation, scale)
    internal static void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale)
    {
        if(IsModItem(item))
        {
            item.modItem.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale);
        }
        foreach(Mod mod in ModLoader.mods.Values)
        {
            if(mod.globalItem != null)
            {
                mod.globalItem.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale);
            }
        }
    }
}}
