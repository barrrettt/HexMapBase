using Godot;
using System;

public static class GeoAux{
     // AUX 
    public static void createColorQuad(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,Color cOt, Color cMe){
        //2 tris:
        createColorTri(st,v1,v2,v3,cMe,cOt,cOt);
        createColorTri(st,v3,v4,v1,cOt,cMe,cMe);
    }

    // AUX 
    public static void createColorTri(SurfaceTool st, Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3){
        st.AddColor(c1);
        st.AddUv(new Vector2(0,0));
        st.AddVertex(v1);
        st.AddColor(c2);
        st.AddUv(new Vector2(0,0));
        st.AddVertex(v2);
        st.AddColor(c3);
        st.AddUv(new Vector2(0,0));
        st.AddVertex(v3);
    }

    // AUX 
    public static void createTri(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Color color){
        createColorTri(st,v1,v2,v3,color,color,color);
    }
    // AUX 
    public static void createQuad(SurfaceTool st, Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4,Color color){
        //2 tris:
        createTri(st,v1,v2,v3,color);
        createTri(st,v3,v4,v1,color);
    }

    // Random floats
    public static float FloatRange(Random random, float min = 0.0f, float max = 1.0f) {
        return (float) (random.NextDouble() * (max - min) + min);
    }

}