using Godot;
using System;
[Tool]
public class Grass : MultiMeshInstance {

    [Export] public bool update {set {
        crear(new Random(),0f);//hack para actualizar en edicion
    }get{return true;}}

    public override void _Ready(){
        if (Multimesh == null) {
             crear(new Random(),0f);
        }
    }

    public override void _Process(float delta){
        Vector3 campos = GetViewport().GetCamera().GlobalTransform.origin;
        float dist = (campos - GlobalTransform.origin).LengthSquared();
        if (dist < 30f){
            Visible = true;
        }else{
            Visible = false;
        }   
    }

    public void crear(Random random,float parentHeight){
        if (Multimesh == null) {
            Multimesh = new MultiMesh();
            Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
            Multimesh.InstanceCount = 250;
        }
        

        var st = new SurfaceTool();
        ShaderMaterial mat = (ShaderMaterial) MaterialOverride;
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);

        float height = 2f;
        float width = 0.5f;
        float med = width/2;
        Color color = Color.ColorN("green");

        //vertices
        Vector3 v1 = new Vector3(med,0,0);//DonwRight
        Vector3 v2 = new Vector3(-med,0f,0);//DownLeft
        Vector3 v3 = new Vector3(0,height,0);//Up
        GeoAux.createTri(st,v1,v2,v3,color);

        //Create Mesh
        var mesh = st.Commit();
        Multimesh.Mesh = mesh;

        ////altura del padre
        Translation = new Vector3(Translation.x,parentHeight,Translation.z); 

        //poblar
        float radius = 0.5f;
        for (int i = 0; i< Multimesh.InstanceCount;i++){
            float angle = GeoAux.FloatRange(random,-Mathf.Pi/2,Mathf.Pi/2);
            float scale = GeoAux.FloatRange(random,0.3f,0.5f);
            float x = GeoAux.FloatRange(random,-radius,radius);
            float z = GeoAux.FloatRange(random,-radius,radius);
            
            Transform tr = Transform.Rotated(Vector3.Up,angle);
            tr.basis.Scale = new Vector3(scale,scale,scale);
            tr.origin = new Vector3(x,0,z);
            Multimesh.SetInstanceTransform(i,tr);
        }

    }

}
