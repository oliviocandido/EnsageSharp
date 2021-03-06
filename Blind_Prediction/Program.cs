using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;

namespace BlindPrediction {
    class Program {
        static Dictionary<Hero, float> HeroList = new Dictionary<Hero, float>();
        static Font Text;
        static float ScaleX, ScaleY;
        private static readonly Menu Menu = new Menu("Blind Prediction", "BlindPrediction", true);
        
        

        static void Main(string[] args) {
            Menu.AddItem(new MenuItem("toggle", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("predict", "Predict movements?").SetValue(true).SetTooltip("Predict movements, otherwise just show last known position."));
            Menu.AddToMainMenu();

            ScaleX = Drawing.Width / 1920f;
            ScaleY = Drawing.Height / 1080f;

            Text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription {
                   FaceName = "Arial",
                   Height = 15,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });
            Game.OnUpdate += Update;
            Drawing.OnDraw += Draw;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        static void Update(EventArgs e) {
            if (!Game.IsInGame) {
                if (HeroList.Any())
                    HeroList.Clear();
                return;
            }
            if (!Menu.Item("toggle").GetValue<bool>()) return;
            Hero MyHero = ObjectManager.LocalHero;

            // Save heroes to dictionary as they cannot be detected in fog (Format <Hero, Last Seen Time>) 
            foreach (Hero v in ObjectManager.GetEntities<Hero>().Where(x => x.Team != MyHero.Team)) {
                if (!HeroList.ContainsKey(v)) {
                    HeroList.Add(v, 0);
                }
            }

            // Update last seen time of all visible heroes
            var ValuesToUpdate = new List<Hero>();

            foreach (var v in HeroList) {
                if (v.Key.IsVisible) {
                    ValuesToUpdate.Add(v.Key);
                }
            }

            foreach (var item in ValuesToUpdate) {
                HeroList[item] = Game.GameTime;
            }
        }

        static void Draw(EventArgs e) {

            if (!Game.IsInGame) return;
            if (!Menu.Item("toggle").GetValue<bool>()) return;

            try {
                foreach (var v in HeroList) {
                    if (!v.Key.IsVisible && v.Key.IsAlive) {
                        if (Menu.Item("predict").GetValue<bool>())
                        {
                            Vector3 ePos = new Vector3(v.Key.NetworkPosition.X + (float)Math.Cos(v.Key.NetworkRotationRad) * v.Key.MovementSpeed * (Game.GameTime - v.Value),
                                                    v.Key.NetworkPosition.Y + (float)Math.Sin(v.Key.NetworkRotationRad) * v.Key.MovementSpeed * (Game.GameTime - v.Value),
                                                    v.Key.NetworkPosition.Z + 50);
                            if (v.Key.NetworkActivity != NetworkActivity.Move) ePos = v.Key.NetworkPosition;
                            Drawing.DrawRect(Drawing.WorldToScreen(ePos) - 25, new Vector2(50, 50), Drawing.GetTexture("materials/ensage_ui/miniheroes/" + v.Key.Name.Substring(v.Key.Name.LastIndexOf("hero_") + 5) + ".vmat"));
                        }
                        else
                        {
                            Vector3 ePos = new Vector3(v.Key.NetworkPosition.X,
                                                    v.Key.NetworkPosition.Y,
                                                    v.Key.NetworkPosition.Z);
                            if (v.Key.NetworkActivity != NetworkActivity.Move) ePos = v.Key.NetworkPosition;
                            Drawing.DrawRect(Drawing.WorldToScreen(ePos) - 25, new Vector2(50, 50), Drawing.GetTexture("materials/ensage_ui/miniheroes/" + v.Key.Name.Substring(v.Key.Name.LastIndexOf("hero_") + 5) + ".vmat"));
                        }
                        
                    }
                }
            } catch (Exception) {
                // Do nothing, HeroList has simply been modified during this foreach loop.
            }
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e) {
            Text.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args) {
            if (!Menu.Item("toggle").GetValue<bool>()) return;
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer) return;

            try {
                foreach (var v in HeroList) {
                    if (!v.Key.IsVisible && v.Key.IsAlive) {
                        if (Menu.Item("predict").GetValue<bool>())
                        {
                            Vector3 ePos =
                                new Vector3(
                                    v.Key.NetworkPosition.X +
                                    (float) Math.Cos(v.Key.NetworkRotationRad)*v.Key.MovementSpeed*
                                    (Game.GameTime - v.Value),
                                    v.Key.NetworkPosition.Y +
                                    (float) Math.Sin(v.Key.NetworkRotationRad)*v.Key.MovementSpeed*
                                    (Game.GameTime - v.Value),
                                    v.Key.NetworkPosition.Z + 50);
                            if (v.Key.NetworkActivity != NetworkActivity.Move) ePos = v.Key.NetworkPosition;
                            Text.DrawText(null,
                                FirstCharToUpper(v.Key.Name.Substring(v.Key.Name.LastIndexOf("hero_") + 5)),
                                (int) Math.Round(WorldToMiniMap(ePos).X, MidpointRounding.AwayFromZero) -
                                (int) (v.Key.Name.Substring(v.Key.Name.LastIndexOf("hero_") + 5).Length*3*ScaleX),
                                (int) Math.Round(WorldToMiniMap(ePos).Y, MidpointRounding.AwayFromZero) -
                                (int) (5*ScaleY),
                                Color.White);

                        }
                        else
                        {
                            Vector3 ePos =
                                new Vector3(
                                    v.Key.NetworkPosition.X,
                                    v.Key.NetworkPosition.Y,
                                    v.Key.NetworkPosition.Z);
                            if (v.Key.NetworkActivity != NetworkActivity.Move) ePos = v.Key.NetworkPosition;
                            Text.DrawText(null,
                                FirstCharToUpper(v.Key.Name.Substring(v.Key.Name.LastIndexOf("hero_") + 5)),
                                (int)Math.Round(WorldToMiniMap(ePos).X, MidpointRounding.AwayFromZero) -
                                (int)(v.Key.Name.Substring(v.Key.Name.LastIndexOf("hero_") + 5).Length * 3 * ScaleX),
                                (int)Math.Round(WorldToMiniMap(ePos).Y, MidpointRounding.AwayFromZero) -
                                (int)(5 * ScaleY),
                                Color.White);

                        }


                    }
                }
            } catch (Exception) {
                // Do nothing, HeroList has simply been modified during this foreach loop.
            }

        }

        static void Drawing_OnPostReset(EventArgs args) {
            Text.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args) {
            Text.OnLostDevice();
        }

        static Vector2 WorldToMiniMap(Vector3 Pos) {
            
            float MapLeft = -8000;
            float MapTop = 7350;
            float MapRight = 7500;
            float MapBottom = -7200;
            float MapWidth = Math.Abs(MapLeft - MapRight);
            float MapHeight = Math.Abs(MapBottom - MapTop);

            float _x = Pos.X - MapLeft;
            float _y = Pos.Y - MapBottom;

            float dx, dy, px, py;
            if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.7) {
                dx = 272f / 1920f * Drawing.Width;
                dy = 261f / 1080f * Drawing.Height;
                px = 11f / 1920f * Drawing.Width;
                py = 11f / 1080f * Drawing.Height;
            } else if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.5){
                dx = 267f / 1680f * Drawing.Width;
                dy = 252f / 1050f * Drawing.Height;
                px = 10f / 1680f * Drawing.Width;
                py = 11f / 1050f * Drawing.Height;
            } else {
                dx = 255f / 1280f * Drawing.Width;
                dy = 229f / 1024f * Drawing.Height;
                px = 6f / 1280f * Drawing.Width;
                py = 9f / 1024f * Drawing.Height;
            }
            float MinimapMapScaleX = dx / MapWidth;
            float MinimapMapScaleY = dy / MapHeight;

            float scaledX = Math.Min(Math.Max(_x * MinimapMapScaleX, 0), dx);
            float scaledY = Math.Min(Math.Max(_y * MinimapMapScaleY, 0), dy);

            float screenX = px + scaledX;
            float screenY = Drawing.Height - scaledY - py;

            return new Vector2((float)Math.Floor(screenX), (float)Math.Floor(screenY));

        }

        static string FirstCharToUpper(string input) {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}
