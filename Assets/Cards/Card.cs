using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour
{
    public CardDef Definition;

    CardAtlas Atlas
    {
        get { return (Definition != null) ? Definition.Atlas : null; }
    }
    CardStock Stock
    {
        get { return (Definition != null) ? Definition.Stock : null; }
    }

    public bool Clicked
    {
        get; set;
    }

    bool m_built = false;

    // State for card being dealt and flying out of deck.
    bool m_flying;
    float m_flyTime;
    float m_flyDuration;
    Vector3 m_flySource;
    Vector3 m_flyTarget;

    public void SetFlyTarget(Vector3 source, Vector3 target, float duration)
    {
        m_flying = true;
        m_flyTime = Time.time;
        m_flyDuration = duration;
        m_flySource = source;
        m_flyTarget = target;
    }

    class SubMesh
    {
        public List<Vector3> VertexList = new List<Vector3>();
        public List<int> IndexList = new List<int>();
        public List<Vector2> TexCoords = new List<Vector2>();
        public List<Color> Colors = new List<Color>();

        public SubMesh()
        {
            VertexList = new List<Vector3>();
            IndexList = new List<int>();
        }

        public void AddVertex(Vector3 v, Vector2 uv, Color color)
        {
            VertexList.Add(v);
            TexCoords.Add(uv);
            Colors.Add(color);
        }

        public void AddTriangle(int a, int b, int c)
        {
            IndexList.Add(a);
            IndexList.Add(b);
            IndexList.Add(c);
        }
    }

    class CardMesh
    {
        public List<Material> Materials = new List<Material>();
        public List<SubMesh> MeshList = new List<SubMesh>();

        public SubMesh NewSubMesh(Material mat)
        {
            SubMesh mesh = new SubMesh();
            MeshList.Add(mesh);
            Materials.Add(mat);
            return mesh;
        }

        public List<Vector3> GetCombinedVertices()
        {
            if (MeshList.Count > 0)
            {
                List<Vector3> combined = new List<Vector3>();
                foreach (SubMesh m in MeshList)
                {
                    combined.AddRange(m.VertexList);
                }
                return combined;
            }
            return MeshList[0].VertexList;
        }

        public List<Vector2> GetCombinedTexCoords()
        {
            if (MeshList.Count > 0)
            {
                List<Vector2> combined = new List<Vector2>();
                foreach (SubMesh m in MeshList)
                {
                    combined.AddRange(m.TexCoords);
                }
                return combined;
            }
            return MeshList[0].TexCoords;
        }

        public List<Color> GetCombinedColors()
        {
            if (MeshList.Count > 0)
            {
                List<Color> combined = new List<Color>();
                foreach (SubMesh m in MeshList)
                {
                    combined.AddRange(m.Colors);
                }
                return combined;
            }
            return MeshList[0].Colors;
        }
    }

    Vector2 UV(CardShape shape, float tu, float tv)
    {
        float u = Mathf.Lerp(shape.Min.x, shape.Max.x, tu);
        float v = Mathf.Lerp(shape.Min.y, shape.Max.y, tv);
        return new Vector2(u, v);
    }

    void BuildCorner3(SubMesh data, CardShape shape, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        CardStock Stock = Definition.Stock;
        int smooth = Stock.Smooth;

        int vbase = data.VertexList.Count;
        this.VertexUV(data, shape, v0, UV(shape, 0, 1));
        this.VertexUV(data, shape, v1, UV(shape, 0, 1));
        this.VertexUV(data, shape, v2, UV(shape, 1, 0));

        float deltaAngle = 0.5f * Mathf.PI / (smooth + 1);
        //int prev = vbase+1;
        Vector3 vy = v1 - v0;
        Vector3 vx = v2 - v0;
        for (int i = 0; i <= smooth; ++i)
        {
            if (i < smooth)
            {
                float angle = (i + 1) * deltaAngle;
                float tu = Mathf.Sin(angle);
                float tv = Mathf.Cos(angle);
                Vector3 xyz = v0 + tu * vx + tv * vy;
                Vector2 uv = UV(shape, tu, tv);
                data.AddVertex(xyz, uv, Color.white);
            }
            int vn = vbase + 3 + i;
            int vprev = (i == 0) ? vbase + 1 : vn - 1;
            int vnext = (i < smooth) ? vn : vbase + 2;
            data.AddTriangle(vbase, vprev, vnext);
            //prev = vn;
        }
    }

    void VertexUV(SubMesh data, CardShape shape, Vector3 pos, Vector2 uv)
    {
        data.AddVertex(pos, uv, Color.white);
    }

    void Square(SubMesh data, CardShape shape, Vector3 pos, Vector2 size, Color color)
    {
        int vi = data.VertexList.Count;
        Vector2 t0 = shape.UV0;
        Vector2 t1 = shape.UV1;
        Vector2 t2 = shape.UV2;
        Vector2 t3 = shape.UV3;
        data.AddVertex(pos + new Vector3(-size.x, +size.y, 0), t0, color);
        data.AddVertex(pos + new Vector3(+size.x, +size.y, 0), t1, color);
        data.AddVertex(pos + new Vector3(+size.x, -size.y, 0), t2, color);
        data.AddVertex(pos + new Vector3(-size.x, -size.y, 0), t3, color);
        data.AddTriangle(vi, vi + 1, vi + 2);
        data.AddTriangle(vi, vi + 2, vi + 3);
    }

    void SquareFlip(SubMesh data, CardShape shape, Vector3 pos, Vector2 size, Color color, bool vflip, bool hflip)
    {
        Vector3 v0 = pos + new Vector3(-size.x, +size.y, 0);
        Vector3 v1 = pos + new Vector3(+size.x, +size.y, 0);
        Vector3 v2 = pos + new Vector3(+size.x, -size.y, 0);
        Vector3 v3 = pos + new Vector3(-size.x, -size.y, 0);
        Square4(data, shape, v0, v1, v2, v3, color, vflip, hflip);
    }

    void Square4(SubMesh data, CardShape shape, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color, bool vflip, bool hflip)
    {
        int vi = data.VertexList.Count;
        Vector2 t0 = shape.UV3;
        Vector2 t1 = shape.UV2;
        Vector2 t2 = shape.UV1;
        Vector2 t3 = shape.UV0;
        if (!vflip)
        {
            if (hflip)
            {
                data.AddVertex(v0, t2, color);
                data.AddVertex(v1, t3, color);
                data.AddVertex(v2, t0, color);
                data.AddVertex(v3, t1, color);
            }
            else
            {
                data.AddVertex(v0, t3, color);
                data.AddVertex(v1, t2, color);
                data.AddVertex(v2, t1, color);
                data.AddVertex(v3, t0, color);
            }
        }
        else
        {
            if (hflip)
            {
                data.AddVertex(v0, t1, color);
                data.AddVertex(v1, t0, color);
                data.AddVertex(v2, t3, color);
                data.AddVertex(v3, t2, color);
            }
            else
            {
                data.AddVertex(v0, t0, color);
                data.AddVertex(v1, t1, color);
                data.AddVertex(v2, t2, color);
                data.AddVertex(v3, t3, color);
            }
        }
        data.AddTriangle(vi, vi + 1, vi + 2);
        data.AddTriangle(vi, vi + 2, vi + 3);
    }

    void Square5(SubMesh data, CardShape shape, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color, bool flip)
    {
        int vi = data.VertexList.Count;
        Vector2 t0 = shape.UV0;
        Vector2 t1 = shape.UV1;
        Vector2 t2 = shape.UV2;
        Vector2 t3 = shape.UV3;
        Vector2 t4 = (t0 + t3) / 2;
        if (!flip)
        {
            data.AddVertex(v0, t3, color);
            data.AddVertex(v1, t2, color);
            data.AddVertex(v2, t1, color);
            data.AddVertex(v3, t0, color);
            data.AddVertex(v4, t4, color);
        }
        else
        {
            data.AddVertex(v0, t0, color);
            data.AddVertex(v1, t1, color);
            data.AddVertex(v2, t2, color);
            data.AddVertex(v3, t3, color);
            data.AddVertex(v4, t4, color);
        }
        data.AddTriangle(vi + 4, vi + 0, vi + 1);
        data.AddTriangle(vi + 4, vi + 1, vi + 2);
        data.AddTriangle(vi + 4, vi + 2, vi + 3);
    }

    static int[] patternBits = new int[]
    {
        0,
        128,
        2+8192,
        2+128+8192,
        1+4+4096+16384,
        1+4+128+4096+16384,
        1+4+64+256+4096+16384,
        1+4+16+64+256+4096+16384,
        1+4+16+64+256+1024+4096+16384,
        1+4+8+32+128+512+2048+4096+16384,
        1+4+8+32768+32+512+65536+2048+4096+16384,
    };

    static Vector2[] patternPos = new Vector2[]
    {
#if false
		new Vector2(0.25f,0.1f), // top left
		new Vector2(0.5f,0.1f), // top mid
		new Vector2(0.75f,0.1f), // top right
		new Vector2(0.25f,0.4f),
		new Vector2(0.5f,0.3f),
		new Vector2(0.75f,0.4f),
		new Vector2(0.25f,0.5f), // center line
		new Vector2(0.5f,0.5f),
		new Vector2(0.75f,0.5f),
		new Vector2(0.25f,0.6f), // line
		new Vector2(0.5f,0.7f),
		new Vector2(0.75f,0.6f),
		new Vector2(0.25f,0.9f), // line
		new Vector2(0.5f,0.9f),
		new Vector2(0.75f,0.9f),
		new Vector2(0.5f,0.25f), // 10 up-mid
		new Vector2(0.5f,0.75f), // 10 down-mid
#else
		// 0.100, 0.366, 0.633, 0.900
		// 0.150, 0.383, 0.616, 0.850
		new Vector2(0.225f,0.15f), // top left
		new Vector2(0.5f,0.15f), // top mid
		new Vector2(0.775f,0.15f), // top right
		new Vector2(0.225f,0.383f),
        new Vector2(0.5f,0.383f-0.05f),
        new Vector2(0.775f,0.383f),
        new Vector2(0.225f,0.5f), // center line
		new Vector2(0.5f,0.5f),
        new Vector2(0.775f,0.5f),
        new Vector2(0.225f,0.616f), // line
		new Vector2(0.5f,0.616f+0.05f),
        new Vector2(0.775f,0.616f),
        new Vector2(0.225f,0.85f), // line
		new Vector2(0.5f,0.85f),
        new Vector2(0.775f,0.85f),
        new Vector2(0.5f,0.263f), // 10 up-mid
		new Vector2(0.5f,0.747f), // 10 down-mid
#endif
	};

    // Which pips are Upside down?
    static bool[] patternUpper = new bool[]
    {
        true, // top left
		true, // top mid
		true, // top right
		true,
        true,
        true,
        true, // center line
		true,
        true,
        false, // line
		false,
        false,
        false, // line
		false,
        false,
        true, // 10 up-mid
		false, // 10 down-mid
	};

    public bool IsValid()
    {
        if (Definition != null)
        {
            if (Definition.Atlas != null)
            {
                if (Definition.Stock != null && Definition.Stock.DefaultMaterial != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    static Color GetSymbolColor(string shape)
    {
        return (shape.ToLower() == "heart") || (shape.ToLower() == "diamond") ? Color.red : Color.black;
    }

    // Cache material so that render batching includes multiple cards.
    static Material LastMat;

    // Optional support for multiple atlases or textures
    SubMesh GetMesh(CardMesh card, Dictionary<Texture2D, SubMesh> table, CardShape shape)
    {
        if (shape != null)
        {
            if (table.ContainsKey(shape.Image))
            {
                return table[shape.Image];
            }
            Material mat = LastMat;
            if (LastMat == null || LastMat.mainTexture != shape.Image || LastMat.shader != Stock.DefaultMaterial.shader)
            {
                mat = new Material(Stock.DefaultMaterial); //new Material(Shader.Find("Diffuse"));
                mat.mainTexture = shape.Image;
                LastMat = mat;
            }
            SubMesh newMesh = card.NewSubMesh(mat);
            table[shape.Image] = newMesh;
            return newMesh;
        }
        return null;
    }

    static Vector3[] RectPoints(Vector2 size, Vector2 border)
    {
        float x = size.x / 2;
        float y = size.y / 2;

        float bx = border.x;
        float by = border.y;

        float cx = x - bx;
        float cy = y - by;

        Vector3[] result = new Vector3[14];

        result[0] = new Vector2(-cx, +cy); // middle
        result[1] = new Vector2(+cx, +cy);
        result[2] = new Vector2(+cx, -cy);
        result[3] = new Vector2(-cx, -cy);
        result[4] = new Vector2(-cx, +y); // top edge
        result[5] = new Vector2(+cx, +y);
        result[6] = new Vector2(+x, +cy); // right edge
        result[7] = new Vector2(+x, -cy);
        result[8] = new Vector2(+cx, -y); // bot edge
        result[9] = new Vector2(-cx, -y);
        result[10] = new Vector2(-x, -cy); // left edge
        result[11] = new Vector2(-x, +cy);
        //   4  5
        // B 0  1 6
        //   C  D
        // A 3  2 7
        //   9  8
        result[12] = new Vector2(-cx, 0); // mid
        result[13] = new Vector2(+cx, 0);

        return result;
    }

    void BuildBack(CardMesh card, Dictionary<Texture2D, SubMesh> table, CardShape paper)
    {
        CardShape back = Atlas.FindById(Stock.Back);
        if (back != null)
        {
            Vector3[] v = RectPoints(Stock.Size * Stock.Scale, Stock.BackBorder * Stock.Scale);

            SubMesh core = GetMesh(card, table, back);
            // middle
            if (Stock.HalfBack)
            {
                Square4(core, back, v[1], v[0], v[12], v[13], Color.white, false, false);
                Square4(core, back, v[13], v[12], v[3], v[2], Color.white, true, true);
            }
            else
            {
                Square4(core, back, v[1], v[0], v[3], v[2], Color.white, false, false);
            }
            // top
            Square4(core, paper, v[5], v[4], v[0], v[1], Color.white, false, false);
            // back-left
            Square5(core, paper, v[2], v[7], v[6], v[1], v[13], Color.white, false);
            // bottom
            Square4(core, paper, v[2], v[3], v[9], v[8], Color.white, false, false);
            // back-right
            Square5(core, paper, v[0], v[11], v[10], v[3], v[12], Color.white, false);

            BuildCorner3(core, paper, v[1], v[6], v[5]);
            BuildCorner3(core, paper, v[2], v[8], v[7]);
            BuildCorner3(core, paper, v[3], v[10], v[9]);
            BuildCorner3(core, paper, v[0], v[4], v[11]);
        }
    }

    // Listen for mouse clicks on cards
    void OnMouseDown()
    {
        Debug.Log(string.Format("On Mouse Down: {0}", this.Definition));
        GameObject.Find("Game").GetComponent<CardGame>().CardClicked(this);
    }

    public void Rebuild()
    {
        if (!IsValid())
        {
            Debug.LogError("The card definition is not valid. " + this.name);
            return;
        }

        Stock.Validate();

        Dictionary<Texture2D, SubMesh> table = new Dictionary<Texture2D, SubMesh>();

        CardMesh card = new CardMesh();
        CardShape paper = Definition.Atlas.FindById(Definition.Stock.Paper);
        if (paper == null)
        {
            Debug.LogError("Paper does not exist in atlas = " + Definition.Atlas.name + "::" + Definition.Stock.Paper);
            return;
        }
        SubMesh data = GetMesh(card, table, paper);

        float x = Stock.Size.x * Stock.Scale / 2;
        float y = Stock.Size.y * Stock.Scale / 2;

        float bx = Mathf.Max(Stock.CornerSize, Stock.Border.x) * Stock.Scale;
        float by = Mathf.Max(Stock.CornerSize, Stock.Border.y) * Stock.Scale;

        //float cx = x-bx;
        //float cy = y-by;

        //   4  5
        // B 0  1 6
        //   C  D
        // A 3  2 7
        //   9  8
        float crad = Stock.CornerSize;
        Vector2 cxy = new Vector2(crad, crad);
        Vector3[] v = RectPoints(Stock.Size * Stock.Scale, Stock.Scale * cxy);

        Vector3 c0 = new Vector3(-x + bx, +y - by, 0);
        Vector3 c1 = new Vector3(+x - bx, +y - by, 0);
        Vector3 c2 = new Vector3(+x - bx, -y + by, 0);
        Vector3 c3 = new Vector3(-x + bx, -y + by, 0);

        if (Stock.Smooth > 0)
        {
            // middle
            Square4(data, paper, v[0], v[1], v[2], v[3], Color.white, false, false);
            // top
            Square4(data, paper, v[4], v[5], v[1], v[0], Color.white, false, false);
            // right
            Square4(data, paper, v[1], v[6], v[7], v[2], Color.white, false, false);
            // bottom
            Square4(data, paper, v[3], v[2], v[8], v[9], Color.white, false, false);
            // left
            Square4(data, paper, v[11], v[0], v[3], v[10], Color.white, false, false);

            BuildCorner3(data, paper, v[0], v[11], v[4]);
            BuildCorner3(data, paper, v[1], v[5], v[6]);
            BuildCorner3(data, paper, v[2], v[7], v[8]);
            BuildCorner3(data, paper, v[3], v[9], v[10]);
        }
        else // simple rectangle
        {
            Vector3 p1 = new Vector3(-x, +y, 0);
            Vector3 p2 = new Vector3(+x, +y, 0);
            Vector3 p3 = new Vector3(+x, -y, 0);
            Vector3 p4 = new Vector3(-x, -y, 0);
            Square4(data, paper, p1, p2, p3, p4, Color.white, false, false);
        }

        Vector2 textSize = Stock.TextSize * Stock.Scale;

        float s1 = Stock.BigSymbolSize * Stock.Scale;
        float s2 = Stock.SymbolSize * Stock.Scale;
        float s3 = Stock.CornerSymbolSize * Stock.Scale;
        //float symW = s2;
        //float symH = s2;
        //float rimX = Mathf.Max(textSize.x,symW);
        //float rimY = Mathf.Max(textSize.y,symH);

        CardShape symbol = Atlas.FindById(Definition.Symbol);
        if (symbol == null && !string.IsNullOrEmpty(Definition.Symbol))
        {
            Debug.LogError(string.Format("Symbol shape '{0}' is not defined in atlas.", Definition.Symbol));
        }

        // Full Image is used by cards like ACE/JOKER that might want a fully unique face image
        // Half Image is used by cards like JQK where half image is flipped and reflected for the top/bottom of the face image
        // Pattern is used to form standard layout of suit symbols for 2-10 cards
        CardShape fullImage = Definition.FullImage ? Atlas.FindById(Definition.Image) : null;
        CardShape halfImage = !Definition.FullImage ? Atlas.FindById(Definition.Image) : null;
        if (fullImage != null)
        {
            SubMesh core = GetMesh(card, table, fullImage);
            //Square4 (core,fullImage,v[0],v[1],v[2],v[3],Color.white,false,false);
            Square4(core, fullImage, c0, c1, c2, c3, Color.white, false, false);
        }
        else if (halfImage != null)
        {
            SubMesh core = GetMesh(card, table, halfImage);
            Vector3 lift = new Vector3(0, 0, -0.01f);
            Square4(core, halfImage, v[0] + lift, v[1] + lift, v[13] + lift, v[12] + lift, Color.white, false, false);
            Square4(core, halfImage, v[12] + lift, v[13] + lift, v[2] + lift, v[3] + lift, Color.white, true, true);
        }
        else if (Definition.Pattern != 0 && symbol != null)
        {
            if (Definition.Pattern >= 1 && Definition.Pattern < patternBits.Length)
            {
                SubMesh core = GetMesh(card, table, symbol);
                int bits = patternBits[Definition.Pattern];
                float x0 = -x + bx;
                float x1 = +x - bx;
                float y0 = +y - by;
                float y1 = -y + by;
                for (int b = 0; b < 17; b++)
                {
                    if ((bits & (1 << b)) != 0)
                    {
                        float px = Mathf.Lerp(x0, x1, patternPos[b].x);
                        float py = Mathf.Lerp(y0, y1, patternPos[b].y);
                        Vector2 symSize = (Definition.Pattern == 1) ? new Vector2(s1, s1) : new Vector2(s2, s2);
                        if (Stock.AlwaysUpright || patternUpper[b])
                        {
                            Square(core, symbol, new Vector3(px, py, -0.01f), symSize, Color.white);
                        }
                        else
                        {
                            SquareFlip(core, symbol, new Vector3(px, py, -0.01f), symSize, Color.white, true, true);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Pattern value '{0}' is not valid.", Definition.Pattern));
            }
        }

        // Text is used for number/letter in corner of card (ex. A for Ace)
        CardShape text = Atlas.FindById(Definition.Text);
        if (text == null && !string.IsNullOrEmpty(Definition.Text))
        {
            Debug.LogError(string.Format("Text shape '{0}' is not defined in atlas.", Definition.Text));
        }
        if (text != null)
        {
            float tx = Stock.TextOffset.x * Stock.Scale;
            float ty = Stock.TextOffset.y * Stock.Scale;
            SubMesh sub = GetMesh(card, table, text);
            float x0 = -x + tx; // + (bx+rimX)*0.5f;
            float x1 = +x - tx; // - (bx+rimX)*0.5f;
            float y0 = +y - ty; //by;
            float y1 = -y + ty; //by;
            Color color = GetSymbolColor(Definition.Symbol);
            Square(sub, text, new Vector3(x0, y0, -0.01f), textSize, color);
            if (Stock.AlwaysUpright)
            {
                Square(sub, text, new Vector3(x1, y1, -0.01f), textSize, color);
            }
            else
            {
                SquareFlip(sub, text, new Vector3(x1, y1, -0.01f), textSize, color, true, true);
            }
        }

        // Symbol is used for corner suit icons (heart,diamond,spade,club)
        if (symbol != null)
        {
            float sx = Stock.SymbolOffset.x * Stock.Scale;
            float sy = Stock.SymbolOffset.y * Stock.Scale;
            SubMesh sub = GetMesh(card, table, symbol);
            //float gapY = s3/3;
            float x0 = -x + sx; //(bx+rimX)*0.5f;
            float x1 = +x - sx; //(bx+rimX)*0.5f;
            float y0 = +y - sy; //by - textSize.y - gapY - rad;
            float y1 = -y + sy; //by + textSize.y + gapY + rad;
            Color color = GetSymbolColor(Definition.Symbol);
            Vector2 halfSym = new Vector2(s3, s3);

            // Top-left corner symbol
            Square(sub, symbol, new Vector3(x0, y0, -0.01f), halfSym, color);

            // Bottom-right symbol can be up-right or upside-down depending on preferences
            if (Stock.AlwaysUpright)
            {
                Square(sub, symbol, new Vector3(x1, y1, -0.01f), halfSym, color);
            }
            else
            {
                SquareFlip(sub, symbol, new Vector3(x1, y1, -0.01f), halfSym, color, true, true);
            }
        }

        if (Stock.TwoSided)
        {
            BuildBack(card, table, paper);
            /*
			CardShape back = Atlas.FindById(Stock.Back);
			if (back != null)
			{
				Vector3 [] v = RectPoints(Stock.Size*Stock.Scale,Stock.BackBorder*Stock.Scale);
				
				SubMesh core = GetMesh(card,table,back);
				// middle
				if (Stock.HalfBack)
				{
					Square4(core,back,v[1],v[0],v[12],v[13],Color.white,false,false);
					Square4(core,back,v[13],v[12],v[3],v[2],Color.white,true,true);
				}
				else
				{
					Square4(core,back,v[1],v[0],v[3],v[2],Color.white,false,false);
				}
				// top
				Square4(core,paper,v[5],v[4],v[0],v[1],Color.white,false,false);
				// back-left
				Square5(core,paper,v[2],v[7],v[6],v[1],v[13],Color.white,false);
				// bottom
				Square4(core,paper,v[2],v[3],v[9],v[8],Color.white,false,false);
				// back-right
				Square5(core,paper,v[0],v[11],v[10],v[3],v[12],Color.white,false);
				
				BuildCorner3(core, paper, v[1],v[6],v[5]);
				BuildCorner3(core, paper, v[2],v[8],v[7]);
				BuildCorner3(core, paper, v[3],v[10],v[9]);
				BuildCorner3(core, paper, v[0],v[4],v[11]);
			}
			*/
        }

        Mesh mesh = SetupMesh();
        mesh.vertices = card.GetCombinedVertices().ToArray();
        mesh.triangles = data.IndexList.ToArray();
        mesh.uv = card.GetCombinedTexCoords().ToArray();
        mesh.colors = card.GetCombinedColors().ToArray();

        if (card.MeshList.Count > 1)
        {
            mesh.subMeshCount = card.MeshList.Count;
            int vbase = 0;
            for (int i = 1; i < card.MeshList.Count; ++i)
            {
                SubMesh sub = card.MeshList[i];
                int[] tris = sub.IndexList.ToArray();
                vbase += card.MeshList[i - 1].VertexList.Count;
                for (int t = 0; t < tris.Length; ++t)
                {
                    tris[t] += vbase;
                }
                mesh.SetTriangles(tris, i);
            }
        }

        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateNormals();

        this.GetComponent<Renderer>().sharedMaterials = card.Materials.ToArray();

        // Setup Collider
        this.gameObject.AddComponent<BoxCollider2D>();

        // Mark as not clicked
        this.Clicked = false;
    }

    Mesh SetupMesh()
    {
        if (this.GetComponent<MeshRenderer>() == null)
        {
            this.gameObject.AddComponent(typeof(MeshRenderer));
        }
        MeshFilter mf = this.GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = this.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        }
        Mesh mesh = new Mesh();
        mf.mesh = mesh;
        return mesh;
    }

    public void TryBuild()
    {
        if (!m_built)
        {
            Rebuild();
            m_built = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        TryBuild();

        if (m_flying)
        {
            float t = Time.time - m_flyTime;
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            if (t < m_flyDuration)
            {
                float tt = t / m_flyDuration;
                pos = Vector3.Lerp(m_flySource, m_flyTarget, tt);
                // parabolic arc to lift card of deck
                pos.z += -2 * Mathf.Sin(tt * 3.14f);
                // delay card flip until 25% of flight
                float rt = Mathf.Clamp01(tt - 0.25f) / 0.75f;
                tt = 1 - rt * rt;
                rot = Quaternion.Euler(0, -180 * tt, 0);
            }
            else
            {
                pos = m_flyTarget;
                rot = Quaternion.identity;
                m_flying = false;
            }
            this.transform.position = pos;
            this.transform.rotation = rot;
        }
    }
}
