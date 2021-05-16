using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using MonoMod.RuntimeDetour.HookGen;
using System.Reflection;
using Terraria.ID;
using System.IO;

namespace FascistTerraria
{
    public class TextureID
    {
        public const short FascistFlag = 0;
        public const short FascistEagle = 1;
    }
	public partial class FascistTerraria : Mod
	{
        public delegate void Orig_AddMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons);

        public delegate void Hook_AddMenuButtons(Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons);

        public static event Hook_AddMenuButtons On_AddMenuButtons
        {
            add
            {
                HookEndpointManager.Add<Hook_AddMenuButtons>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetMethod("AddMenuButtons", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
            }
            remove
            {
                HookEndpointManager.Remove<Hook_AddMenuButtons>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetMethod("AddMenuButtons", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
            }
        }
        public static void AddButton(string text, Action act, int selectedMenu, string[] buttonNames, ref int buttonIndex, ref int numButtons)
        {
            buttonNames[buttonIndex] = text;

            if (selectedMenu == buttonIndex)
            {
                Main.PlaySound(SoundID.MenuOpen);
                act();
            }

            buttonIndex++;
            numButtons++;
        }
        private bool stopTitleMusic;
        private ManualResetEvent titleMusicStopped;
        private int customTitleMusicSlot;
        private void ChangeMenuMusic(ILContext il)
        {
            var ilcursor = new ILCursor(il);
            var ilcursor2 = ilcursor;
            var moveType = MoveType.After;
            var array = new Func<Instruction, bool>[1];
            array[0] = i => i.MatchLdfld<Main>("newMusic");
            ilcursor2.GotoNext(moveType, array);
            ilcursor.EmitDelegate<Func<int, int>>(delegate (int newMusic)
            {
                if (newMusic != 6) return newMusic;
                return customTitleMusicSlot;
            });
        }
        public void ChangeMenuMusic()
        {
            customTitleMusicSlot = GetSoundSlot(SoundType.Music, "Sounds/Music/italysong");
            IL.Terraria.Main.UpdateAudio += new ILContext.Manipulator(ChangeMenuMusic);
        }
        public override void PreSaveAndQuit()
        {
            ChangeMenuMusic();
        }
        public override void Close()
        {
            var soundSlot2 = GetSoundSlot(SoundType.Music, "Sounds/Music/italysong");
            if (Main.music.IndexInRange(soundSlot2))
            {
                var musicIndex = Main.music[soundSlot2];
                if (musicIndex != null && musicIndex.IsPlaying) Main.music[soundSlot2].Stop(AudioStopOptions.Immediate);
            }
            base.Close();
        }

        public override void PostSetupContent()
        {
            ChangeMenuMusic();
        }

        #region Instance Variables

        private readonly Texture2D[] cachedLogoTextures = new Texture2D[2];

        private readonly Texture2D[] myTextures = new Texture2D[2];

        #endregion

        public override void Load()
        {
            On.Terraria.Main.DrawMenu += Main_DrawMenu;
            // Main.ChangeGameTitle()
            On_AddMenuButtons += FascistTerraria_On_AddMenuButtons;
            On.Terraria.Lang.GetRandomGameTitle += Lang_GetRandomGameTitle;
            Main.chTitle = true;

            cachedLogoTextures[0] = Main.logoTexture;
            cachedLogoTextures[1] = Main.logo2Texture;
            myTextures[TextureID.FascistFlag] = GetTexture("Pictures/fascismflag");
            myTextures[TextureID.FascistEagle] = GetTexture("Pictures/eagle");

            Main.logoTexture = myTextures[TextureID.FascistEagle];
            Main.logo2Texture = myTextures[TextureID.FascistEagle];
        }

        private void FascistTerraria_On_AddMenuButtons(Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
        {
            orig(main, selectedMenu, buttonNames, buttonScales, ref offY, ref spacing, ref buttonIndex, ref numButtons);

            buttonNames[0] = "My Player";
            buttonNames[1] = "My Players";
            buttonNames[2] = "My Mods";
            buttonNames[3] = "My Mod Sources";
            buttonNames[4] = "My Mod browser";
            AddButton("Wipe Useless Ideologies From Computer", delegate
            {
                string path = Path.Combine(ModLoader.ModPath, "CommunistTerraria.tmod");
                if (File.Exists(path))
                    File.Delete(path);

                string path2 = Path.Combine(ModLoader.ModPath, "CapitalismTerraria.tmod");
                if (File.Exists(path2))
                    File.Delete(path2);
            }, selectedMenu, buttonNames, ref buttonIndex, ref numButtons);
        }
        private string Lang_GetRandomGameTitle(On.Terraria.Lang.orig_GetRandomGameTitle orig)
        {
            switch (Main.rand.Next(12))
            {
                case 1:
                    return "Fascist Terraria: One ruler rules superior.";
                case 2:
                    return "Fascist Terraria: Demonstating power since the '40s.";
                case 3:
                    return "Fascist Terraria: Mussolini, our beloved.";
                case 4:
                    return "Fascist Terraria: Stay in your place.";
                case 5:
                    return "Fascist Terraria: Never step out of line.";
                case 6:
                    return "Fascist Terraria: 'Democracy is beautiful in theory; in practice it is a fallacy. You in America will see that some day.'";
                case 7:
                    return "Fascist Terraria: 'Every anarchist is a baffled dictator.'";
                case 8:
                    return "Fascist Terraria: Trust only yourself.";
                case 9:
                    return "Fascist Terraria: Get to work!";
                case 10:
                    return "Fascist Terraria: One man rules all.";
                default:
                    return "Fascist Terraria: Power among men.";
            }
        }

        private void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            var sb = Main.spriteBatch;

            if (TextureExists("Pictures/fascismflag") && myTextures[TextureID.FascistFlag] != null) sb.Draw(myTextures[TextureID.FascistFlag], new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            orig(self, gameTime);
        }

        public override void Unload()
        {
            Main.chTitle = true;
            Main.logoTexture = cachedLogoTextures[0];
            Main.logo2Texture = cachedLogoTextures[1];

            myTextures[TextureID.FascistFlag] = null;
            myTextures[TextureID.FascistEagle] = null;

            var evt = titleMusicStopped;
            if (evt != null) evt.Set();

            titleMusicStopped = null;
        }
        public override void UpdateMusic(ref int music)
        {
            if (stopTitleMusic || !Main.gameMenu && customTitleMusicSlot != 6 && Main.ActivePlayerFileData != null && Main.ActiveWorldFileData != null)
            {
                if (!stopTitleMusic)
                    music = 6;
                else
                    stopTitleMusic = true;

                customTitleMusicSlot = 6;
                var music2 = GetMusic("Sounds/Music/italysong");
                if (music2.IsPlaying) music2.Stop(AudioStopOptions.Immediate);

                titleMusicStopped?.Set();
                stopTitleMusic = false;
            }
        }
    }
}