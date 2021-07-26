using Godot;
using System;
[Tool]
public class Grass : MeshInstance {
    [Export] public bool update {set {
        crear(new Random());//hack para actualizar en edicion
    }get{return true;}}


    public void crear(Random random){
        SurfaceTool st = new SurfaceTool();
        ShaderMaterial mat = ResourceLoader.Load("res://src/shaders_materials/grass.material") as ShaderMaterial;
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);

        //vertex
        int count = 50;
        float radius = 0.5f;
        for (int i = 0; i<count;i++){
            float scale = GeoAux.FloatRange(random,0.4f, 0.5f);
            float x = GeoAux.FloatRange(random, -radius, radius);
            float z = GeoAux.FloatRange(random, -radius, radius);
            Vector3 pos = new Vector3(x,0,z);
            createVertex(st,random,pos, scale);
        }

        //finaly
        st.GenerateNormals(); 
        st.GenerateTangents(); 
        Mesh = st.Commit(); 
    }

    public static void createVertex(SurfaceTool st,Random rnd, Vector3 position, float scale) {
        float height = 2f;
        float width = 0.25f;
        float md = width/2;
        Color color = Color.ColorN("green");

        //vertices 
        Vector3 v1 = new Vector3(md,0,0);//DonwRight 
        Vector3 v2 = new Vector3(-md,0f,0);//DownLeft 
        Vector3 v3 = new Vector3(0,height,0);//Up 
        
        // rotations 
        float angle1 = (2*Mathf.Pi) * GeoAux.FloatRange(rnd,-1f, 1f); 
        float angle2 = (2*Mathf.Pi) * GeoAux.FloatRange(rnd,-0.02f, 0.02f); 

        v1 = v1.Rotated(Vector3.Up,angle1); v1 = v1.Rotated(Vector3.Forward,angle2); 
        v2 = v2.Rotated(Vector3.Up,angle1); v2 = v2.Rotated(Vector3.Forward,angle2); 
        v3 = v3.Rotated(Vector3.Up,angle1); v3 = v3.Rotated(Vector3.Forward,angle2); 

        //scale 
        v1 *= scale; v2 *= scale; v3 *= scale; 

        //translate
        v1+=position; v2+=position; v3+=position;

        // TRIS 
        GeoAux.createTri(st,v1,v2,v3,color); 
    }

    private static float deformation (Random random,float size){
        float min = size /4;
        float max = size /2;
        return Mathf.Lerp(min,max,GeoAux.FloatRange(random,0,1)); //random deformation
    }
}
