using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Starlite.Entities;
using Microsoft.Xna.Framework.Graphics;
using Starlite.Map;
using Starlite.Input;
using Starlite.Rendering;

namespace Starlite.Entities
{
    // TODO aliens don't attack when on walls, enemy hit sound fx, potential shooting alien, bloom
    public class Scene
    {
        // How many tiles should we get surrounding the player's current tile position
        private const int CollisionRadius = 3;

        public static Vector2 RespawnPoint;

        public Player player;

        private Room bossRoom;
        public Boss boss;

        // The list of every entity (e.g., player, different enemies, etc.) in the scene
        private readonly List<ICollidable> collidables = new List<ICollidable>();
        private readonly List<Entity> entities = new List<Entity>();

        private readonly List<Bullet> bullets = new List<Bullet>();

        private readonly MapGenerator map;
        private Vector2 offset;
        public static int ScreenWidth, ScreenHeight;

        public Vector2 effectsOffset;

        private Random rand;

        private String[] SceneFx;
        private String currentFX;

        private Effect vignetteEffect;
        private bool LoadedSounds;
        public static SoundEffect bulletFX;
        private SfxManager sfxManager;
        private CameraEffects cameraEffects;
        public Scene(Texture2D[] space, Dictionary<string, Texture2D> textures, Dictionary<string, SoundEffect> sounds)
        {
            Room.openDoorFx = sounds["doorOpenSound"];// doorOpenSound;
            Room.closeDoorFx = sounds["doorCloseSound"];
            Room.TileSet = textures["tileSet"];
            Room.DoorSet = textures["door"];
            Room.SideDoorSet = textures["sideDoor"];
            MapGenerator.space = space;
            MapGenerator.meleeAlien = textures["meleeAlien"];
            Enemy.attackAni = textures["alien1Attack"];
            
            this.map = new MapGenerator(100, 100);
            Room playerRoom = map.SelectPlayerRoom();
            RespawnPoint = new Vector2(playerRoom.centerTile.Position.X - MapGenerator.TileSize / 2, playerRoom.centerTile.Position.Y - MapGenerator.TileSize / 2);
            Vector2 zeroRoomCenter = new Vector2(playerRoom.centerTile.Position.X - MapGenerator.TileSize / 2, playerRoom.centerTile.Position.Y - MapGenerator.TileSize / 2);
            rand = new Random();

            this.cameraEffects = new CameraEffects(this);


            SceneFx = new string[] { "calm1", "calm2", "action1", "action2" };
            currentFX = "calm1";

            this.player = new Player(this, null, new Rectangle(0, 0, 32, 32), 1.0f, 1,
                RespawnPoint + new Vector2(), 0.0f, 0.1f, 25.0f, 10.0f);

            this.collidables.Add(player);
            this.entities.Add(player);

            foreach (Room r in map.Rooms)
            {
                foreach (Tile door in r.doors)
                    door.TileType = TileType.Impassable;
            }
            bossRoom = map.SelectBossRoom(playerRoom);
            map.FillWithAliens(5, 10, this, entities, collidables, player, playerRoom, bossRoom);
            this.boss = new Boss(this, null, new Rectangle(0, 0, 128, 128), 1f, 1, bossRoom.centerTile.Position, 0f, 2f, 35f, 5f, 1000, 3, 2000, player, bossRoom);
            this.entities.Add(boss);
            this.collidables.Add(boss);
            //bossRoom.Locked = true;
        }

        public void LoadTextures(Dictionary<String, Texture2D> textures)
        {
            boss.LoadTextures(textures["boss"]);
            player.LoadTextures(textures["player"], textures["cannon"]);
            Bullet.BulletTexture = textures["cannonBullet"];
        }

        public void LoadSounds(SoundEffect[] sceneSounds, SoundEffect[] playerSounds, SoundEffect[] mapSounds)
        {
            bulletFX = mapSounds[1];
            sfxManager = new SfxManager(SceneFx, sceneSounds, new float[] { 0.1f, 0.1f , 0.1f , 0.1f});
            player.LoadSounds(playerSounds);
            LoadedSounds = true;
        }


        public void LoadEffects(Effect vignetteEffect)
        {
            this.vignetteEffect = vignetteEffect;
        }

        public void SetTrack(string track)
        {
            if (track.Equals("action") && !currentFX.Equals("action1") &&!currentFX.Equals("action2"))
            {
                currentFX = rand.Next(10) > 4 ? "action1" : "action2";
            }
            else if (track.Equals("calm") && !currentFX.Equals("calm1") &&!currentFX.Equals("calm2"))
            {
                currentFX = rand.Next(10) > 4 ? "calm1" : "calm2";
            }
        }

        public string GetTrack() => currentFX.Equals("action1") || currentFX.Equals("action2") ? "action" : "calm";
        public void UpdateFX(float deltaTime)
        {
            this.cameraEffects.Update(deltaTime);

            if (LoadedSounds)
                if (sfxManager.UpdateSfx(currentFX, true, true))
                    currentFX = rand.Next(10) > 4 ? "calm1" : "calm2";
        }

        private Vector2 GetMousePosition()
        {
            var pos = InputManager.GetMousePosition();

            pos.X /= ScreenWidth;
            pos.Y /= ScreenHeight;

            pos.X *= 2.0f;
            pos.X -= 1.0f;

            pos.Y *= 2.0f;
            pos.Y -= 1.0f;

            return pos;
        }

        public void Update(float deltaTime)
        {
            if (Enemy.PlayerNearby)
                SetTrack("action");
            else
                SetTrack("calm");
            Enemy.PlayerNearby = false;
            UpdateFX(deltaTime);
            this.player.Update(deltaTime);

            this.offset = this.player.Position - new Vector2(ScreenWidth / 2, ScreenHeight / 2) + this.effectsOffset + this.GetMousePosition() * 20.0f;
            
            map.Update(player);
            // This is the core of the collision system and therefore all that is required for it to function
            foreach (var collidable in this.collidables)
            {
                var position = this.FindCollidablePosition(collidable);
                var surroundingTiles = this.GetSurroundingTiles(position);
                Depentrate(collidable, surroundingTiles);
            }

            var bulletsToRemove = new List<Bullet>();
            var entitiesToRemove = new List<Entity>();
            
            // This is the core of the collision system and therefore all that is required for it to function
            foreach (var collidable in this.collidables)
            {
                if (collidable is Bullet bullet)
                {
                    bullet.Update(deltaTime);

                    var shouldDestroyBullet = BulletTileIntersection(bullet);
                    if (shouldDestroyBullet)
                    {
                        bulletsToRemove.Add(bullet);
                        continue;
                    }

                    var collidingEntity = BulletIntersection(collidable.GetWorldPosition(), collidable.GetBoundingBox(),
                        bullet.ignore);
                    if (collidingEntity != null)
                    {
                        if (!(collidingEntity is Enemy && bullet.bossBullet))
                        {
                            if (!collidingEntity.IsInvincible && collidingEntity.recent != bullet)
                            {
                                collidingEntity.recent = bullet;
                                collidingEntity.Damage(bullet.damage);
                            }

                            if (!bullet.penetrating)
                                bulletsToRemove.Add(bullet);
                        }

                        continue;
                    }
                }
                else
                {
                    var position = this.FindCollidablePosition(collidable);
                    var surroundingTiles = this.GetSurroundingTiles(position);
                    Depentrate(collidable, surroundingTiles);
                }
            }

            foreach (var bullet in bulletsToRemove)
            {
                this.collidables.Remove(bullet);
                this.bullets.Remove(bullet);
            }

            foreach (var entity in entities)
            {
                if (!(entity is Player))
                {
                    if (entity.IsDead)
                    {
                        entitiesToRemove.Add(entity);
                    }
                    else
                    {
                        entity.Update(deltaTime);
                    }
                }
            }

            foreach (var entity in entitiesToRemove)
            {
                entities.Remove(entity);
                collidables.Remove(entity as ICollidable);
            }
            
            int emptyRooms = 0;
            foreach (Room room in map.Rooms)
            {
                if (room.alienCount == 0)
                    emptyRooms++;
            }
        }

        // Convert the entity's world coordinates to 
        private TilePosition FindCollidablePosition(ICollidable collidable)
        {
            var pos = collidable.GetWorldPosition() / MapGenerator.TileSize;

            return new TilePosition((int)pos.X, (int)pos.Y);
        }

        private Tile[] GetSurroundingTiles(TilePosition position)
        {
            var tiles = new List<Tile>();

            var mapLengthX = this.map.Tiles.GetLength(0);
            var mapLengthY = this.map.Tiles.GetLength(1);

            var startingPositionX = Util.Clamp(position.X - CollisionRadius, 0, mapLengthX);
            var endingPositionX = Util.Clamp(position.X + CollisionRadius, 0, mapLengthX);

            var startingPositionY = Util.Clamp(position.Y - CollisionRadius, 0, mapLengthY);
            var endingPositionY = Util.Clamp(position.Y + CollisionRadius, 0, mapLengthY);

            for (var x = startingPositionX; x < endingPositionX; x++)
            {
                for (var y = startingPositionY; y < endingPositionY; y++)
                {
                    var tile = this.map.Tiles[x, y];
                    if (tile.TileType == TileType.Impassable)
                        tiles.Add(tile);
                }
            }

            return tiles.ToArray();
        }

        private static void Depentrate(ICollidable collidable, Tile[] tiles)
        {
            foreach (var tile in tiles)
            {
                // Gets the normalized, raw vector showing the direction between the tile position and the entity position
                var direction = (collidable.GetWorldPosition() - tile.Position).Normalized();

                var angle = MathHelper.ToDegrees((float)Math.Atan2(direction.Y, direction.X));

                const float angleBias = 5.0f;

                Vector2 cardinalDirection;
                if (angle > 135 + angleBias || angle < -135 - angleBias)
                    cardinalDirection = new Vector2(-1, 0);
                else if (angle < 135 - angleBias && angle > 45 + angleBias)
                    cardinalDirection = new Vector2(0, 1);
                else if (angle < 45 - angleBias && angle > -45 + angleBias)
                    cardinalDirection = new Vector2(1, 0);
                else if (angle > -135 + angleBias && angle < -45 - angleBias)
                    cardinalDirection = new Vector2(0, -1);
                else
                    cardinalDirection = new Vector2();

                // Rectangle of tile
                var tileRect = new Rectangle((int)tile.Position.X, (int)tile.Position.Y, MapGenerator.TileSize,
                    MapGenerator.TileSize);

                // Generates a new rectangle given the intersection of the entity and tile
                Rectangle intersection;
                if (collidable.GetBoundingBox().Intersects(tileRect))
                    intersection = Rectangle.Intersect(collidable.GetBoundingBox(), tileRect);
                else
                    intersection = Rectangle.Empty;

                // Calculates exactly how much we should move the entity in order to not intersect with the tile
                var depenetration = new Vector2(intersection.Width, intersection.Height) * cardinalDirection;

                // Performs the movement of the entity
                collidable.Depentrate(depenetration);
            }
        }

        private Entity BulletIntersection(Vector2 position, Rectangle size, ICollidable ignore)
        {
            var bulletRectangle = new Rectangle((int)position.X, (int)position.Y, size.Width, size.Height);

            foreach (var collidable in collidables)
            {
                if (!(collidable is Enemy) && !(collidable is Player)) continue;
                if (collidable == ignore) continue;

                //var collidableRectangle = new Rectangle((int)collidable.GetWorldPosition().X,
                //    (int)collidable.GetWorldPosition().Y, collidable.GetBoundingBox().Width,
                //    collidable.GetBoundingBox().Height);
                Rectangle[] hitboxes = collidable.GetHitboxes();
                foreach (Rectangle hitbox in hitboxes)
                {
                    if (bulletRectangle.Intersects(hitbox))
                    {
                        return collidable as Entity;
                    }
                }
            }

            return null;
        }

        private bool BulletTileIntersection(ICollidable bullet)
        {
            var bulletPosition = FindCollidablePosition(bullet);
            var surroundingTiles = GetSurroundingTiles(bulletPosition);
            foreach (var tile in surroundingTiles)
            {
                var tileRect = tile.GetBox();//new Rectangle(tile.PositionX, tile.PositionY, MapGenerator.TileSize   ,MapGenerator.TileSize);
                var bulletRect = bullet.GetBoundingBox(bullet.GetWorldPosition());
                if (tileRect.Intersects(bulletRect))
                    return true;
            }

            return false;
        }

        public void SpawnBullet(float damage, Vector2 direction, float speed, ICollidable ignore,
                                Rectangle bulletFrame, Vector2 offset, float scale, bool penetrating)
        {
            SoundEffectInstance sound = bulletFX.CreateInstance();
            sound.Pitch = penetrating ? -0.5f : (float)(rand.NextDouble() * 0.3);
            var bullet = new Bullet(damage, direction, speed, ignore, bulletFrame,
                scale, 1, ignore.GetWorldPosition() + offset, 0.0f, sound, penetrating);
            this.bullets.Add(bullet);
            this.collidables.Add(bullet);
            int strength = 1;
            if (penetrating)
                strength = 3;
            cameraEffects.PerformCameraShake(strength, 0.2f);
        }

        public void Shake(float strength, float duration)
        {
            cameraEffects.PerformCameraShake(strength, duration);
        }
        public void SpawnBullet(Bullet bullet)
        {
            this.bullets.Add(bullet);
            this.collidables.Add(bullet);
        }

        public void Draw(SpriteBatch batch)
        {
            this.map.Draw(batch, offset);

            foreach (var entity in entities)
            {
                if (!(entity is Player))
                    entity.Draw(batch, offset);
            }

            foreach (var bullet in bullets)
                bullet.Draw(batch, offset);

            player.Draw(batch, offset, this.vignetteEffect);

        }
    }
}