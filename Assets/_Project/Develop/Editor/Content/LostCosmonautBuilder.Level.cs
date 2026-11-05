using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Develop.Runtime.Gameplay.Dust;
using _Project.Develop.Runtime.Gameplay.Hero;
using _Project.Develop.Runtime.Gameplay.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Interaction.Actions;
using _Project.Develop.Runtime.Gameplay.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace _Project.Develop.Editor.Content
{
    // Мир из блоков: высоты задаются по столбцам, спрайт блока подбирается по соседям.
    public static partial class LostCosmonautBuilder
    {
        private const float BlockSize = 2f;
        private const float HeroHalfHeight = 0.82f;
        private const int GateColumn = 46;

        private sealed class LedgeMark
        {
            public int Column;
            public int Low;
            public int High;
            public bool IsUp;
        }

        private sealed class LevelLayout
        {
            public const int FirstColumn = -24;

            private readonly List<int> _heights = new();
            private readonly Dictionary<int, Vector2Int> _ceilings = new();

            public readonly List<LedgeMark> Ledges = new();
            public int Height;
            public int TunnelStart;
            public int TunnelEnd;

            public int Cursor => FirstColumn + _heights.Count;
            public int LastColumn => Cursor - 1;

            public int HeightAt(int column)
                => _heights[Mathf.Clamp(column - FirstColumn, 0, _heights.Count - 1)];

            public float SurfaceY(int column) => HeightAt(column) * BlockSize;
            public static float ColumnX(int column) => column * BlockSize + BlockSize * 0.5f;

            public bool IsSolid(int column, int row)
            {
                if (row < HeightAt(column)) return true;
                return _ceilings.TryGetValue(column, out Vector2Int ceiling) && row >= ceiling.x && row < ceiling.y;
            }

            public IEnumerable<KeyValuePair<int, Vector2Int>> Ceilings => _ceilings;

            public void Flat(int count)
            {
                for (int i = 0; i < count; i++)
                    _heights.Add(Height);
            }

            public void Step(int delta)
            {
                int before = Height;
                Height += delta;
                if (delta >= 2)
                    Ledges.Add(new LedgeMark { Column = Cursor, Low = before, High = Height, IsUp = true });
                else if (delta <= -3)
                    Ledges.Add(new LedgeMark { Column = Cursor, Low = Height, High = before, IsUp = false });
            }

            // Однообразная равнина с перепадами в блок и редкими уступами под тюк.
            public void Roll(int count, Random random, int ledgeEvery, int min, int max)
            {
                int end = Cursor + count;
                int sinceLedge = 0;
                while (Cursor < end)
                {
                    int run = Mathf.Min(random.Next(6, 14), end - Cursor);
                    Flat(run);
                    sinceLedge += run;
                    if (Cursor >= end) break;

                    if (ledgeEvery > 0 && sinceLedge >= ledgeEvery)
                    {
                        sinceLedge = 0;
                        int delta = Height + 3 <= max && (Height - 3 < min || random.Next(2) == 0)
                            ? random.Next(2) + 2
                            : -3;
                        Step(delta);
                    }
                    else
                    {
                        int delta = random.Next(2) == 0 ? 1 : -1;
                        if (Height + delta > max || Height + delta < min) delta = -delta;
                        Step(delta);
                    }
                }
            }

            public void Tunnel(int count, Random random)
            {
                const int airRows = 3;
                const int rock = 22;
                TunnelStart = Cursor;
                int end = Cursor + count;
                while (Cursor < end)
                {
                    int run = Mathf.Min(random.Next(8, 16), end - Cursor);
                    for (int i = 0; i < run; i++)
                    {
                        _ceilings[Cursor] = new Vector2Int(Height + airRows, Height + airRows + rock);
                        Flat(1);
                    }

                    if (Cursor < end)
                        Step(random.Next(2) == 0 ? 1 : -1);
                }
                TunnelEnd = Cursor - 1;
            }
        }

        private static LevelLayout CreateLayout()
        {
            var layout = new LevelLayout();
            var random = new Random(2036);

            // Скала слева закрывает путь: герою остаётся идти только вправо.
            layout.Height = 26;
            layout.Flat(-LevelLayout.FirstColumn);
            layout.Height = 0;

            // Место крушения и первые шаги.
            layout.Flat(18);
            layout.Step(1);
            layout.Flat(6);
            layout.Step(-1);
            layout.Flat(52);

            // Перепады в один блок учат прыжку.
            layout.Step(1);
            layout.Flat(8);
            layout.Step(-1);
            layout.Flat(14);
            layout.Step(1);
            layout.Flat(10);
            layout.Step(1);
            layout.Flat(8);

            // Первый подъём и первый спуск с тюком.
            layout.Step(3);
            layout.Flat(26);
            layout.Step(-3);
            layout.Flat(26);

            // Равнина: однообразие, облака и пыль.
            layout.Roll(320, random, 46, -3, 8);

            // Подход к горе и тоннель.
            int approachTarget = layout.Height;
            layout.Flat(30);
            layout.Tunnel(110, random);
            layout.Flat(12);

            // Вторая половина пути постепенно уводит вверх.
            layout.Roll(170, random, 42, approachTarget - 2, approachTarget + 8);
            layout.Roll(170, random, 36, approachTarget + 2, approachTarget + 14);

            // Финальный склон к линии видимости.
            for (int i = 0; i < 8; i++)
            {
                layout.Step(i % 2 == 0 ? 3 : 2);
                layout.Flat(8);
            }

            layout.Flat(18);
            layout.Flat(30);
            return layout;
        }

        [MenuItem("Tools/The Lost Cosmonaut/Content/7. Build Level")]
        public static void BuildLevel()
        {
            RequireEditMode();
            Scene scene = EditorSceneManager.OpenScene(GameplayScene, OpenSceneMode.Single);

            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name is "World" or "Interaction Examples")
                    Object.DestroyImmediate(root);

            GameObject setup = scene.GetRootGameObjects().First(root => root.name == "Player Setup");
            foreach (Transform child in setup.transform.Cast<Transform>().ToArray())
                if (child.name != "Player Spawn")
                    Object.DestroyImmediate(child.gameObject);

            LevelLayout layout = CreateLayout();
            var world = new GameObject("World");

            BuildTilemap(world.transform, layout);
            BuildBackground(world.transform, layout);
            BuildCrashSite(world.transform, layout);
            BuildLedges(world.transform, layout);
            BuildDustSources(world.transform, layout);
            BuildDecor(world.transform, layout);
            BuildStoryZones(world.transform, layout);
            BuildVictory(world.transform, layout);

            setup.transform.Find("Player Spawn").position = new Vector3(6f, HeroHalfHeight, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Level built: {layout.LastColumn} columns, {layout.Ledges.Count} ledges, " +
                      $"tunnel {layout.TunnelStart}-{layout.TunnelEnd}.");
        }

        private static void BuildTilemap(Transform parent, LevelLayout layout)
        {
            var gridObject = new GameObject("Blocks");
            gridObject.transform.SetParent(parent, false);
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(BlockSize, BlockSize, 0f);

            var mapObject = new GameObject("Ground");
            mapObject.transform.SetParent(gridObject.transform, false);
            Tilemap map = mapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = mapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = -5;
            renderer.mode = TilemapRenderer.Mode.Chunk;

            var positions = new List<Vector3Int>();
            var tiles = new List<TileBase>();
            var picker = new BlockPicker(layout);

            void Put(int column, int row)
            {
                positions.Add(new Vector3Int(column, row, 0));
                tiles.Add(picker.Pick(column, row));
            }

            const int depth = 12;
            for (int column = LevelLayout.FirstColumn; column <= layout.LastColumn; column++)
            {
                // Заполняем вниз до самого низкого соседа, чтобы под скалами и уступами не было пустот.
                int height = layout.HeightAt(column);
                int lowest = height;
                for (int near = column - 14; near <= column + 14; near++)
                    lowest = Mathf.Min(lowest, layout.HeightAt(near));
                for (int row = lowest - depth; row < height; row++)
                    Put(column, row);
            }

            foreach (KeyValuePair<int, Vector2Int> ceiling in layout.Ceilings)
                for (int row = ceiling.Value.x; row < ceiling.Value.y; row++)
                    Put(ceiling.Key, row);

            map.SetTiles(positions.ToArray(), tiles.ToArray());
            map.CompressBounds();

            BuildTunnelBackWall(gridObject.transform, layout);

            Rigidbody2D body = mapObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            TilemapCollider2D tileCollider = mapObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            CompositeCollider2D composite = mapObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
            composite.GenerateGeometry();
        }

        private static void BuildTunnelBackWall(Transform grid, LevelLayout layout)
        {
            var wallObject = new GameObject("Tunnel Back Wall");
            wallObject.transform.SetParent(grid, false);
            Tilemap wall = wallObject.AddComponent<Tilemap>();
            wall.color = new Color(0.62f, 0.62f, 0.68f, 1f);
            TilemapRenderer renderer = wallObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = -6;
            renderer.mode = TilemapRenderer.Mode.Chunk;

            Tile fill = LoadTile("fill_1");
            var cells = new List<Vector3Int>();
            foreach (KeyValuePair<int, Vector2Int> ceiling in layout.Ceilings)
                for (int row = layout.HeightAt(ceiling.Key) - 1; row <= ceiling.Value.x; row++)
                    cells.Add(new Vector3Int(ceiling.Key, row, 0));

            wall.SetTiles(cells.ToArray(), cells.Select(_ => (TileBase)fill).ToArray());
            wall.CompressBounds();
        }

        // Подбирает блок по соседям. Сейчас поверхность, стены, углы и столбы — это 7 типов блоков.
        private sealed class BlockPicker
        {
            private readonly LevelLayout _layout;
            private readonly Tile[] _surface = LoadVariants("surface", 5);
            private readonly Tile[] _ceiling = LoadVariants("ceiling", 5);
            private readonly Tile _fill = LoadTile("fill_1");
            private readonly Tile _fillFossilA = LoadTile("fill_2");
            private readonly Tile _fillFossilB = LoadTile("fill_3");
            private readonly Tile _cornerLeft = LoadTile("corner_left");
            private readonly Tile _cornerRight = LoadTile("corner_right");
            private readonly Tile _cap = LoadTile("cap");
            private readonly Tile _wallLeft = LoadTile("wall_left");
            private readonly Tile _wallRight = LoadTile("wall_right");
            private readonly Tile _wallLeftEnd = LoadTile("wall_left_end");
            private readonly Tile _wallRightEnd = LoadTile("wall_right_end");
            private readonly Tile _pillar = LoadTile("pillar");

            public BlockPicker(LevelLayout layout) => _layout = layout;

            public Tile Pick(int column, int row)
            {
                bool top = !_layout.IsSolid(column, row + 1);
                bool bottom = !_layout.IsSolid(column, row - 1);
                bool left = !_layout.IsSolid(column - 1, row);
                bool right = !_layout.IsSolid(column + 1, row);
                int hash = Hash(column, row);

                if (top && !bottom)
                {
                    if (left && right) return _cap;
                    if (left) return _cornerLeft;
                    if (right) return _cornerRight;
                    return _surface[hash % _surface.Length];
                }

                if (bottom)
                    return _ceiling[hash % _ceiling.Length];

                if (left && right) return _pillar;
                if (left) return _layout.IsSolid(column - 1, row - 1) ? _wallLeftEnd : _wallLeft;
                if (right) return _layout.IsSolid(column + 1, row - 1) ? _wallRightEnd : _wallRight;

                int roll = hash % 37;
                if (roll == 0) return _fillFossilA;
                return roll == 1 ? _fillFossilB : _fill;
            }

            private static int Hash(int column, int row)
                => Mathf.Abs(column * 73856093 ^ row * 19349663);
        }

        private static Tile[] LoadVariants(string prefix, int count)
            => Enumerable.Range(1, count).Select(index => LoadTile($"{prefix}_{index}")).ToArray();

        private static Tile LoadTile(string name)
        {
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>($"{Tiles}/{name}.asset");
            if (tile == null) throw new InvalidOperationException("Missing tile: " + name);
            return tile;
        }

        private static void BuildBackground(Transform parent, LevelLayout layout)
        {
            var background = new GameObject("Background").transform;
            background.SetParent(parent, false);

            Ridge(background, "Far ridge", "ridge_far", -200, 0.92f, 0.86f, -12f);
            Ridge(background, "Near ridge", "ridge_near", -150, 0.82f, 0.8f, -13f);

            // Красная точка на небе из макета места крушения.
            GameObject mars = SpriteObject(background, "Mars", Decor + "/mars.png", Vector3.zero, -250);
            Set(mars.AddComponent<ParallaxLayer>(), ("_followX", 0.985f), ("_followY", 1f),
                ("_offset", new Vector2(7f, 5.2f)));

            // Земля появляется только в самом конце пути.
            float endX = LevelLayout.ColumnX(VictoryColumn(layout));
            float summit = layout.SurfaceY(VictoryColumn(layout));
            GameObject earth = SpriteObject(background, "Earth", Decor + "/earth.png",
                new Vector3(endX + 7.5f, summit - 8f, 0f), -180);
            Set(earth.AddComponent<EarthHorizon>(),
                ("_startX", endX - 70f), ("_endX", endX),
                ("_hiddenY", summit - 12f), ("_visibleY", summit + 5f));
        }

        private static void Ridge(Transform parent, string name, string sprite, int order, float followX, float followY, float offsetY)
        {
            GameObject item = SpriteObject(parent, name, $"{Decor}/{sprite}.png", Vector3.zero, order);
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            float width = renderer.sprite.bounds.size.x;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(width * 5f, renderer.sprite.bounds.size.y);
            Set(item.AddComponent<ParallaxLayer>(), ("_followX", followX), ("_followY", followY),
                ("_repeatWidth", width), ("_offset", new Vector2(0f, offsetY)));
        }

        private static void BuildCrashSite(Transform parent, LevelLayout layout)
        {
            var site = new GameObject("Crash Site").transform;
            site.SetParent(parent, false);

            void Wreck(string name, int column, float angle, float sink)
            {
                GameObject item = SpriteObject(site, name, $"{Decor}/{name}.png",
                    new Vector3(LevelLayout.ColumnX(column), layout.SurfaceY(column) - sink, 0f), -8);
                item.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            Wreck("wreck_hull", 9, -4f, 0.35f);
            Wreck("wreck_nose", 27, 5f, 0.3f);
            Wreck("wreck_tail", 37, -7f, 0.4f);

            void Sparks(int column, float height, string name)
            {
                GameObject item = SpriteObject(site, name, Decor + "/sparks_1.png",
                    new Vector3(LevelLayout.ColumnX(column), layout.SurfaceY(column) + height, 0f), -7);
                Set(item.AddComponent<SpriteFrameLoop>(),
                    ("_frames", new Object[]
                    {
                        AssetDatabase.LoadAssetAtPath<Sprite>(Decor + "/sparks_1.png"),
                        AssetDatabase.LoadAssetAtPath<Sprite>(Decor + "/sparks_2.png"),
                        AssetDatabase.LoadAssetAtPath<Sprite>(Decor + "/sparks_3.png")
                    }));
            }

            Sparks(11, 2.2f, "Sparks hull");
            Sparks(12, 1.2f, "Sparks hull 2");
            Sparks(36, 2f, "Sparks tail");

            for (int i = 0; i < 9; i++)
            {
                int column = 4 + i * 5;
                SpriteObject(site, "Debris " + i, $"{Decor}/debris_{i % 3 + 1}.png",
                    new Vector3(LevelLayout.ColumnX(column), layout.SurfaceY(column) - 0.05f, 0f), -6);
            }

            Pickup(site, layout, HeroItem.Food, 14, Items + "/food_box.png", "Подобрать еду", "item_food");
            Pickup(site, layout, HeroItem.Tent, 21, Items + "/tent_pack.png", "Подобрать палатку", "item_tent");
            Pickup(site, layout, HeroItem.Oxygen, 31, Items + "/oxygen_tanks.png", "Подобрать баллоны", "item_oxygen");
            Pickup(site, layout, HeroItem.MagneticDevice, 42, Items + "/magnetic_device.png", "Подобрать МУ", "item_device");

            // Пока вещи не собраны, дальше герой не пойдёт.
            float gateX = GateColumn * BlockSize;
            var gate = new GameObject("Exploration Gate");
            gate.transform.SetParent(site, false);
            gate.transform.position = new Vector3(gateX, layout.SurfaceY(GateColumn) + 10f, 0f);
            BoxCollider2D blocker = gate.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(0.5f, 40f);
            Set(gate.AddComponent<ExplorationGate>(), ("_size", new Vector2(5f, 40f)), ("_blocker", blocker));
        }

        private static void Pickup(Transform parent, LevelLayout layout, HeroItem item, int column, string sprite,
            string text, string monologue)
        {
            float x = LevelLayout.ColumnX(column);
            float y = layout.SurfaceY(column);

            var root = new GameObject("Pickup " + item);
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(x, y, 0f);
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 2f);
            collider.offset = new Vector2(0f, 1f);

            GameObject visual = SpriteObject(root.transform, "Visual", sprite, root.transform.position, -3);
            visual.transform.localScale = Vector3.one * 1.1f;

            Transform point = Child(root.transform, "InteractionPoint", new Vector3(x, y + 1f, 0f));
            Transform actions = Child(root.transform, "SuccessActions", root.transform.position);
            actions.gameObject.AddComponent<PickupAction>();

            PickupInteractable pickup = root.AddComponent<PickupInteractable>();
            Set(pickup, ("_item", (int)item), ("_interactionText", text), ("_interactionPoint", point),
                ("_successActions", actions), ("_interactionType", 4));

            Zone(parent, "Comment " + item, x, y, 9f, monologue);
        }

        private static void BuildLedges(Transform parent, LevelLayout layout)
        {
            var ledges = new GameObject("Ledges").transform;
            ledges.SetParent(parent, false);

            foreach (LedgeMark mark in layout.Ledges)
            {
                float wall = mark.Column * BlockSize;
                float low = mark.Low * BlockSize;
                float high = mark.High * BlockSize;
                bool up = mark.IsUp;
                float standY = up ? low : high;

                var root = new GameObject(up ? "Ledge Up" : "Ledge Down");
                root.transform.SetParent(ledges, false);
                root.transform.position = new Vector3(wall - 1f, standY + 1f, 0f);
                BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(2.4f, 3f);

                Transform point = Child(root.transform, "InteractionPoint", root.transform.position);
                Transform wallPoint = Child(root.transform, "WallPoint", new Vector3(wall - 0.5f, low + HeroHalfHeight, 0f));
                Transform bagTarget = Child(root.transform, "BagTarget", up
                    ? new Vector3(wall + 2.8f, high + 0.78f, 0f)
                    : new Vector3(wall + 2.2f, low + 0.78f, 0f));
                Transform heroTarget = Child(root.transform, "HeroTarget", up
                    ? new Vector3(wall + 1.3f, high + HeroHalfHeight, 0f)
                    : new Vector3(wall + 1.1f, low + HeroHalfHeight, 0f));
                Transform actions = Child(root.transform, "SuccessActions", root.transform.position);
                actions.gameObject.AddComponent<LedgeAction>();

                LedgeInteractable ledge = root.AddComponent<LedgeInteractable>();
                Set(ledge, ("_direction", (int)(up ? LedgeDirection.Up : LedgeDirection.Down)),
                    ("_interactionPoint", point), ("_wallPoint", wallPoint), ("_bagTarget", bagTarget),
                    ("_heroTarget", heroTarget), ("_successActions", actions));
            }
        }

        private static void BuildDustSources(Transform parent, LevelLayout layout)
        {
            var dust = new GameObject("Dust").transform;
            dust.SetParent(parent, false);

            Sprite[] charged = Enumerable.Range(1, 3)
                .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dust}/charged_{index}.png")).ToArray();
            Sprite[] piles = Enumerable.Range(1, 3)
                .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dust}/warm_{index}.png")).ToArray();

            void Cloud(int column, float capacity)
            {
                float y = layout.SurfaceY(column) + 2.4f;
                var root = new GameObject("Magnetic Cloud");
                root.transform.SetParent(dust, false);
                root.transform.position = new Vector3(LevelLayout.ColumnX(column), y, 0f);
                root.AddComponent<CircleCollider2D>().isTrigger = true;
                root.GetComponent<CircleCollider2D>().radius = 1.4f;
                GameObject visual = SpriteObject(root.transform, "Visual", $"{Dust}/charged_1.png", root.transform.position, 3);
                DustSource source = root.AddComponent<DustSource>();
                Set(source, ("_kind", (int)DustKind.Magnetic), ("_capacity", capacity),
                    ("_renderer", visual.GetComponent<SpriteRenderer>()), ("_frames", charged.Cast<Object>().ToArray()));
            }

            void Pile(int column)
            {
                var root = new GameObject("Warm Dust");
                root.transform.SetParent(dust, false);
                root.transform.position = new Vector3(LevelLayout.ColumnX(column), layout.SurfaceY(column) - 0.05f, 0f);
                BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(2.6f, 1.6f);
                collider.offset = new Vector2(0f, 0.8f);
                GameObject visual = SpriteObject(root.transform, "Visual", $"{Dust}/warm_1.png", root.transform.position, -3);
                DustSource source = root.AddComponent<DustSource>();
                Set(source, ("_kind", (int)DustKind.Warm), ("_capacity", 40f),
                    ("_renderer", visual.GetComponent<SpriteRenderer>()), ("_frames", piles.Cast<Object>().ToArray()));
            }

            // Первое облако стоит сразу за местом крушения: с него начинается обучение МУ.
            Cloud(FirstCloudColumn, 24f);

            bool Free(int column) => layout.Ledges.All(mark => Mathf.Abs(mark.Column - column) > 4);
            int Nearest(int column)
            {
                while (!Free(column)) column++;
                return column;
            }

            int victory = VictoryColumn(layout);
            for (int column = 180; column < victory - 40; column += 72)
            {
                if (column >= layout.TunnelStart - 2 && column <= layout.TunnelEnd + 2) continue;
                Cloud(Nearest(column), 20f);
            }

            // Пыль лежит кучами на всём пути, собрать её можно только на тёплом грунте.
            Pile(FirstDustColumn);
            for (int column = FirstDustColumn + 12; column < victory - 60; column += 64)
            {
                if (column >= layout.TunnelStart - 2 && column <= layout.TunnelEnd + 2) continue;
                Pile(Nearest(column));
            }

            // В тоннеле намагниченной пыли особенно много.
            for (int column = layout.TunnelStart + 26; column < layout.TunnelEnd - 10; column += 30)
                Cloud(column, 14f);
        }

        private const int FirstCloudColumn = 70;
        private const int FirstDustColumn = 164;

        private static void BuildDecor(Transform parent, LevelLayout layout)
        {
            var decor = new GameObject("Decor").transform;
            decor.SetParent(parent, false);
            var random = new Random(77);
            int column = 48;
            int end = VictoryColumn(layout) + 28;
            while (column < end)
            {
                column += random.Next(5, 16);
                if (layout.Ledges.Any(mark => Mathf.Abs(mark.Column - column) <= 1)) continue;
                if (column >= layout.TunnelStart && column <= layout.TunnelEnd && random.Next(3) > 0) continue;
                SpriteObject(decor, "Rock", $"{Decor}/rock_{random.Next(1, 6)}.png",
                    new Vector3(LevelLayout.ColumnX(column) + (float)random.NextDouble() - 0.5f,
                        layout.SurfaceY(column) - 0.06f, 0f), -4);
            }
        }

        private static void BuildStoryZones(Transform parent, LevelLayout layout)
        {
            var zones = new GameObject("Story Zones").transform;
            zones.SetParent(parent, false);

            void At(int column, string monologue, float width = 6f, bool newPlace = false)
                => Zone(zones, "Zone " + monologue, LevelLayout.ColumnX(column), layout.SurfaceY(column), width, monologue, newPlace);

            At(FirstCloudColumn - 6, "cloud_first");

            LedgeMark firstUp = layout.Ledges.First(mark => mark.IsUp);
            LedgeMark firstDown = layout.Ledges.First(mark => !mark.IsUp);
            At(firstUp.Column - 4, "ledge_up_first");
            At(firstDown.Column - 4, "ledge_down_first");
            At(FirstDustColumn - 5, "warm_dust_first");

            At(310, "new_place_1", 6f, true);
            At(layout.TunnelStart - 22, "tunnel_mouth", 8f, true);
            At(layout.TunnelStart + 6, "tunnel_inside", 6f, true);
            At(layout.TunnelEnd + 6, "tunnel_exit", 6f, true);
            At(layout.TunnelEnd + 170, "new_place_2", 6f, true);
            At(VictoryColumn(layout) - 90, "final_climb", 6f, true);

            // Тоннель целиком: внутри другие правила температуры и почти нет обзора.
            float start = layout.TunnelStart * BlockSize;
            float end = (layout.TunnelEnd + 1) * BlockSize;
            var tunnel = new GameObject("Tunnel");
            tunnel.transform.SetParent(zones, false);
            tunnel.transform.position = new Vector3((start + end) * 0.5f, layout.SurfaceY(layout.TunnelStart), 0f);
            Set(tunnel.AddComponent<TunnelZone>(), ("_size", new Vector2(end - start, 60f)));
        }

        private static void BuildVictory(Transform parent, LevelLayout layout)
        {
            int column = VictoryColumn(layout);
            var zone = new GameObject("Victory Zone");
            zone.transform.SetParent(parent, false);
            zone.transform.position = new Vector3(LevelLayout.ColumnX(column), layout.SurfaceY(column), 0f);
            Set(zone.AddComponent<VictoryZone>(), ("_size", new Vector2(2f, 40f)));

            var wall = new GameObject("World End");
            wall.transform.SetParent(parent, false);
            wall.transform.position = new Vector3((layout.LastColumn - 2) * BlockSize, layout.SurfaceY(column) + 10f, 0f);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 60f);
        }

        // Плато наверху длиной в 48 столбцов: 18 до зоны победы и 30 для вида на горизонт.
        private static int VictoryColumn(LevelLayout layout) => layout.LastColumn - 32;

        private static void Zone(Transform parent, string name, float x, float y, float width, string monologue, bool newPlace = false)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent, false);
            zone.transform.position = new Vector3(x, y, 0f);
            Set(zone.AddComponent<MonologueZone>(), ("_size", new Vector2(width, 30f)),
                ("_monologueId", monologue), ("_isNewPlace", newPlace));
        }

        private static GameObject SpriteObject(Transform parent, string name, string spritePath, Vector3 position, int order)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (renderer.sprite == null)
                throw new InvalidOperationException("Missing sprite: " + spritePath);
            renderer.sortingOrder = order;
            return item;
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            return item.transform;
        }
    }
}
