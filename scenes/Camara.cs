using Godot;
using System;
using System.Collections.Generic;

public class Camara : Spatial{

    public Boolean playerControl = true; // si el player la controla
    public Boolean isPC = true;
    private Camera camara;
    private Vector3 offsetCamera = new Vector3(0f,20.25f,11.25f);

    public override void _Ready() {
        // mobil o PC
        string osname = Godot.OS.GetName();
        if (osname=="Android" || osname=="iOS" ) isPC = false;

        //referancia
        camara = GetNode<Camera>("Camara");
    }

    public override void _Process(float delta){
        if (!playerControl) return;
        if (isPC) inputPC(delta);
    }

    public override void _UnhandledInput(InputEvent @event){
        if (!playerControl) return;
        if (isPC){
            unhadledPc( @event);
        } else{
            unhadledMobile(@event);
        }
    }

    // CONTROL PC
    private const float SENSITIVITY_PC = 0.05f, JOYPAD_DEADZONE = 0.15f;
    private const float ZOOM_MIN = 0.2f, ZOOM_MAX = 10f, ZOOM_SPEED = 0.01f;
    private float zoom = 1f; 

    private void inputPC(float delta){
        Vector2 vel = new Vector2();
        Vector2 rot = new Vector2();
        float speed = 1;

        //ROTACIONES DE JOYPADS
        if (Input.GetConnectedJoypads().Count > 0){
            if (Godot.OS.GetName()=="Windows"){
                vel = new Vector2(Input.GetJoyAxis(0, 2), Input.GetJoyAxis(0, 3));
            }else{
                vel = new Vector2(Input.GetJoyAxis(0, 2), Input.GetJoyAxis(0, 4));
            }

            //zona muerta: sin temblor
            float lVel = vel.Length();
            if ( lVel < JOYPAD_DEADZONE){
                vel = new Vector2();
            }else{
                vel = vel.Normalized() * ((lVel - JOYPAD_DEADZONE) / (1 - JOYPAD_DEADZONE));
            }
        }

        //CON TECLADO
        if (Input.IsActionPressed("ui_left")){
             vel.x = -1f;
        }
        if (Input.IsActionPressed("ui_right")){
             vel.x = +1f;
        }
        if (Input.IsActionPressed("ui_up")){
             vel.y = -1f;
        }
        if (Input.IsActionPressed("ui_down")){
             vel.y = +1f;
        }
        //rotates
        if (Input.IsActionPressed("ui_turn_right")){
            rot.y = +1f;
        }
        if (Input.IsActionPressed("ui_turn_left")){
            rot.y = -1f;
        }
        if (Input.IsActionPressed("ui_page_up")){
            rot.x = +1f;
        }
        if (Input.IsActionPressed("ui_page_down")){
            rot.x = -1f;
        }
        //fast
        if (Input.IsActionPressed("ui_fast")){
            speed = 2f;
        }

        // ZOOM camara
        if (Input.IsActionPressed("ui_zoom_in")){
            zoom -= ZOOM_SPEED;
        }

        if (Input.IsActionPressed("ui_zoom_out")){
            zoom += ZOOM_SPEED;
        }

        //mover camara
        vel.x = Mathf.Clamp(vel.x,-1, 1) * SENSITIVITY_PC;
        vel.y = Mathf.Clamp(vel.y,-1, 1) * SENSITIVITY_PC;
        move (vel,rot,speed,delta);

    }

    private void unhadledPc(InputEvent @event){
         //PC ZOOM with mouse wheel
        if (@event is InputEventMouseButton){
            InputEventMouseButton emb = (InputEventMouseButton)@event;
            if (emb.IsPressed()){
                if (emb.ButtonIndex == (int)ButtonList.WheelUp){
                    zoom -= ZOOM_SPEED*2;
                }
                if (emb.ButtonIndex == (int)ButtonList.WheelDown){
                    zoom += ZOOM_SPEED*2;
                }
            }
        }
    }

    // MOBILES
    private int countTouches = 0;
    private InputEvent firstTouch;
    private const float SENSITIVITY_MOBILE =  2e-4f, SENSITIVITY_ZOOM_MOBILE = 2e-5f;
    private void unhadledMobile(InputEvent @event){
        if (@event is InputEventScreenTouch){
            InputEventScreenTouch evTouch = (InputEventScreenTouch)@event;
            if (evTouch.IsPressed()){ 
                countTouches = evTouch.Index+1;
                if (countTouches == 1) firstTouch = evTouch;
            }else{
                countTouches = 0;
                firstTouch = null;
            }
        }

        Vector2 vel = new Vector2();
        Vector2 rot = new Vector2();

        if (@event is InputEventScreenDrag){
            InputEventScreenDrag evDrag = (InputEventScreenDrag)@event;
            
            if (countTouches == 1){
                vel = -evDrag.Relative;
                if (vel.LengthSquared()>2){
                    //GD.Print(" move: " + vel.ToString());
                    move(vel,rot, SENSITIVITY_MOBILE,1f);
                }
            }

            if (countTouches>1){
                if (firstTouch == null) {
                    return;
                }
                    
                Vector2 firstPos = ((InputEventScreenTouch)firstTouch).Position;
                Vector2 dirInit = evDrag.Position - evDrag.Relative;
                
                float initdist = (firstPos - dirInit).Length();
                float findist = (firstPos - evDrag.Position).Length();
                float leght = (firstPos - evDrag.Position).Length();

                if (initdist>findist){
                    zoom += leght * SENSITIVITY_ZOOM_MOBILE;
                }else{ 
                    zoom -= leght * SENSITIVITY_ZOOM_MOBILE;
                }
                
                move(vel,rot,SENSITIVITY_MOBILE,1f);
                //GD.Print(String.Format(" spin: {0}, {1}",initdist.ToString(),findist.ToString()));
            }
        }

    }

    // AUX movement, rotation and zoom
    private void move(Vector2 vel,Vector2 rot, float speed,float delta){

        RotateObjectLocal(Vector3.Up,rot.y*speed*delta*1f);
        camara.RotateObjectLocal(Vector3.Right,rot.x*speed*delta*0.1f);
        TranslateObjectLocal(new Vector3(vel.x,0,vel.y)*speed*delta*50f);

        if (zoom <= ZOOM_MIN) zoom = ZOOM_MIN;
        if (zoom >= ZOOM_MAX) zoom = ZOOM_MAX;
        Vector3 result = offsetCamera * zoom;
        camara.Translation = result;
    }

    //AUX Ray
    public Vector3[] getMouseRay(){
        Vector3 vOrigin = camara.GlobalTransform.origin;
        Vector2 v2Mouse = GetViewport().GetMousePosition();
        Vector3 v3Mouse = camara.ProjectPosition(v2Mouse,50);
        return new Vector3[] {vOrigin,v3Mouse};
    }
}
