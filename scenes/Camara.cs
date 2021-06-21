using Godot;
using System;

public class Camara : Spatial{

    public Boolean playerControl = true; // si el player la controla
    public Boolean isPC = true;
    private Camera camara;

    public override void _Ready() {
        // mobil o PC
        string osname = Godot.OS.GetName();
        if (osname=="Android" || osname=="iOS" ) isPC = false;

        //referancia
        camara = GetNode<Camera>("Camara");
    }

    
    public override void _Process(float delta){
        //inputs del player
        inputPC(delta);
    }

    public override void _UnhandledInput(InputEvent @event){
        if (!playerControl || !isPC) return;
        
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

    // CONTROL PC
    private const float JOYPAD_SENSITIVITY = 0.05f, JOYPAD_DEADZONE = 0.15f;
    private const float ZOOM_MIN = 0.8f, ZOOM_MAX = 1.25f, ZOOM_SPEED = 0.01f;
    private float zoom = 1f; 

    private void inputPC(float delta){
        
        Vector2 vel = new Vector2();
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

        if (Input.IsActionPressed("ui_fast")){
            speed = 2f;
        }

        //mover camara
        float valueX = Mathf.Clamp(vel.x,-1, 1) * JOYPAD_SENSITIVITY;
        float valueY = Mathf.Clamp(vel.y,-1, 1) * JOYPAD_SENSITIVITY;

        TranslateObjectLocal(new Vector3(valueX,0,valueY)*speed*delta*50f);

        // ZOOM camara
        if (Input.IsActionPressed("ui_zoom_in")){
            zoom -= ZOOM_SPEED;
        }

        if (Input.IsActionPressed("ui_zoom_out")){
            zoom += ZOOM_SPEED;
        }

        Vector3 offset = new Vector3(0f,22f,8.5f);
        if (zoom <= ZOOM_MIN) zoom = ZOOM_MIN;
        if (zoom >= ZOOM_MAX) zoom = ZOOM_MAX;
        offset = offset*zoom;
        camara.Translation = offset;
    }


    //AUX
    public Vector3[] getMouseRay(){
        Vector3 vOrigin = camara.GlobalTransform.origin;
        Vector2 v2Mouse = GetViewport().GetMousePosition();
        Vector3 v3Mouse = camara.ProjectPosition(v2Mouse,50);
        return new Vector3[] {vOrigin,v3Mouse};
    }
}
