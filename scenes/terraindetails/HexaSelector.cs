using Godot;
using System;

[Tool]
public class HexaSelector : MeshInstance{

    [Export] public Color color;
    [Export] public bool update {set {
        crear();//hack para actualizar en edicion
    }get{return true;}}

    public override void _Ready(){
        crear();
    }
    public override void _Process(float delta){
        //efecto seleccion
        if(Visible){
            float t = OS.GetTicksMsec();
            float scaleWave = 0.8f + Mathf.Sin(t*0.008f)*0.02f;
            Scale = (Vector3.One * scaleWave);
        }
    }

    private void crear(){
        var st = new SurfaceTool();
        SpatialMaterial mat = (SpatialMaterial) MaterialOverride;
        //mat.AlbedoColor = color;
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.LineLoop);

        //vertices
        float height = 0f;
        float size = 0.70f;
        float sexto = Mathf.Pi/3;

        Vector3 v0 = new Vector3(0,height,0);//CENTRO
        Vector3 v1 = new Vector3(Mathf.Cos(sexto * 1), height, Mathf.Sin(sexto*1)) * size;//SE
        Vector3 v2 = new Vector3(Mathf.Cos(sexto * 2), height, Mathf.Sin(sexto*2)) * size;//SW
        Vector3 v3 = new Vector3(Mathf.Cos(sexto * 3), height, Mathf.Sin(sexto*3)) * size;//W
        Vector3 v4 = new Vector3(Mathf.Cos(sexto * 4), height, Mathf.Sin(sexto*4)) * size;//NW
        Vector3 v5 = new Vector3(Mathf.Cos(sexto * 5), height, Mathf.Sin(sexto*5)) * size;//NE
        Vector3 v6 = new Vector3(Mathf.Cos(sexto * 6), height, Mathf.Sin(sexto*6)) * size;//E
        
        createLine(st,v1,color);
        createLine(st,v2,color);
        createLine(st,v3,color);
        createLine(st,v4,color);
        createLine(st,v5,color);
        createLine(st,v6,color);

        //FINALY
        var mesh = st.Commit();
        Mesh = mesh;
    }
    
    private void createLine(SurfaceTool st, Vector3 v1,Color color){
        st.AddColor(color);
        st.AddUv(new Vector2());
        st.AddVertex(v1);
    }

}
