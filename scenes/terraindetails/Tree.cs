using Godot;
using System;
[Tool]
public class Tree : MeshInstance {
    [Export] public bool update {set {
        crear(new Random());//hack to update on editor
    }get{return true;}}


    public void crear(Random random){
        SurfaceTool st = new SurfaceTool();
        ShaderMaterial matTree = ResourceLoader.Load("res://src/shaders_materials/tree_vs.material") as ShaderMaterial;
        st.SetMaterial(matTree);
        st.Begin(Mesh.PrimitiveType.Triangles);

        //styles
        float scale = GeoAux.FloatRange(random,0.4f,0.5f);
        createVertex(st,random,Vector3.Zero, scale, new Color("#261506"), new Color("#1c8c3a"));

        //finaly
        st.GenerateNormals(); 
        Mesh = st.Commit(); 
    }

    public static void createVertex(SurfaceTool st,Random rnd, Vector3 position, float scale, Color color1, Color color2) {
        
        float size = 1f;
        float md = size/2; 
        
        Vector3 center = Vector3.Zero; // base tree position 
        Vector3[,] vertex = new Vector3[4,4]; // Vertex 

        // base trunk 4 quads 
        vertex[0,0] = new Vector3(center.x + md,center.y, center.z + md);
        vertex[0,1] = new Vector3(center.x - md,center.y, center.z + md);
        vertex[0,2] = new Vector3(center.x - md,center.y, center.z - md);
        vertex[0,3] = new Vector3(center.x + md,center.y, center.z - md);

        size = 0.4f;
        md = size/2;
        float h = 2f; // height truk 
        vertex[1,0] = new Vector3(center.x + md,center.y+h, center.z + md);
        vertex[1,1] = new Vector3(center.x - md,center.y+h, center.z + md);
        vertex[1,2] = new Vector3(center.x - md,center.y+h, center.z - md);
        vertex[1,3] = new Vector3(center.x + md,center.y+h, center.z - md);
        
        // foliage base 
        size = 2.0f;
        md = size/2;
        vertex[2,0] = new Vector3(center.x + md,center.y+h, center.z + md);
        vertex[2,1] = new Vector3(center.x - md,center.y+h, center.z + md);
        vertex[2,2] = new Vector3(center.x - md,center.y+h, center.z - md);
        vertex[2,3] = new Vector3(center.x + md,center.y+h, center.z - md);
        
        size = 0.001f;
        md = size/2;
        h = h+3f; // height foliage 
        vertex[3,0] = new Vector3(center.x + md,center.y+h, center.z + md);
        vertex[3,1] = new Vector3(center.x - md,center.y+h, center.z + md);
        vertex[3,2] = new Vector3(center.x - md,center.y+h, center.z - md);
        vertex[3,3] = new Vector3(center.x + md,center.y+h, center.z - md);

        // rotation 
        float rotAngle = Mathf.Pi * GeoAux.FloatRange(rnd,-1,1f); 
        for (int i = 0;i<vertex.GetLength(0);i++){ 
            for (int j = 0; j<vertex.GetLength(1);j++){ 
                vertex[i,j] = vertex[i,j].Rotated(Vector3.Up,rotAngle); 
            } 
        } 
        
        // scale 
        for (int i = 0;i<vertex.GetLength(0);i++){ 
            for (int j = 0; j<vertex.GetLength(1);j++){ 
                vertex[i,j] *= scale; 
            } 
        } 

        // translate
        for (int i = 0;i<vertex.GetLength(0);i++){
            for (int j = 0; j<vertex.GetLength(1);j++){
                vertex[i,j] += position;
            }
        }
        
        // TRIS trunk (4)
        GeoAux.createQuad(st,vertex[0,0],vertex[0,1],vertex[1,1],vertex[1,0],color1);
        GeoAux.createQuad(st,vertex[0,1],vertex[0,2],vertex[1,2],vertex[1,1],color1);
        GeoAux.createQuad(st,vertex[0,2],vertex[0,3],vertex[1,3],vertex[1,2],color1);
        GeoAux.createQuad(st,vertex[0,3],vertex[0,0],vertex[1,0],vertex[1,3],color1);
        GeoAux.createQuad(st,vertex[1,0],vertex[1,1],vertex[1,2],vertex[1,3],color1); // top trunk

        //union
        GeoAux.createQuad(st,vertex[2,1],vertex[2,0],vertex[1,0],vertex[1,1],color2);
        GeoAux.createQuad(st,vertex[2,2],vertex[2,1],vertex[1,1],vertex[1,2],color2);
        GeoAux.createQuad(st,vertex[2,3],vertex[2,2],vertex[1,2],vertex[1,3],color2);
        GeoAux.createQuad(st,vertex[2,0],vertex[2,3],vertex[1,3],vertex[1,0],color2);
        
        // TRIS foliage (4)
        GeoAux.createQuad(st,vertex[2,0],vertex[2,1],vertex[3,1],vertex[3,0],color2);
        GeoAux.createQuad(st,vertex[2,1],vertex[2,2],vertex[3,2],vertex[3,1],color2);
        GeoAux.createQuad(st,vertex[2,2],vertex[2,3],vertex[3,3],vertex[3,2],color2);
        GeoAux.createQuad(st,vertex[2,3],vertex[2,0],vertex[3,0],vertex[3,3],color2);
        GeoAux.createQuad(st,vertex[3,0],vertex[3,1],vertex[3,2],vertex[3,3],color2); // top

    }

    private static float deformation (Random random,float size){ 
        float min = size /4; float max = size /2; 
        return Mathf.Lerp(min,max,GeoAux.FloatRange(random,0,1)); //random deformation 
    }
}
