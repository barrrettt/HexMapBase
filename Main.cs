using Godot;
using System;

public class Main : Node{
    // Main 
    public override void _Ready(){ 
        GD.Print("App running..."); 
        // Input.SetMouseMode(Input.MouseMode.Confined); //mouse confined? 
        OS.WindowMaximized = true; // Maxi? 
        
        // Load first scene directly 
        PackedScene packedScene; 
        packedScene = (PackedScene) GD.Load("res://scenes/Editor.tscn");//otra escena 
        //packedScene = (PackedScene) GD.Load("res://scenes/Test.tscn");//otra escena 
        AddChild(packedScene.Instance()); 
    }
}
