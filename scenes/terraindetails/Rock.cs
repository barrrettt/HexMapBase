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
        float scale = GeoAux.FloatRange(random,0.2f,1f);
        createVertex(st,random,Vector3.Zero, scale, new Color("#382c0a"));

        //finaly
        st.GenerateNormals(); 
        //st.GenerateTangents(); 
        Mesh = st.Commit(); 
    }

    public static void createVertex(SurfaceTool st,Random rnd, Vector3 position,float scale, Color color) {
        float size = 1f;
        float md = size/2; //mitad
        //rock center and 
        Vector3 center = Vector3.Zero + (Vector3.Up *size* 0.20f);

        // vertex and random deformation (with natural efecct)
        // Vertex A: botton(Y-)
        Vector3 vAA = new Vector3(center.x,center.y-md,center.z);
        Vector3 vA1 = new Vector3(vAA.x+md,vAA.y,vAA.z+md);
        Vector3 vA2 = new Vector3(vAA.x-md,vAA.y,vAA.z+md);
        Vector3 vA3 = new Vector3(vAA.x-md,vAA.y,vAA.z-md);
        Vector3 vA4 = new Vector3(vAA.x+md,vAA.y,vAA.z-md);

        // vertex B: Forward (X+)
        Vector3 vBB = new Vector3(center.x + md,center.y,center.z);
        Vector3 vB1 = vA4;
        Vector3 vB2 = vA1;
        Vector3 vB3 = new Vector3(vBB.x,vBB.y+md,vBB.z+md);
        Vector3 vB4 = new Vector3(vBB.x,vBB.y+md,vBB.z-md);

        // Vertex C: right Z+
        Vector3 vCC = new Vector3(center.x,center.y,center.z+md);
        Vector3 vC1 = vA1;
        Vector3 vC2 = vA2;

        Vector3 vC3 = new Vector3(vCC.x-md,vCC.y+md,vCC.z);
        Vector3 vC4 = vB3;

        // Vertex D: Back X-
        Vector3 vDD = new Vector3(center.x-md,center.y,center.z);
        Vector3 vD1 = vA2;
        Vector3 vD2 = vA3;
        Vector3 vD3 = new Vector3(vDD.x,vDD.y+md,vDD.z-md);
        Vector3 vD4 = vC3;

        // Vertex E: Left Z-
        Vector3 vEE = new Vector3(center.x,center.y,center.z-md);
        Vector3 vE1 = vA3;
        Vector3 vE2 = vA4;
        Vector3 vE3 = vB4;
        Vector3 vE4 = vD3;

        // Vertex F: Top Y+
        Vector3 vFF = new Vector3(center.x,center.y+md,center.z);
        Vector3 vF1 = vB3;
        Vector3 vF2 = vC3;
        Vector3 vF3 = vD3;
        Vector3 vF4 = vB4;

        // DEFORMATIONS //botton
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

        // mediun height
        float sizeMed = size*0.3f;
        vDelta = (vBB-center)* -deformation(rnd,sizeMed);
        vBB += vDelta;
        vDelta = (vCC-center)* -deformation(rnd,sizeMed);
        vCC += vDelta;
        vDelta = (vDD-center)* -deformation(rnd,sizeMed);
        vDD += vDelta;
        vDelta = (vEE-center)* -deformation(rnd,sizeMed);
        vEE += vDelta;

        // top
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

        // rotations
        float angle1 = (2*Mathf.Pi) * GeoAux.FloatRange(rnd,0,1f);
        float angle2 = (2*Mathf.Pi) * GeoAux.FloatRange(rnd,0,1f);
        float angle3 = (2*Mathf.Pi) * GeoAux.FloatRange(rnd,0,1f);

        vAA = vAA.Rotated(Vector3.Up,angle1); vAA = vAA.Rotated(Vector3.Forward,angle2); vAA = vAA.Rotated(Vector3.Right,angle3);
        vA1 = vA1.Rotated(Vector3.Up,angle1); vA1 = vA1.Rotated(Vector3.Forward,angle2); vA1 = vA1.Rotated(Vector3.Right,angle3);
        vA2 = vA2.Rotated(Vector3.Up,angle1); vA2 = vA2.Rotated(Vector3.Forward,angle2); vA2 = vA2.Rotated(Vector3.Right,angle3);
        vA3 = vA3.Rotated(Vector3.Up,angle1); vA3 = vA3.Rotated(Vector3.Forward,angle2); vA3 = vA3.Rotated(Vector3.Right,angle3);
        vA4 = vA4.Rotated(Vector3.Up,angle1); vA4 = vA4.Rotated(Vector3.Forward,angle2); vA4 = vA4.Rotated(Vector3.Right,angle3);

        vBB = vBB.Rotated(Vector3.Up,angle1); vBB = vBB.Rotated(Vector3.Forward,angle2); vBB = vBB.Rotated(Vector3.Right,angle3);
        vB1 = vB1.Rotated(Vector3.Up,angle1); vB1 = vB1.Rotated(Vector3.Forward,angle2); vB1 = vB1.Rotated(Vector3.Right,angle3);
        vB2 = vB2.Rotated(Vector3.Up,angle1); vB2 = vB2.Rotated(Vector3.Forward,angle2); vB2 = vB2.Rotated(Vector3.Right,angle3);
        vB3 = vB3.Rotated(Vector3.Up,angle1); vB3 = vB3.Rotated(Vector3.Forward,angle2); vB3 = vB3.Rotated(Vector3.Right,angle3);
        vB4 = vB4.Rotated(Vector3.Up,angle1); vB4 = vB4.Rotated(Vector3.Forward,angle2); vB4 = vB4.Rotated(Vector3.Right,angle3);

        vCC = vCC.Rotated(Vector3.Up,angle1); vCC = vCC.Rotated(Vector3.Forward,angle2); vCC = vCC.Rotated(Vector3.Right,angle3);
        vC1 = vC1.Rotated(Vector3.Up,angle1); vC1 = vC1.Rotated(Vector3.Forward,angle2); vC1 = vC1.Rotated(Vector3.Right,angle3);
        vC2 = vC2.Rotated(Vector3.Up,angle1); vC2 = vC2.Rotated(Vector3.Forward,angle2); vC2 = vC2.Rotated(Vector3.Right,angle3);
        vC3 = vC3.Rotated(Vector3.Up,angle1); vC3 = vC3.Rotated(Vector3.Forward,angle2); vC3 = vC3.Rotated(Vector3.Right,angle3);
        vC4 = vC4.Rotated(Vector3.Up,angle1); vC4 = vC4.Rotated(Vector3.Forward,angle2); vC4 = vC4.Rotated(Vector3.Right,angle3);

        vDD = vDD.Rotated(Vector3.Up,angle1); vDD = vDD.Rotated(Vector3.Forward,angle2); vDD = vDD.Rotated(Vector3.Right,angle3);
        vD1 = vD1.Rotated(Vector3.Up,angle1); vD1 = vD1.Rotated(Vector3.Forward,angle2); vD1 = vD1.Rotated(Vector3.Right,angle3);
        vD2 = vD2.Rotated(Vector3.Up,angle1); vD2 = vD2.Rotated(Vector3.Forward,angle2); vD2 = vD2.Rotated(Vector3.Right,angle3);
        vD3 = vD3.Rotated(Vector3.Up,angle1); vD3 = vD3.Rotated(Vector3.Forward,angle2); vD3 = vD3.Rotated(Vector3.Right,angle3);
        vD4 = vD4.Rotated(Vector3.Up,angle1); vD4 = vD4.Rotated(Vector3.Forward,angle2); vD4 = vD4.Rotated(Vector3.Right,angle3);

        vEE = vEE.Rotated(Vector3.Up,angle1); vEE = vEE.Rotated(Vector3.Forward,angle2); vEE = vEE.Rotated(Vector3.Right,angle3);
        vE1 = vE1.Rotated(Vector3.Up,angle1); vE1 = vE1.Rotated(Vector3.Forward,angle2); vE1 = vE1.Rotated(Vector3.Right,angle3);
        vE2 = vE2.Rotated(Vector3.Up,angle1); vE2 = vE2.Rotated(Vector3.Forward,angle2); vE2 = vE2.Rotated(Vector3.Right,angle3);
        vE3 = vE3.Rotated(Vector3.Up,angle1); vE3 = vE3.Rotated(Vector3.Forward,angle2); vE3 = vE3.Rotated(Vector3.Right,angle3);
        vE4 = vE4.Rotated(Vector3.Up,angle1); vE4 = vE4.Rotated(Vector3.Forward,angle2); vE4 = vE4.Rotated(Vector3.Right,angle3);

        vFF = vFF.Rotated(Vector3.Up,angle1); vFF = vFF.Rotated(Vector3.Forward,angle2); vFF = vFF.Rotated(Vector3.Right,angle3);
        vF1 = vF1.Rotated(Vector3.Up,angle1); vF1 = vF1.Rotated(Vector3.Forward,angle2); vF1 = vF1.Rotated(Vector3.Right,angle3);
        vF2 = vF2.Rotated(Vector3.Up,angle1); vF2 = vF2.Rotated(Vector3.Forward,angle2); vF2 = vF2.Rotated(Vector3.Right,angle3);
        vF3 = vF3.Rotated(Vector3.Up,angle1); vF3 = vF3.Rotated(Vector3.Forward,angle2); vF3 = vF3.Rotated(Vector3.Right,angle3);
        vF4 = vF4.Rotated(Vector3.Up,angle1); vF4 = vF4.Rotated(Vector3.Forward,angle2); vF4 = vF4.Rotated(Vector3.Right,angle3);

        //scale
        vAA *= scale; vA1 *= scale; vA2 *= scale; vA3 *= scale; vA4 *= scale;
        vBB *= scale; vB1 *= scale; vB2 *= scale; vB3 *= scale; vB4 *= scale;
        vCC *= scale; vC1 *= scale; vC2 *= scale; vC3 *= scale; vC4 *= scale;
        vDD *= scale; vD1 *= scale; vD2 *= scale; vD3 *= scale; vD4 *= scale;
        vEE *= scale; vE1 *= scale; vE2 *= scale; vE3 *= scale; vE4 *= scale;
        vFF *= scale; vF1 *= scale; vF2 *= scale; vF3 *= scale; vF4 *= scale;

        //translate
        vAA += position; vA1 += position; vA2 += position; vA3 += position; vA4 += position;
        vBB += position; vB1 += position; vB2 += position; vB3 += position; vB4 += position;
        vCC += position; vC1 += position; vC2 += position; vC3 += position; vC4 += position;
        vDD += position; vD1 += position; vD2 += position; vD3 += position; vD4 += position;
        vEE += position; vE1 += position; vE2 += position; vE3 += position; vE4 += position;
        vFF += position; vF1 += position; vF2 += position; vF3 += position; vF4 += position;

        // TRIS A 
        GeoAux.createTri(st,vAA,vA2,vA1,color); 
        GeoAux.createTri(st,vAA,vA3,vA2,color); 
        GeoAux.createTri(st,vAA,vA4,vA3,color); 
        GeoAux.createTri(st,vAA,vA1,vA4,color); 

        // TRIS B 
        GeoAux.createTri(st,vBB,vB1,vB2,color); 
        GeoAux.createTri(st,vBB,vB2,vB3,color); 
        GeoAux.createTri(st,vBB,vB3,vB4,color); 
        GeoAux.createTri(st,vBB,vB4,vB1,color); 

        // TRIS C 
        GeoAux.createTri(st,vCC,vC1,vC2,color); 
        GeoAux.createTri(st,vCC,vC2,vC3,color); 
        GeoAux.createTri(st,vCC,vC3,vC4,color); 
        GeoAux.createTri(st,vCC,vC4,vC1,color); 

        // TRIS D 
        GeoAux.createTri(st,vDD,vD1,vD2,color); 
        GeoAux.createTri(st,vDD,vD2,vD3,color); 
        GeoAux.createTri(st,vDD,vD3,vD4,color); 
        GeoAux.createTri(st,vDD,vD4,vD1,color); 

        // TRIS E 
        GeoAux.createTri(st,vEE,vE1,vE2,color); 
        GeoAux.createTri(st,vEE,vE2,vE3,color); 
        GeoAux.createTri(st,vEE,vE3,vE4,color); 
        GeoAux.createTri(st,vEE,vE4,vE1,color); 

        // TRIS F 
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
