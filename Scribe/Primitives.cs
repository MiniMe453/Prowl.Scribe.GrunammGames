using Prowl.Scribe.Internal;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Prowl.Scribe
{

    /// <summary>
    /// Represents a node in the font atlas, typically a free rectangular space.
    /// For a skyline bottom-left bin packer, this might store skyline segments.
    /// </summary>
    internal struct AtlasNode
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public AtlasNode(int x, int y, int width, int height = 0)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Represents a rectangular area, typically in a texture atlas.
    /// </summary>
    public struct AtlasRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public AtlasRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public enum TextWrapMode
    {
        NoWrap,
        Wrap
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        //Justify
    }


    public enum FontStyle
    {
        Regular,
        Bold,
        Italic,
        BoldItalic
    }

    public struct TextLayoutSettings
    {
        public float PixelSize;
        public FontFile Font;
        public float LetterSpacing;
        public float WordSpacing;
        public float LineHeight; // multiplier (1.0 = normal, 1.2 = 20% larger)
        public int TabSize; // in characters
        public TextWrapMode WrapMode;
        public TextAlignment Alignment;
        public float MaxWidth; // for wrapping, 0 = no limit

        public Func<int, FontFile> FontSelector; // optional: index in the full string -> font

        public static TextLayoutSettings Default => new TextLayoutSettings {
            PixelSize = 16,
            Font = null,
            LetterSpacing = 0,
            WordSpacing = 0,
            LineHeight = 1.0f,
            TabSize = 4,
            WrapMode = TextWrapMode.NoWrap,
            Alignment = TextAlignment.Left,
            MaxWidth = 0
        };
    }
    public struct GlyphInstance
    {
        public AtlasGlyph Glyph;
        public Vector2 Position;
        public char Character;
        public float AdvanceWidth;
        public int CharIndex;

        // Static pool
        private static List<GlyphInstance> pool = new List<GlyphInstance>();
        private static int indexCounter = 0;

        public GlyphInstance(AtlasGlyph glyph, Vector2 position, char character, float advanceWidth, int charIndex)
        {
            Glyph = glyph;
            Position = position;
            Character = character;
            AdvanceWidth = advanceWidth;
            CharIndex = charIndex;
        }

        // Get a new instance from the pool or create a new one
        public static GlyphInstance Get(AtlasGlyph glyph, Vector2 position, char character, float advanceWidth, int charIndex)
        {
            if (indexCounter < pool.Count)
            {
                // Reuse an existing instance
                var instance = pool[indexCounter++];
                instance.Glyph = glyph;
                instance.Position = position;
                instance.Character = character;
                instance.AdvanceWidth = advanceWidth;
                instance.CharIndex = charIndex;
                return instance;
            }
            else
            {
                // Create a new instance and add it to the pool
                var instance = new GlyphInstance(glyph, position, character, advanceWidth, charIndex);
                pool.Add(instance);
                indexCounter++;
                return instance;
            }
        }

        // Reset the pool counter (does not clear the pool, just allows reuse)
        public static void Free()
        {
            indexCounter = 0;
        }
    }


    public struct Line
    {
        private static List<Line> _pool = new List<Line>();
        private static int _poolIndex = 0;
        
        public List<GlyphInstance> Glyphs;
        public float Width;
        public float Height;
        public Vector2 Position; // relative to layout origin
        public int StartIndex; // character index in original string
        public int EndIndex; // character index in original string

        public Line(Vector2 position, int startIndex)
        {
            Glyphs = new List<GlyphInstance>();
            Width = 0;
            Height = 0;
            Position = position;
            StartIndex = startIndex;
            EndIndex = startIndex;
        }

        public void SetData(Vector2 position, int startIndex)
        {
            if(Glyphs == null) Glyphs = new List<GlyphInstance>(128);
            
            Glyphs.Clear();
            Width = 0;
            Height = 0;
            Position = position;
            StartIndex = startIndex;
            EndIndex = startIndex;
        }

        public static Line GetFromPool()
        {
            if (_poolIndex >= _pool.Count)
            {
                _pool.Add(new Line());
            }

            var line = _pool[_poolIndex];
            _poolIndex++;
            return line;
        }

        public static void ResetPool()
        {
            _poolIndex = 0;
            GlyphInstance.Free();
        }
    }
}
