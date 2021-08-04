using Godot;
using System;

public class Editor : Spatial{
    private Camara camara;
    private WorldEnvironment centro;
    private Map map;

    //GUI REFERENCES
    private Label lblMousePos1, lblCameraPos1, lblSelectedPos1;
    private Label lblMousePos2, lblCameraPos2, lblSelectedPos2;

    private Button buElevations, buStyles, buGeneration, buOptions;

    private Control pElevations, pStyles, pGeneration, pOptions;
    private Control pTools;
    private Control[] panels;
    private String actualToolSelected = "";
    private HexaData lastOrigin = null;
    private Label lblActualTool;
    private Button buUp, buUp2, buDown, buDown2, buStyle, buDetail, buDetailClear, buRoad, buRoadClear, buRivers, buRiverClear;

    private LineEdit lblNameMap;
    private SpinBox sbSeedMap;
    private HSlider sSizeMap,sP1,sH1,sP2,sH2,sP3,sH3;
    private Button buGenMap;

    //DEBUG 
    int ballnumber = 0;
    private RigidBody[] balls; 
    //GENERATION
    private Random random = new Random(); 
    public override void _EnterTree(){ 
        //Things
        centro = GetNode<WorldEnvironment>("center"); 
        map = centro.GetNode<Map>("Map"); 
        camara = centro.GetNode<Camara>("Camara"); 
        spaceState = GetWorld().DirectSpaceState; //physic ray neededs 

        //GUI panel top
        lblActualTool = GetNode<Label>("GUI/UpPanel/HB/HB/lblTool");

        //GUI panel botton
        lblMousePos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB1/Label");
        lblMousePos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB1/Label2");
        lblCameraPos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB2/Label");
        lblCameraPos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB2/Label2");
        lblSelectedPos1 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB3/Label");
        lblSelectedPos2 = GetNode<Label>("GUI/BottomPanel/HB/LPanel/HB/VB/HB3/Label2");

        //right pane buttons
        buElevations = GetNode<Button>("GUI/RightPanel/Right/PButtons/HB/CC/BUElevations"); 
        buStyles= GetNode<Button>("GUI/RightPanel/Right/PButtons/HB/CC2/BUStyles"); 
        buGeneration= GetNode<Button>("GUI/RightPanel/Right/PButtons/HB/CC3/BUGeneration"); 
        buOptions= GetNode<Button>("GUI/RightPanel/Right/PButtons/HB/CC4/BUOptions"); 

        buElevations.Connect("pressed", this, nameof(buttonPanelclick),new Godot.Collections.Array{0});
        buStyles.Connect("pressed", this, nameof(buttonPanelclick),new Godot.Collections.Array{1});
        buGeneration.Connect("pressed", this, nameof(buttonPanelclick),new Godot.Collections.Array{2});
        buOptions.Connect("pressed", this, nameof(buttonPanelclick),new Godot.Collections.Array{3});

        //subpanels
        pTools = GetNode<Control>("GUI/RightPanel/Right/PTools");
        pElevations = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations"); 
        pStyles = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles"); 
        pGeneration = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration");
        pOptions = GetNode<Control>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBOptions"); 

        panels = new Control[]{
            pElevations,
            pStyles,
            pGeneration,
            pOptions
        };

        // TOOLS
        buUp = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations/VB/BuUp");
        buUp2 = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations/VB/BuUp2");
        buDown = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations/VB/BuDown");
        buDown2 = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations/VB/BuDown2");
        buRivers = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations/VB/BuRiver");
        buRiverClear = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/PElevations/VB/BuRiverClear");
        buStyle = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles/VB/BuStyle");

        buDetail = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles/VB/BuDetail");
        buDetailClear = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles/VB/BuDetailClear");
        buRoad = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles/VB/BuRoad");
        buRoadClear = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBStyles/VB/BuRoadClear");

        buUp.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"up"});
        buUp2.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"up2"});
        buDown.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"down"});
        buDown2.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"down2"});
        buRivers.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"river"});
        buRiverClear.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"riverclear"});

        buStyle.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"style"});
        buDetail.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"detail_0"});
        buDetailClear.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"detailClear"});
        buRoad.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"road"});
        buRoadClear.Connect("pressed", this, nameof(buttonToolSelect),new Godot.Collections.Array{"roadClear"});

        // PROCEDURAL GEN
        lblNameMap = GetNode<LineEdit>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB1/txtNameMap");
        sbSeedMap = GetNode<SpinBox>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB4/sbSeedMap");
        sSizeMap = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB2/sSizeMap");
        sP1 = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB5/sP1");
        sH1 = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB6/sH1");
        sP2 = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB7/sP2");
        sH2 = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB8/sH2");
        sP3 = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB9/sP3");
        sH3 = GetNode<HSlider>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB10/sH3");

        buGenMap = GetNode<Button>("GUI/RightPanel/Right/PTools/ScrollContainer/VB/VBGeneration/VB/HB0/MC/BuGenerate");
        buGenMap.Connect("pressed", this, nameof(buttonGenerateTerrain));

        // translate text GUI
        locateTexts();

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
         //map debug
        float [][] datas = { 
            new float[]{1f,2f},//1pass 
            new float[]{3f,6f},//2pass 
            new float[]{10f,20f} //3pass 
        }; 
        generateSimplexNoise(10,0,datas);

        //physics balls debug
        balls = new RigidBody[ballnumber];
        
        for (int i = 0;i<balls.Length;i++){
            RigidBody ball = new RigidBody();
            CollisionShape cs = new CollisionShape();
            SphereShape sphere = new SphereShape();
            sphere.Radius = 0.1f;
            cs.Shape = sphere;
            CSGSphere csgs = new CSGSphere();
            csgs.Radius = 0.1f; csgs.RadialSegments = 12; csgs.Rings = 6;
            ball.AddChild(cs);
            ball.AddChild(csgs);
            centro.AddChild(ball);
            balls[i] = ball;
        }

        map.instanceAllMap(random); //Show all map

        initcamera();//camera to center

        GD.Print("Editor ready");
    }

    public override void _Process(float delta) {
        //debug
        physicDebug();
    }

    public override void _PhysicsProcess(float delta){
        if (camara.isPC) mousePosOver(); //on mobile no over indicator
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
        Vector3 ray = origin + (dir*5);// ray

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
            int r = hx.hexData.row; 
            int c = hx.hexData.col;
            lblCameraPos2.Text = String.Format("({0},{1})",r,c);
            map.cameraRayPosition = map.mapData.getHexPosition(r,c);
        }else{
            lblCameraPos2.Text ="-";
        }
    }

    private void mousePosOver() { 
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3[] mRay = camara.getMouseRay(); 
        Hexagon hx = ray(mRay[0],mRay[1]);
        if (hx != null){
            lblMousePos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col);
            map.moveOver(hx.hexData.row, hx.hexData.col);
        }else{
            lblMousePos2.Text ="";
            map.moveOver(-1,-1);
        }
    }
    
    private void rayoMouseClick(){
        //Control a donde apunta la cámara para saber el punto central del mapa
        Vector3[] mRay = camara.getMouseRay(); 
        Hexagon hx = ray(mRay[0],mRay[1]);
        if (hx != null){
            lblSelectedPos2.Text = String.Format("({0},{1})",hx.hexData.row,hx.hexData.col); 
            
            if (actualToolSelected != ""){
                exeTool(hx.hexData);
            }

            map.moveSelector(hx.hexData.row, hx.hexData.col);

        }else{
            lblSelectedPos2.Text ="";
        }
    }

    //camera init: limits and center
    private void initcamera(){
        //calculate camera limits
        int size = map.mapData.getSize();
        Vector3 pos = map.mapData.getHexPosition(size-1,size-1);
        camara.movelimitsTopLeft = new Vector2 (0,0);
        camara.movelimitsDownRight = new Vector2 (pos.x,pos.z);

        //center point
        pos = map.mapData.getHexPosition(size/2,size/2);
        camara.move(new Vector2(pos.x,pos.z));
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
    private void buttonPanelclick(int buttonIndex){

        Boolean allHides = true;
        for (int i = 0; i< panels.Length;i++){
            if (i==buttonIndex){
                panels[i].Visible = !panels[i].Visible;
            } else{
                panels[i].Visible = false;
            }
            allHides &= !panels[i].Visible;
        }
                
        //panel base se muestra si alguien visible
        pTools.Visible = !allHides;

        //no tools
        actualToolSelected = "";
        lblActualTool.Text = "";

        //si algun panel activo, camara no responde a acciones
        camara.playerControl = allHides;
    }

    private void buttonToolSelect(String toolname){
        buttonPanelclick(-1);//hide panels
        actualToolSelected = toolname;
        lblActualTool.Text = toolname;
    }

    private void buttonGenerateTerrain(){
        //get datas
        int sizeMap = (int)sSizeMap.Value;
        int seed = (int)sbSeedMap.Value;
        
        //pass datas 
        float [][] datas = { 
            new float[]{(float)sP1.Value,(float)sH1.Value},//1pass 
            new float[]{(float)sP2.Value,(float)sH2.Value},//2pass 
            new float[]{(float)sP3.Value,(float)sH3.Value} //3pass 
        }; 

        //generate data terrain
        generateSimplexNoise(sizeMap,seed,datas);

        //Instanciar toda la vista
        map.instanceAllMap(random);

        //camera to center
        initcamera();
    }

    //GENERACION CON RUIDO - BIOMAS
    public void generateSimplexNoise(int sizeMap, int seed, float[][] datas){
        //SIZE
        GD.Print("New map...");
        map.mapData = new MapData(sizeMap);
        float[,] dataTerrain = new float[sizeMap,sizeMap];
        
        //Ramdom
        Random rnd = new Random();
        if (seed == 0) seed = rnd.Next();
        
        //Noise
        OpenSimplexNoise noise = new OpenSimplexNoise();
        noise.Seed = seed;
        noise.Octaves = 8;
        noise.Lacunarity = 1.5f;
        noise.Persistence = 0.02f;

        //parameters algotitmi 
        /*float [][] datas = { 
            new float[]{1,10},//1pass 
            new float[]{3,20},//2pass 
            new float[]{5,30} //3pass 
        }; */

        //1 pass low noise (big areas)
        noise.Period = datas[0][0]; float heigthMulti = datas[0][1];
        for (int i = 0; i<sizeMap; i++){
            for (int j = 0; j<sizeMap; j++){
                dataTerrain[j,i] += noise.GetNoise2d(j,i) * heigthMulti;
            }
        }

        //2 pass (medium elevations)
        noise.Period = datas[1][0]; heigthMulti =datas[1][1];
        for (int i = 0; i<sizeMap; i++){
            for (int j = 0; j<sizeMap; j++){
                dataTerrain[j,i] += noise.GetNoise2d(j,i) * heigthMulti;
            }
        }

        //3 pass (max elevations) and get max and min heights
        noise.Period = datas[2][0]; heigthMulti = datas[2][1];
        float min = float.MaxValue; 
        float max = float.MinValue;
        for (int i = 0; i<sizeMap; i++){
            for (int j = 0; j<sizeMap; j++){
                dataTerrain[j,i] += noise.GetNoise2d(j,i) * heigthMulti;
                //minMax
                float h =  dataTerrain[j,i];
                if (h<min) min = h;
                if (h>max) max = h;
            }
        }

        //pasamos las alturas generedas a mis alturas, necesitamos las cotas:
        for (int i = 0; i<sizeMap; i++){
            for (int j = 0; j<sizeMap; j++){
                float h =  dataTerrain[j,i];
                float value = Mathf.InverseLerp(min,max,h);
                if (value.Equals(float.NaN)) {
                    value = 0;
                }                
                float lerp = Mathf.Lerp(0,9,value);
                int height =  Mathf.RoundToInt(lerp);

                HexaData hexaData = map.mapData.GetHexaData(i,j);
                hexaData.setHeight(height);
            }
        }
    }

    //MANUAL EDITION
    public void exeTool(HexaData hxd){
        
        switch(actualToolSelected){
            case "up": map.upTerrain(hxd); break;
            case "up2": map.up2Terrain(hxd); break;
            case "down":map.downTerrain(hxd); break;
            case "down2":map.down2Terrain(hxd); break;
            case "river":
                map.createRiver(lastOrigin,hxd); 
                lastOrigin = hxd;
                break;
            case "riverclear": 
                map.cleanRivers(hxd); 
                lastOrigin = null;
                break;
            case "detail_0":map.placeGO(hxd,0); break;
            case "detailClear":map.placeGO(hxd,-1); break;
        }
    }

}
