using Godot;
using System;

public class Editor : Spatial{
    Camara camara;
    Map map;

    //GUI REFERENCES
    Label lblMousePos1, lblCameraPos1, lblSelectedPos1;
    Label lblMousePos2, lblCameraPos2, lblSelectedPos2;

    Button buElevations, buStyles, buGeneration, buOptions;

    Control pElevations, pStyles, pGeneration, pOptions;
    private String actualToolSelected = "";
    Label lblActualTool;
    Button buUp, buUp2, buDown, buDown2, buStyle, buDetail, buRoads, buRibers;

    //DEBUG
    RigidBody[] balls;
    Random random = new Random();

    public override void _EnterTree(){
        //Things
        Spatial centro = GetNode<Spatial>("center");
        map = centro.GetNode<Map>("Map");
        camara = centro.GetNode<Camara>("Camara");

        spaceState = GetWorld().DirectSpaceState; //physic ray neededs

        //GUI
        lblMousePos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB1/Label");
        lblCameraPos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB2/Label");
        lblSelectedPos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB3/Label");
        lblMousePos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB1/Label2");
        lblCameraPos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB2/Label2");
        lblSelectedPos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB3/Label2");

        buElevations = GetNode<Button>("GUI/BottomPanel/HB/RPanel/HB/CC/BUElevations"); 
        buStyles= GetNode<Button>("GUI/BottomPanel/HB/RPanel/HB/CC2/BUStyles"); 
        buGeneration= GetNode<Button>("GUI/BottomPanel/HB/RPanel/HB/CC3/BUGeneration"); 
        buOptions= GetNode<Button>("GUI/BottomPanel/HB/RPanel/HB/CC4/BUOptions"); 

        pElevations = GetNode<Control>("GUI/RightPanel/VBElevations"); 
        pStyles = GetNode<Control>("GUI/RightPanel/VBStyles"); 
        pGeneration = GetNode<Control>("GUI/RightPanel/VBGeneration"); 
        pOptions = GetNode<Control>("GUI/RightPanel/VBOptions"); 

        // SIGNALS
        buElevations.Connect("pressed", this, "downbuttonclick",new Godot.Collections.Array{0});
        buStyles.Connect("pressed", this, "downbuttonclick",new Godot.Collections.Array{1});
        buGeneration.Connect("pressed", this, "downbuttonclick",new Godot.Collections.Array{2});
        buOptions.Connect("pressed", this, "downbuttonclick",new Godot.Collections.Array{3});

        // TOOLS
        lblActualTool = GetNode<Label>("GUI/UpPanel/MC/HB/CC2/lblTool");

        buUp = GetNode<Button>("GUI/RightPanel/VBElevations/PContent/VB/HB0/CC/BuUp");
        buUp.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"up"});
        buUp2 = GetNode<Button>("GUI/RightPanel/VBElevations/PContent/VB/HB1/CC/BuUp2");
        buUp2.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"up2"});
        buDown = GetNode<Button>("GUI/RightPanel/VBElevations/PContent/VB/HB2/CC/BuDown");
        buDown.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"down"});
        buDown2 = GetNode<Button>("GUI/RightPanel/VBElevations/PContent/VB/HB3/CC/BuDown2");
        buDown2.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"down2"});

        buStyle = GetNode<Button>("GUI/RightPanel/VBStyles/PContent/VB/HB0/CC/BuStyle");
        buStyle.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"style"});
        buDetail = GetNode<Button>("GUI/RightPanel/VBStyles/PContent/VB/HB1/CC/BuDetail");
        buDetail.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"detail"});
        buRoads = GetNode<Button>("GUI/RightPanel/VBStyles/PContent/VB/HB2/CC/BuRoads");
        buRoads.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"road"});
        buRibers = GetNode<Button>("GUI/RightPanel/VBStyles/PContent/VB/HB3/CC/BuRibers");
        buRibers.Connect("pressed", this, "buttonToolSelect",new Godot.Collections.Array{"riber"});

        // translate text GUI
        locateTexts();

        //physics balls
        balls = new RigidBody[50];
        
        for (int i = 0;i<balls.Length;i++){
            RigidBody ball = new RigidBody();
            CollisionShape cs = new CollisionShape();
            SphereShape sphere = new SphereShape();
            sphere.Radius = 0.1f;
            cs.Shape = sphere;
            CSGSphere csgs = new CSGSphere();
            csgs.Radius = 0.1f; csgs.RadialSegments = 12; csgs.Rings = 6;
            ball.AddChild(cs);ball.AddChild(csgs);centro.AddChild(ball);
            balls[i] = ball;
        }
    }
    private void locateTexts(){
        lblMousePos1.Text = "Mouse:";
        lblCameraPos1.Text = "Camera:";
        lblSelectedPos1.Text = "Selected:";
        lblMousePos2.Text = "";
        lblCameraPos2.Text = "";
        lblSelectedPos2.Text = "";
    }

    public override void _Ready(){
        GD.Print("Editor ready");
    }

    public override void _Process(float delta) {
        //debug
        physicDebug();
    }

    public override void _PhysicsProcess(float delta){
        mousePosOver();
        cameraPosOver();
    }

    public override void _UnhandledInput(InputEvent @event){
        if (@event is InputEventMouseButton ){
            InputEventMouse btn = (InputEventMouse) @event;
            if (btn.IsPressed()){
                
                if (btn.ButtonMask == (int)ButtonList.Left){
                    rayoMouseClick();
                }
                if (btn.ButtonMask == (int)ButtonList.Right){
                    buttonToolSelect("");
                }
            }
        }

    }

    // RAYS CAMEREA AND MOUSE
    private PhysicsDirectSpaceState spaceState;

    private Hexagon ray(Vector3 origin, Vector3 target){
        Hexagon hx = null;
        if (origin == null || target == null) return hx;

        Vector3 dir = target - origin;
        Vector3 ray = origin + dir;// ray

        var result = spaceState.IntersectRay(origin,ray,null,2147483647,true,false);
        if (result.Count > 0){
            Node coll = result["collider"] as Node;
            Node parent = coll.GetParent();
            if (parent is Hexagon){
                hx = parent as Hexagon;
            }
        }
        return hx;
    }
    private void cameraPosOver(){
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3 camTarget = ((Spatial)camara.GetChild(0)).GlobalTransform.origin;
        Vector3 camOrigen = camara.GlobalTransform.origin;
        Hexagon hx = ray(camOrigen,camTarget);
        if (hx != null){
            lblCameraPos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col);
        }
    }

    private void mousePosOver() { 
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3[] mRay = camara.getMouseRay(); 
        Hexagon hx = ray(mRay[0],mRay[1]);
        if (hx != null){
            lblMousePos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col);
        }else{
            lblMousePos2.Text ="";
        }
    }
    
    private void rayoMouseClick(){
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3[] mRay = camara.getMouseRay(); 
        Hexagon hx = ray(mRay[0],mRay[1]);
        if (hx != null){
            lblSelectedPos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col);
            map.moveSelector(hx.hexData.row, hx.hexData.col);
        }else{
            lblSelectedPos2.Text ="";
        }
    }

    //DEBUG PHYSICS
    private void physicDebug(){
        foreach (RigidBody ball in balls){
            if (ball.Translation.y<-10f || ball.Sleeping){
                int row = random.Next(map.mapData.getSize());
                int col = random.Next(map.mapData.getSize());
                
                Vector3 vPos = map.mapData.getHexPosition(row,col);
                vPos = vPos + new Vector3(0,20f,0);
                ball.Translation = vPos;
                ball.Sleeping = false;
                ball.AddTorque(new Vector3(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble()));
            }
        }
    }

    //GUI CONTROLS
    private bool deactivateInput = false;
    private void downbuttonclick(int buttonIndex){

        switch (buttonIndex){
            case 0: 
                pElevations.Visible = !pElevations.Visible; 
                pStyles.Visible = false; 
                pGeneration.Visible = false; 
                pOptions.Visible = false; 
                break;
            case 1:
                pElevations.Visible = false;  
                pStyles.Visible =  !pStyles.Visible; 
                pGeneration.Visible = false; 
                pOptions.Visible = false; 
                break;
            case 2: 
                pElevations.Visible = false;  
                pStyles.Visible = false; 
                pGeneration.Visible = !pGeneration.Visible; 
                pOptions.Visible = false; 
                break;
            case 3: 
                pElevations.Visible = false; 
                pStyles.Visible = false; 
                pGeneration.Visible = false; 
                pOptions.Visible = !pOptions.Visible; 
                break;
            default:
                pElevations.Visible = false; 
                pStyles.Visible = false; 
                pGeneration.Visible = false; 
                pOptions.Visible = false;
                break;
        }
    }

    private void buttonToolSelect(String toolname){
        actualToolSelected = toolname;
        lblActualTool.Text = toolname;
        downbuttonclick(-1);
    }

}
