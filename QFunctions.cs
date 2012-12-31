﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TShockAPI;
using Terraria;
using ChatAssistant;

namespace QuestSystemLUA
{
    public class QFunctions
    {
        public static bool AtXY(int x, int y, QPlayer Player, int radius = 1)
        {
            Rectangle rec, playerrec;
            rec = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
            playerrec = new Rectangle((int)Player.TSPlayer.X / 16, (int)Player.TSPlayer.Y / 16, 1, 1);
            return rec.Intersects(playerrec);
        }
        public static void TileEdit(int x, int y, string tile)
        {
            byte type;

            if (QTools.GetTileTypeFromName(tile, out type))
            {
                if (type < 253)
                {
                    Main.tile[x, y].type = (byte)type;
                    Main.tile[x, y].active = true;
                    Main.tile[x, y].liquid = 0;
                    Main.tile[x, y].skipLiquid = true;
                    Main.tile[x, y].frameNumber = 0;
                    Main.tile[x, y].frameX = -1;
                    Main.tile[x, y].frameY = -1;
                }
                else if (type == 253)
                {
                    Main.tile[x, y].active = false;
                    Main.tile[x, y].skipLiquid = false;
                    Main.tile[x, y].lava = false;
                    Main.tile[x, y].liquid = 255;
                    Main.tile[x, y].checkingLiquid = false;
                }
                else if (type == 254)
                {
                    Main.tile[x, y].active = false;
                    Main.tile[x, y].skipLiquid = false;
                    Main.tile[x, y].lava = true;
                    Main.tile[x, y].liquid = 255;
                    Main.tile[x, y].checkingLiquid = false;
                }
                if ((Main.tile[x, y].type == 53) || (Main.tile[x, y].type == 253) || (Main.tile[x, y].type == 254))
                    WorldGen.SquareTileFrame(x, y, false);
                QTools.UpdateTile(x, y);
            }
            else
                throw new Exception("Invalid Tile Name");
        }
        public static void WallEdit(int x, int y, string wall)
        {
            byte type;

            if (QTools.GetWallTypeFromName(wall, out type))
            {
                if (type < 255)
                {
                    Main.tile[x, y].wall = (byte)type;
                }
                QTools.UpdateTile(x, y);
            }
            else
                throw new Exception("Invalid Wall Name");
        }
        public static void DeleteBoth(int x, int y)
        {
            Main.tile[x, y].active = false;
            Main.tile[x, y].wall = 0;
            Main.tile[x, y].skipLiquid = true;
            Main.tile[x, y].liquid = 0;
            QTools.UpdateTile(x, y);
        }
        public static void DeleteWall(int x, int y)
        {
            Main.tile[x, y].wall = 0;
            QTools.UpdateTile(x, y);
        }
        public static void DeleteTile(int x, int y)
        {
            Main.tile[x, y].active = false;
            Main.tile[x, y].skipLiquid = true;
            Main.tile[x, y].liquid = 0;
            QTools.UpdateTile(x, y);
        }
        public static void Sleep(int time)
        {
            Thread.Sleep(time);
        }
        public static void Teleport(int x, int y, QPlayer Player)
        {
            Player.TSPlayer.Teleport(x, y + 3);
        }
        public static void ClearKillList(QPlayer Player)
        {
            lock (Player.KillNames)
                Player.KillNames.Clear();
        }
        public static void GoCollectItem(string name, int amount, QPlayer Player)
        {
            int count;
            do
            {
                count = 0;
                try
                {
                    foreach (Item slot in Player.Inventory)
                    {
                        if (slot != null)
                            if (slot.name.ToLower() == name.ToLower())
                                count += slot.stack;
                    }
                }
                catch (Exception e)
                {
                    Log.Info(e.Message);
                }
                Thread.Sleep(1);
            }
            while (count < amount);
        }
        public static void TakeItem(string qname, string iname, int amt, QPlayer Player)
        {
            if (amt > 0)
            {
                var aitem = new AwaitingItem(qname, amt, iname);
                Player.AwaitingItems.Add(aitem);
                if (amt > 1)
                    Player.TSPlayer.SendInfoMessage(string.Format("Please drop {0} {1}s, The excess will be returned.", amt, iname));
                else
                    Player.TSPlayer.SendInfoMessage(string.Format("Please drop {0} {1}, The excess will be returned.", amt, iname));
                while (Player.AwaitingItems.Contains(aitem)) { Thread.Sleep(1); }
            }
        }
        public static int GetRegionTilePercentage(string tiletype, string regionname)
        {
            double amountofmatchedtiles = 0;
            double totaltilecount = 0;
            TShockAPI.DB.Region r;
            byte type;
            if (QTools.GetTileTypeFromName(tiletype, out type))
            {
                if ((r = TShock.Regions.ZacksGetRegionByName(regionname)) != null)
                {
                    for (int i = r.Area.X; i < (r.Area.X + r.Area.Width); i++)
                    {
                        for (int j = r.Area.Y; j < (r.Area.Y + r.Area.Height); j++)
                        {
                            if (Main.tile[i, j].active && Main.tile[i, j].type == type )
                                amountofmatchedtiles++;
                            totaltilecount++;
                        }
                    }
                }
            }
            if (totaltilecount != 0)
                return (int)((amountofmatchedtiles / totaltilecount) * 100);
            return 0;
        }
        public static int GetXYTilePercentage(string tiletype, int X, int Y, int Width, int Height)
        {
            double amountofmatchedtiles = 0;
            double totaltilecount = 0;
            byte type;
            if (QTools.GetTileTypeFromName(tiletype, out type))
            {
                for (int i = X; i < (X + Width); i++)
                {
                    for (int j = Y; j < (Y + Height); j++)
                    {
                        if (Main.tile[i, j].active && Main.tile[i, j].type == type)
                            amountofmatchedtiles++;
                        totaltilecount++;
                    }
                }
            }
            if (totaltilecount != 0)
                return (int)((amountofmatchedtiles / totaltilecount) * 100);
            return 0;
        }
        public static int GetRegionWallPercentage(string walltype, string regionname)
        {
            double amountofmatchedwalls = 0;
            double totalwallcount = 0;
            TShockAPI.DB.Region r;
            byte type;
            if (QTools.GetWallTypeFromName(walltype, out type))
            {
                if ((r = TShock.Regions.ZacksGetRegionByName(regionname)) != null)
                {
                    for (int i = r.Area.X; i < (r.Area.X + r.Area.Width); i++)
                    {
                        for (int j = r.Area.Y; j < (r.Area.Y + r.Area.Height); j++)
                        {
                            if (Main.tile[i, j].active && Main.tile[i, j].wall == type)
                                amountofmatchedwalls++;
                            totalwallcount++;
                        }
                    }
                }
            }
            if (totalwallcount != 0)
                return (int)((amountofmatchedwalls / totalwallcount) * 100);
            return 0;
        }
        public static int GetXYWallPercentage(string walltype, int X, int Y, int Width, int Height)
        {
            double amountofmatchedwalls = 0;
            double totalwallcount = 0;
            byte type;
            if (QTools.GetWallTypeFromName(walltype, out type))
            {
                for (int i = X; i < (X + Width); i++)
                {
                    for (int j = Y; j < (Y + Height); j++)
                    {
                        if (Main.tile[i, j].wall == type)
                            amountofmatchedwalls++;
                        totalwallcount++;
                    }
                }
            }
            if (totalwallcount != 0)
                return (int)((amountofmatchedwalls / totalwallcount) * 100);
            return 0;
        }
        //Below = New in V1.2
        //Fixed/Working
        public static void Give(string name, QPlayer Player, int amount = 1, int prefix = 0)
        {
            Main.rand = new Random();
            Item item = TShock.Utils.GetItemByName(name)[0];
            Player.TSPlayer.GiveItem(item.type, item.name, item.width, item.height, amount, prefix != 0 ? prefix : 0);
        } //In Wiki
        public static void Private(string message, QPlayer Player, Color color)
        {
            Player.TSPlayer.SendMessage(message, color);
        } //In Wiki
        public static void Broadcast(string message, Color color)
        {
            TShock.Utils.Broadcast(message, color);
        } //In Wiki
        public static void StartQuest(string qname, QPlayer Player)
        {
            Player.NewQuest(QTools.GetQuestByName(qname), true);
        }
        public static string ReadNextChatLine(QPlayer Player, bool hide = false)
        {
            Player.AwaitingChat = true;
            Player.HideChat = hide;
            while (Player.AwaitingChat) {
                Thread.Sleep(1);    
            }
            Player.HideChat = false;
            return Player.LastChatMessage;
        }
        public static void Kill(string name, QPlayer Player, int amount = 1)
        {
            var npc = TShock.Utils.GetNPCByName(name);
            if (npc.Count == 1)
            {
                string naem = npc[0].name;
                for (int i = 0; i < amount; i++)
                {
                    Player.KillNames.Add(naem);
                }
                Player.AwaitingKill = true;
                while (Player.KillNames.Contains(naem))
                {
                    Thread.Sleep(1); 
                }
                Player.AwaitingKill = false;
            }
                /*Player.AwaitingKill = true;
                while (!Player.KillNames.Contains(name)) { Thread.Sleep(1); }
                Player.KillNames.Remove(name);
                Player.AwaitingKill = false;*/
            //}
        } //In Wiki
        public static void KillNpc(int id)
        {
            Main.rand = new Random();
            Main.npc[id].StrikeNPC(99999, 0, 0);
            NetMessage.SendData((int)PacketTypes.NpcStrike, -1, -1, "", id, 99999, 0, 0);
        } //In Wiki
        public static List<int> SpawnMob(string name, int x, int y, int amount = 1)
        {
            List<int> Ids = new List<int>();
            NPC npc = TShock.Utils.GetNPCByName(name)[0];
            for (int i = 0; i < amount; i++)
            {
                int npcid;
                int spawnTileX;
                int spawnTileY;
                TShock.Utils.GetRandomClearTileWithInRange(x, y, 1, 1, out spawnTileX, out spawnTileY);
                npcid = QNPC.NewNPC(spawnTileX * 16, spawnTileY * 16, npc.type, 0);
                Main.npc[npcid].SetDefaults(npc.name);
                Main.npc[npcid].UpdateNPC(npcid);
                Ids.Add(npcid);
            }
            return Ids;
        } //In Wiki
        public static void SetNPCHealth(int id, int health)
        {
            Main.rand = new Random();
            Main.npc[id].life = health;
        } //In Wiki

        public static TileData2 GetTile(int x, int y)
        {
            TileData2 data = new TileData2();
            data.active = Main.tile[x, y].active;
            data.checkingLiquid = Main.tile[x, y].checkingLiquid;
            data.frameNumber = Main.tile[x, y].frameNumber;
            data.frameX = Main.tile[x, y].frameX;
            data.frameY = Main.tile[x, y].frameY;
            data.lava = Main.tile[x, y].lava;
            data.lighted = Main.tile[x, y].lighted;
            data.liquid = Main.tile[x, y].liquid;
            data.skipLiquid = Main.tile[x, y].skipLiquid;
            data.type = Main.tile[x, y].type;
            data.wall = Main.tile[x, y].wall;
            data.wallFrameNumber = Main.tile[x, y].wallFrameNumber;
            data.wallFrameX = Main.tile[x, y].wallFrameX;
            data.wallFrameY = Main.tile[x, y].wallFrameY;
            data.wire = Main.tile[x, y].wire;
            return data;
        }
        public static void SetTile(int x, int y, TileData2 data)
        {
            Main.tile[x, y].active = data.active;
            Main.tile[x, y].checkingLiquid = data.checkingLiquid;
            Main.tile[x, y].frameNumber = data.frameNumber;
            Main.tile[x, y].frameX = data.frameX;
            Main.tile[x, y].frameY = data.frameY;
            Main.tile[x, y].lava = data.lava;
            Main.tile[x, y].lighted = data.lighted;
            Main.tile[x, y].liquid = data.liquid;
            Main.tile[x, y].skipLiquid = data.skipLiquid;
            Main.tile[x, y].type = data.type;
            Main.tile[x, y].wall = data.wall;
            Main.tile[x, y].wallFrameNumber = data.wallFrameNumber;
            Main.tile[x, y].wallFrameX = data.wallFrameX;
            Main.tile[x, y].wallFrameY = data.wallFrameY;
            Main.tile[x, y].wire  = data.wire;
            QTools.UpdateTile(x, y);
        }
        public static bool CheckEmpty(int x, int y, bool tile = true)
        {
            if (tile == true)
            {
                if (Main.tile[x, y].type > 0)
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (Main.tile[x, y].wall > 0)
                {
                    return false;
                }
                return true;

            }
        }
        public static void BuffPlayer(string buffname, QPlayer Player, int time)
        {
            var buffs = TShock.Utils.GetBuffByName(buffname);
            if (buffs.Count == 1)
            {
                Player.TSPlayer.SetBuff(buffs[0], time*60);
            }
        }
        public static bool CheckDay()
        {
            if (Main.dayTime)
            {
                return true;
            }
            return false;
        }
        public static bool CheckTime(double time, int range, bool dayTime = true)
        {
            if (dayTime)
            {
                if (Main.time - range < Main.time && Main.time + range > Main.time && Main.dayTime)
                {
                    return true;
                }
                return false;
            }
            else 
            {
                if (Main.time - range < Main.time && Main.time + range > Main.time && Main.dayTime == false)
                {
                    return true;
                }
                return false;
            }
        }
        public static void HealPlayer(QPlayer Player)
        {
            /* Out of the TShock source.*/
            Item heart = TShock.Utils.GetItemById(58);
            Item star = TShock.Utils.GetItemById(184);
            for (int i = 0; i < 20; i++)
                Player.TSPlayer.GiveItem(heart.type, heart.name, heart.width, heart.height, heart.maxStack);
            for (int i = 0; i < 10; i++)
                Player.TSPlayer.GiveItem(star.type, star.name, star.width, star.height, star.maxStack);
        }
        public static void SetWire(int x, int y, bool wire = true, bool active = false)
        {
            Main.tile[x, y].wire = wire;
            Main.tile[x, y].active = active;
            if (active)
                WorldGen.TripWire(x, y);
            QTools.UpdateTile(x, y);
        }
        public static void SetTileType(int x, int y, byte type, short frameX = 0, short frameY = 0)
            //Offers a more specific function than TileEdit(). You can set frames using this function and so can create multitile objects like plants, furniture, and boulders. Also, this function uses direct IDs while TileEdit() goes through a list lookup first.
        {
            Main.tile[x, y].type = type;
            Main.tile[x, y].frameX = frameX;
            Main.tile[x, y].frameY = frameY;
            QTools.UpdateTile(x, y);
        }
        public static void AddParty(QPlayer Player, string partyname)
        {
            if (QMain.QuestParties.Count > 0)
            {
                foreach (QuestParty pty in QMain.QuestParties)
                {
                    if (pty.PartyName == partyname)
                    {
                        pty.AddMember(Player);
                        Player.CurrentParties.Add(pty);
                    }
                }
            }
            else
            {
                QMain.QuestParties.Add(new QuestParty(partyname));
                foreach (QuestParty pty in QMain.QuestParties)
                {
                    if (pty.PartyName == partyname)
                    {
                        pty.AddMember(Player);
                        Player.CurrentParties.Add(pty);
                    }
                }
            }
        }
        public static void PartyHunt(QPlayer Player, string pty, string npc, int amt = 1)
        {
            QuestParty party = QTools.GetQuestPartyByName(pty);
            var name = TShock.Utils.GetNPCByName(npc);
            //if (Player.TSPlayer.Name == party.Leader)
            if (party.Expansion == true)
            {
                if (name.Count == 1)
                {
                    party.ObjComplete = false;
                    party.Hunt(name[0].name, amt);
                    party.Expansion = false;
                }
            }
            while (party.ObjComplete == false) { Thread.Sleep(1); }
        }
        /*public static void WaitForObjective(string pty)
        {
            QuestParty party = QTools.GetQuestPartyByName(pty);
            while (party.ObjComplete == false) { Thread.Sleep(1); }
        }*/
        public static void ChangeGroup(QPlayer Player, string group)
        {
            var user = TShock.Users.GetUserByID(Player.TSPlayer.UserID);
            TShock.Users.SetUserGroup(user, group);
        }
        public static void CreateMenu(QPlayer Player, string title, dynamic menu) // {text = "", value = "", selectable = ""}
        {
            List<MenuItem> newmenu = new List<MenuItem>();
            if (!Player.RunningPython)
            {
                LuaInterface.LuaTable menu2 = menu;
                foreach (LuaInterface.LuaTable item in menu2.Values)
                {
                    string textbuffer = "";
                    int valuebuffer = 0;
                    bool selectbuffer = false;
                    textbuffer = item["text"].ToString();
                    valuebuffer = Convert.ToInt16(item["value"]);
                    selectbuffer = Convert.ToBoolean(item["selectable"]);
                    newmenu.Add(new MenuItem(textbuffer, valuebuffer, selectbuffer, false));
                }
            }
            else
            {
                foreach (IronPython.Runtime.PythonDictionary item in menu)
                {
                    string textbuffer = "";
                    int valuebuffer = 0;
                    bool selectbuffer = false;
                    textbuffer = item["text"].ToString();
                    valuebuffer = Convert.ToInt16(item["value"]);
                    selectbuffer = Convert.ToBoolean(item["selectable"]);
                    newmenu.Add(new MenuItem(textbuffer, valuebuffer, selectbuffer, false));
                }
            }
            Chat.Menu menu3 = Chat.CreateMenu(Player.TSPlayer.Index, title, newmenu, new Chat.MenuAction(QTools.MenuCallback));
            //Section to wait until menu input happens.
            Player.MenuOption = -21546985; // Ok, I'm getting a little silly.
            bool decided = false;
            while (!decided)
            {
                if (Player.MenuOption != -21546985)
                {
                    decided = true;
                }
            }
        }
        public static void PartyHuntList(QPlayer Player, string pty, dynamic hunt)
        {
            QuestParty party = QTools.GetQuestPartyByName(pty);
            if (party.Expansion == true)
            {
                if (!Player.RunningPython)
                {
                    foreach (LuaInterface.LuaTable tbl in hunt.Values)
                    {
                        //This area is pretty shitty. For some reason I found trouble indexing a lua table, so I just resorted to doing this.
                        string name = "";
                        int amt = 0;
                        foreach (var val in tbl.Values)
                        {
                            if (val.GetType().ToString() == "System.String")
                            {
                                name = val.ToString();
                            }
                            else
                            {
                                amt = Convert.ToInt16(val);
                            }
                        }
                        party.Hunt(name, amt);
                    }
                    party.ObjComplete = false;
                    party.Expansion = false;
                }
                else
                {
                    foreach (IronPython.Runtime.List tbl in hunt)
                    {
                        party.Hunt(tbl[0].ToString(), Convert.ToInt16(tbl[1]));
                        party.ObjComplete = false;
                        party.Expansion = false;
                    }
                }
            }

            while (party.ObjComplete == false) { Thread.Sleep(1); }
        }
        public static string DirectoryIterate(string path, string pattern, bool alldir = false)
        {
            string[] lol = System.IO.Directory.GetFiles(path, pattern, (alldir ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly));
            string x = Newtonsoft.Json.JsonConvert.SerializeObject(lol);
            return x;
        }
        public static void RunFileDirectly(string path)
        {
            if (path.EndsWith(".lua"))
            {
                QMain.lua.DoFile(path);
            }
            else if (path.EndsWith(".py"))
            {
                QMain.pyEngine.ExecuteFile(path);
            }
        }
        public static void RunTShockCommand(QPlayer player, string commandText, bool serverRuns=false)
        {
        	if (serverRuns)
        		TShockAPI.Commands.HandleCommand(TShockAPI.TSPlayer.Server, commandText);
        	else
        		TShockAPI.Commands.HandleCommand(player.TSPlayer, commandText);
        }
        public static void ModifyVaultBalance(QPlayer player, int amount)
        {
        	Vault.Vault.ModifyBalance(player.TSPlayer.Name, amount);
        }
    }
}