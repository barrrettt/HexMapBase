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
        st.SetMaterial(mat);
        st.Begin(Mesh.PrimitiveType.Triangles);

        //vertices
        float height = 0f;
        float size = 0.7f; float sizee = 0.9f;
        float sexto = Mathf.Pi/3;

        Vector3 v0 = new Vector3(0,height,0);//CENTRO
        Vector3 v1 = new Vector3(Mathf.Cos(sexto * 1), height, Mathf.Sin(sexto*1)) * size;//SE
        Vector3 v2 = new Vector3(Mathf.Cos(sexto * 2), height, Mathf.Sin(sexto*2)) * size;//SW
        Vector3 v3 = new Vector3(Mathf.Cos(sexto * 3), height, Mathf.Sin(sexto*3)) * size;//W
        Vector3 v4 = new Vector3(Mathf.Cos(sexto * 4), height, Mathf.Sin(sexto*4)) * size;//NW
        Vector3 v5 = new Vector3(Mathf.Cos(sexto * 5), height, Mathf.Sin(sexto*5)) * size;//NE
        Vector3 v6 = new Vector3(Mathf.Cos(sexto * 6), height, Mathf.Sin(sexto*6)) * size;//E

        Vector3 ev1 = new Vector3(Mathf.Cos(sexto * 1), height, Mathf.Sin(sexto*1)) * sizee;//SE EXTERIORES
        Vector3 ev2 = new Vector3(Mathf.Cos(sexto * 2), height, Mathf.Sin(sexto*2)) * sizee;//SW
        Vector3 ev3 = new Vector3(Mathf.Cos(sexto * 3), height, Mathf.Sin(sexto*3)) * sizee;//W
        Vector3 ev4 = new Vector3(Mathf.Cos(sexto * 4), height, Mathf.Sin(sexto*4)) * sizee;//NW
        Vector3 ev5 = new Vector3(Mathf.Cos(sexto * 5), height, Mathf.Sin(sexto*5)) * sizee;//NE
        Vector3 ev6 = new Vector3(Mathf.Cos(sexto * 6), height, Mathf.Sin(sexto*6)) * sizee;//E
        

        GeoAux.createQuad(st,v1,ev1,ev2,v2,color);
        GeoAux.createQuad(st,v2,ev2,ev3,v3,color);
        GeoAux.createQuad(st,v3,ev3,ev4,v4,color);
        GeoAux.createQuad(st,v4,ev4,ev5,v5,color);
        GeoAux.createQuad(st,v5,ev5,ev6,v6,color);
        GeoAux.createQuad(st,v6,ev6,ev1,v1,color);

        //FINALY
        var mesh = st.Commit();
        Mesh = mesh;
    }

}
