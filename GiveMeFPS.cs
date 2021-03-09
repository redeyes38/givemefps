using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;

/// <summary>
/// Give Me FPS Version 1.0.0
/// By Redeyes
/// Session plugin to quickly set ALL person options to give more frames per second at diffently levels to user requirements
/// </summary>

namespace Redeyes{

    public class GiveMeFPS : MVRScript
    {
        private UIDynamicButton btn;
        protected JSONStorableBool ClothSimState;
        private bool _dirty;
        public override void Init()
        {
            try
            {
                ClothSimState = new JSONStorableBool("Toggle all clothes Sim Off/On", false);
                RegisterBool(ClothSimState);

                btn = CreateButton("Give me some FPS - Recommend");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 3, 0.00045000f, 1, true, false, false, "Quality", true, "1", "1024"); });

                btn = CreateButton("Give me some FPS - Hair Only");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 3, 0.00045000f, 1, true, true, true, "Quality", false, "8", "1024"); });

                btn = CreateButton("Give me ALL the FPS!");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(false, 10, 2, 0.00065000f, 1, false, false, false, "Fast", true, "1", "512"); });

                btn = CreateButton("Default");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 16, 0.00010000f, 2, true, true, true, "Quality", false , "8", "1024"); });

                UIDynamicToggle onToggle = CreateToggle(ClothSimState, false);
                onToggle.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleSimClothFPS(on);
                });

                SetupInfoText(this, 
                    "<color=#606060><size=40><b>Give Me FPS v1.0</b></size>\nA Session Plugin.\n" +
                    "These will set softbody physics for Tongue, breast & glute on/off to gain fps</color>\n\n" +
                    "<b>Give me some FPS - Recommend:</b> Turns off Tongue & Glute softbody physics, breasts on, Hair Curve Density 16 - Multiplier 3 - strand width 0.00045 - iterations 1, Quality hair shader, disable pixel lights reflections and anti aliasing 1\n\n" +
                    "<b>Give me some FPS - Hair Only:</b> Same as recommend but only hair parameters adjusted\n\n" +
                    "<b>Give me ALL the FPS!:</b> In addition Turns off breast softbody physics, Hair sim off, Hair Curve Density 10 - Multiplier 2, Fast hair shader, reflections texture size 512.\n\n" +
                    "<b>Default:</b> Switch all settings back to VAM defaults (this isn't the same as the scene loaded with)\n\n" +
                    "<b>Toggle all clothes Sim Off/On:</b> Toggles all clothing simulation Off / On \n\n",
                    1100.0f, true
                );
            }
            catch (Exception e)
            {
                SuperController.LogError("Failed to initialize plugin: " + e);
            }
        }


        public void ExecuteGiveMeFPS(bool HairSimulationState, float CurveDensityValue, float HairMultiplierValue, float widthValue, float iterationsValue, bool BreastPhysicsState, bool LowerPhysicsState, bool TongueControlState, string shaderTypeValue, bool disablePixelLightsState, string antiAliasingValue, string textureSizeValue )
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetBoolParamValue("simulationEnabled", HairSimulationState);
                            hairControl.SetFloatParamValue("curveDensity", CurveDensityValue);
                            hairControl.SetFloatParamValue("hairMultiplier", HairMultiplierValue);
                            hairControl.SetFloatParamValue("width", widthValue);
                            hairControl.SetFloatParamValue("iterations", iterationsValue);
                            hairControl.SetStringChooserParamValue("shaderType", shaderTypeValue);
                        }
                        //SuperController.LogMessage("test = " + hairControl);
                    }
                    atom.GetStorableByID("BreastPhysicsMesh").GetBoolJSONParam("on").val=BreastPhysicsState;
                    atom.GetStorableByID("LowerPhysicsMesh").GetBoolJSONParam("on").val=LowerPhysicsState;
                    atom.GetStorableByID("TongueControl").GetBoolJSONParam("tongueCollision").val=TongueControlState;
                }

                if (atom.GetStorableByID("MirrorRender"))
                {
                    atom.GetStorableByID("MirrorRender").GetBoolJSONParam("disablePixelLights").val=disablePixelLightsState;
                    atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("antiAliasing").val = antiAliasingValue;
                    atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("textureSize").val = textureSizeValue;
                }
                
            }
        }

        public void ToggleSimClothFPS(bool ClothSimState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZClothingItem clothGroup in atom.GetComponentsInChildren<DAZClothingItem>())
                    {
                        ClothSimControl clothControl = clothGroup.GetComponentInChildren<ClothSimControl>();
                        clothControl.SetBoolParamValue("simEnabled", ClothSimState);
                        SuperController.LogMessage("ClothSimState = " + clothControl.GetBoolParamValue("simEnabled"));
                    }
                }
            }
        }


        private static JSONStorableString SetupInfoText(MVRScript script, string text, float height, bool rightSide)
        {
            JSONStorableString storable = new JSONStorableString("Info", text);
            UIDynamic textfield = script.CreateTextField(storable, rightSide);
            textfield.height = height;
            return storable;
        }

    }
}
