using Godot;
using System;
[Tool]
public class Rock : MeshInstance {
    [Export] public bool update {set {
        crear(new Random());//hack para actualizar en edicion
    }get{return true;}}


    public void crear(Random random){
        SurfaceTool st = new SurfaceTool();
        ShaderMaterial matRock = ResourceLoader.Load("res://src/shaders_materials/rock_vs.material") as ShaderMaterial;
        st.SetMaterial(matRock);
        st.Begin(Mesh.PrimitiveType.Triangles);

        //styles
        createVertex(st,random,Vector3.Zero,1f, new Color("#382c0a"));

        //finaly
        st.GenerateNormals(); 
        //st.GenerateTangents(); 
        Mesh = st.Commit(); 
    }

    public static void createVertex(SurfaceTool st,Random rnd, Vector3 position,float scale, Color color) {
        float size = 1f;
        float md = size/2; //mitad
        //rock center
        Vector3 center = position + (Vector3.Up *size* 0.20f);

        //vertex and random deformation (with natural efecct)

        //Vertex A: botton(Y-)
        Vector3 vAA = new Vector3(center.x,center.y-md,center.z);
        Vector3 vA1 = new Vector3(vAA.x+md,vAA.y,vAA.z+md);
        Vector3 vA2 = new Vector3(vAA.x-md,vAA.y,vAA.z+md);
        Vector3 vA3 = new Vector3(vAA.x-md,vAA.y,vAA.z-md);
        Vector3 vA4 = new Vector3(vAA.x+md,vAA.y,vAA.z-md);

        //vertex B: Forward (X+)
        Vector3 vBB = new Vector3(center.x + md,center.y,center.z);
        Vector3 vB1 = vA4;
        Vector3 vB2 = vA1;
        Vector3 vB3 = new Vector3(vBB.x,vBB.y+md,vBB.z+md);
        Vector3 vB4 = new Vector3(vBB.x,vBB.y+md,vBB.z-md);

        //Vertex C: right Z+
        Vector3 vCC = new Vector3(center.x,center.y,center.z+md);
        Vector3 vC1 = vA1;
        Vector3 vC2 = vA2;

        Vector3 vC3 = new Vector3(vCC.x-md,vCC.y+md,vCC.z);
        Vector3 vC4 = vB3;

        //Vertex D: Back X-
        Vector3 vDD = new Vector3(center.x-md,center.y,center.z);
        Vector3 vD1 = vA2;
        Vector3 vD2 = vA3;
        Vector3 vD3 = new Vector3(vDD.x,vDD.y+md,vDD.z-md);
        Vector3 vD4 = vC3;

        //Vertex E: Left Z-
        Vector3 vEE = new Vector3(center.x,center.y,center.z-md);
        Vector3 vE1 = vA3;
        Vector3 vE2 = vA4;
        Vector3 vE3 = vB4;
        Vector3 vE4 = vD3;

        //Vertex F: Top Y+
        Vector3 vFF = new Vector3(center.x,center.y+md,center.z);
        Vector3 vF1 = vB3;
        Vector3 vF2 = vC3;
        Vector3 vF3 = vD3;
        Vector3 vF4 = vB4;

        //DEFORMATIONS
        //botton
        float sizeBotton = size*1.2f;
        Vector3 vDelta = (vAA-center)* -deformation(rnd,sizeBotton);
        vAA += vDelta; 

        vDelta = (vA1-center)* -deformation(rnd,sizeBotton);
        vA1 += vDelta; vB2 += vDelta;vC1 += vDelta;

        vDelta = (vA2-center)* -deformation(rnd,sizeBotton);
        vA2 += vDelta; vC2 += vDelta;vD1 += vDelta;

        vDelta = (vA3-center)* -deformation(rnd,sizeBotton);
        vA3 += vDelta; vD2 += vDelta;vE1 += vDelta;

        vDelta = (vA4-center)* -deformation(rnd,sizeBotton);
        vA4 += vDelta; vE2 += vDelta;vB1 += vDelta;

        //mediun height
        float sizeMed = size*0.3f;
        vDelta = (vBB-center)* -deformation(rnd,sizeMed);
        vBB += vDelta;
        vDelta = (vCC-center)* -deformation(rnd,sizeMed);
        vCC += vDelta;
        vDelta = (vDD-center)* -deformation(rnd,sizeMed);
        vDD += vDelta;
        vDelta = (vEE-center)* -deformation(rnd,sizeMed);
        vEE += vDelta;

        //top
        float sizeTop = size*1.5f;
        vDelta = (vFF-center)* -deformation(rnd,sizeTop);
        vFF += vDelta; 
        
        vDelta = (vF1-center)* -deformation(rnd,sizeTop);
        vF1 += vDelta; vB3 += vDelta;vC4 += vDelta;

        vDelta = (vF2-center)* -deformation(rnd,sizeTop);
        vF2 += vDelta; vC3 += vDelta;vD4 += vDelta;

        vDelta = (vF3-center)* -deformation(rnd,sizeTop);
        vF3 += vDelta; vD3 += vDelta;vE4 += vDelta;

        vDelta = (vF4-center)* -deformation(rnd,sizeTop);
        vF4 += vDelta; vE3 += vDelta; vB4 += vDelta;

        //Vertex
        Vector3[] vertex = new Vector3[]{
            vAA,vA1,vA2,vA3,vA4,
            vBB,vA1,vA2,vA3,vA4,
            vCC,vA1,vA2,vA3,vA4,
            vDD,vA1,vA2,vA3,vA4,
            vEE,vA1,vA2,vA3,vA4,
            vFF,vA1,vA2,vA3,vA4
        };

        

        //TRIS A
        GeoAux.createTri(st,vAA,vA2,vA1,color);
        GeoAux.createTri(st,vAA,vA3,vA2,color);
        GeoAux.createTri(st,vAA,vA4,vA3,color);
        GeoAux.createTri(st,vAA,vA1,vA4,color);

         //TRIS B
        GeoAux.createTri(st,vBB,vB1,vB2,color);
        GeoAux.createTri(st,vBB,vB2,vB3,color);
        GeoAux.createTri(st,vBB,vB3,vB4,color);
        GeoAux.createTri(st,vBB,vB4,vB1,color);

        //TRIS C
        GeoAux.createTri(st,vCC,vC1,vC2,color);
        GeoAux.createTri(st,vCC,vC2,vC3,color);
        GeoAux.createTri(st,vCC,vC3,vC4,color);
        GeoAux.createTri(st,vCC,vC4,vC1,color);

        //TRIS D
        GeoAux.createTri(st,vDD,vD1,vD2,color);
        GeoAux.createTri(st,vDD,vD2,vD3,color);
        GeoAux.createTri(st,vDD,vD3,vD4,color);
        GeoAux.createTri(st,vDD,vD4,vD1,color);

        //TRIS E
        GeoAux.createTri(st,vEE,vE1,vE2,color);
        GeoAux.createTri(st,vEE,vE2,vE3,color);
        GeoAux.createTri(st,vEE,vE3,vE4,color);
        GeoAux.createTri(st,vEE,vE4,vE1,color);

        //TRIS F
        GeoAux.createTri(st,vFF,vF1,vF2,color);
        GeoAux.createTri(st,vFF,vF2,vF3,color);
        GeoAux.createTri(st,vFF,vF3,vF4,color);
        GeoAux.createTri(st,vFF,vF4,vF1,color);
    }

    private static float deformation (Random random,float size){
        float min = size /4;
        float max = size /2;
        return Mathf.Lerp(min,max,GeoAux.FloatRange(random,0,1)); //random deformation
    }
}
