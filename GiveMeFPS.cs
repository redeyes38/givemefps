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
        protected JSONStorableBool HairSimulationState;
        protected JSONStorableBool BreastPhysicsState;
        protected JSONStorableBool LowerPhysicsState;
        protected JSONStorableBool TonguePhysicsState;
        protected JSONStorableBool disablePixelLightsState;
        private JSONStorableFloat HairMultiplierValue;
        private JSONStorableFloat CurveDensityValue;
        private JSONStorableFloat HairWidthValue;
        private JSONStorableFloat iterationsValue;

        private UIDynamicToggle onToggleHairSimulationState;
        private UIDynamicToggle onToggleBreastPhysicsState;
        private UIDynamicToggle onToggleLowerPhysicsState;
        private UIDynamicToggle onToggleTonguePhysicsState;
        private UIDynamicToggle onToggledisablePixelLightState;

        private JSONStorableStringChooser shaderChooser;
        private JSONStorableStringChooser reflectionTextureSizeChooser;

        private UIDynamicSlider iterationsSlider;
        private UIDynamicSlider HairMultiplierSlider;
        private UIDynamicSlider CurveDensitySlider;
        private UIDynamicSlider HairWidthValueSlider;
        
        private List<string> shaderChoices = new List<string>();
        private List<string> reflectionTextureChoices = new List<string>();

        private bool _dirty;
        public override void Init()
        {
            try
            {

                //start if user fine tuning
                SetupInfoText(this, "<color=#606060><size=40><b>Quick Buttons</b></size></color>", 20.0f, false);

                //Quick buttons
                btn = CreateButton("Give me some FPS - Recommend");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 3, 0.00045000f, 1, true, false, false, "Quality", true, "1", "1024"); });

                btn = CreateButton("Give me some FPS - Hair Only");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 3, 0.00045000f, 1, true, true, true, "Quality", false, "8", "1024"); });

                btn = CreateButton("Give me ALL the FPS!");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(false, 10, 2, 0.00065000f, 1, false, false, false, "Fast", true, "1", "512"); });

                btn = CreateButton("VAM Default");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 16, 0.00010000f, 2, true, true, true, "Quality", false , "8", "1024"); });

                //cloth sim toggle
                ClothSimState = new JSONStorableBool("All clothes Sim Off/On", true);
                RegisterBool(ClothSimState);
                UIDynamicToggle onToggleClothSimState = CreateToggle(ClothSimState, false);
                onToggleClothSimState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleSimClothFPS(on);
                });

                SetupInfoText(this, 
                    "<color=#606060><size=40><b>Give Me FPS v2.3</b></size>\nA Session Plugin.\n" +
                    "These will set softbody physics for Tongue, breast & glute on/off to gain fps\n\n" +
                    "4 Quick buttons and cloth sim - with user fine tuning of the options the 4 buttons use</color>\n\n" +
                    "<b>Give me some FPS - Recommend:</b> Turns off Tongue & Glute softbody physics, breasts on, Hair Curve Density 16 - Multiplier 3 - strand width 0.00045 - iterations 1, Quality hair shader, disable pixel lights reflections and anti aliasing 1\n\n" +
                    "<b>Give me some FPS - Hair Only:</b> Hair same as recommend but all other options VAM default\n\n" +
                    "<b>Give me ALL the FPS!:</b> In addition to recommended - Turns off breast softbody physics, Hair sim off, Hair Curve Density 10 - Multiplier 2, reflections texture size 512.\n\n" +
                    "<b>Default:</b> Switch all settings back to VAM defaults (this isn't the same as the scene loaded with)\n\n" +
                    "<b>Toggle all clothes Sim Off/On:</b> Toggles all clothing simulation Off / On \n\n",
                    2100.0f, false
                );

                //start if user fine tuning
                SetupInfoText(this, "<color=#606060><size=40><b>User Fine Tuning</b></size></color>", 20.0f, true);

                //Hair sim
                HairSimulationState = new JSONStorableBool("All Hair Sim Off/On", true);
                RegisterBool(HairSimulationState);
                onToggleHairSimulationState = CreateToggle(HairSimulationState, true);
                onToggleHairSimulationState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleHairSimulationFPS(on);
                });

                //hair sliders, sim iterations & shader control
                HairMultiplierValue = new JSONStorableFloat("Hair Multiplier", 3f, HairMultiplierCallback, 1f, 64f, false);
                RegisterFloat(HairMultiplierValue);
                HairMultiplierSlider = CreateSlider(HairMultiplierValue, true);
                HairMultiplierSlider.slider.wholeNumbers = true;
                HairMultiplierSlider.rangeAdjustEnabled = false;

                CurveDensityValue = new JSONStorableFloat("Curve Density", 16f, CurveDensityCallback, 2f, 64f, false);
                RegisterFloat(CurveDensityValue);
                CurveDensitySlider = CreateSlider(CurveDensityValue, true);
                CurveDensitySlider.slider.wholeNumbers = true;
                CurveDensitySlider.rangeAdjustEnabled = false;

                HairWidthValue = new JSONStorableFloat("Hair Width", 0.00045f, HairWidthValueCallback, 0.00000f, 0.00100f, false);
                RegisterFloat(HairWidthValue);
                HairWidthValueSlider = CreateSlider(HairWidthValue, true);
                HairWidthValueSlider.rangeAdjustEnabled = false;
                HairWidthValueSlider.valueFormat = "F5";

                iterationsValue = new JSONStorableFloat("Sim iterations", 1f, iterationsCallback, 1f, 5f, false);
                RegisterFloat(iterationsValue);
                iterationsSlider = CreateSlider(iterationsValue, true);
                iterationsSlider.slider.wholeNumbers = true;
                iterationsSlider.rangeAdjustEnabled = false;
                iterationsSlider.quickButtonsEnabled  = false;

                shaderChoices.Add("Fast");
                shaderChoices.Add("Quality");
                shaderChoices.Add("QualityThicken");
                shaderChoices.Add("QualityThickenMore");
                shaderChooser = new JSONStorableStringChooser("Hair shader", shaderChoices, "Quality", "Hair shader", doShaderChoice);
                RegisterStringChooser(shaderChooser);
                CreatePopup(shaderChooser, true);

                //Softbody physcis
                BreastPhysicsState = new JSONStorableBool("Breast softbody Sim Off/On", true);
                RegisterBool(BreastPhysicsState);
                onToggleBreastPhysicsState = CreateToggle(BreastPhysicsState, true);
                onToggleBreastPhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleBreastPhysicsFPS(on);
                });

                LowerPhysicsState = new JSONStorableBool("Glute softbody Sim Off/On", true);
                RegisterBool(LowerPhysicsState);
                onToggleLowerPhysicsState = CreateToggle(LowerPhysicsState, true);
                onToggleLowerPhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleLowerPhysicsFPS(on);
                });

                TonguePhysicsState = new JSONStorableBool("Tongue softbody Sim Off/On", true);
                RegisterBool(TonguePhysicsState);
                onToggleTonguePhysicsState = CreateToggle(TonguePhysicsState, true);
                onToggleTonguePhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleTonguePhysicsFPS(on);
                });

                //reflection controls
                disablePixelLightsState = new JSONStorableBool("Reflection disable pixel shader Off/On", false);
                RegisterBool(disablePixelLightsState);
                onToggledisablePixelLightState = CreateToggle(disablePixelLightsState, true);
                onToggledisablePixelLightState.toggle.onValueChanged.AddListener((on) =>
                {
                    onToggledisablePixelLightStateFPS(on);
                });

                reflectionTextureChoices.Add("512");
                reflectionTextureChoices.Add("1024");
                reflectionTextureChoices.Add("2048");
                reflectionTextureChoices.Add("4096");
                reflectionTextureSizeChooser = new JSONStorableStringChooser("Reflection Texture Size", reflectionTextureChoices, "1024", "Reflection Texture Size", doReflectionTextureChoice);
                RegisterStringChooser(reflectionTextureSizeChooser);
                CreatePopup(reflectionTextureSizeChooser, true);

            }
            catch (Exception e)
            {
                SuperController.LogError("Failed to initialize plugin: " + e);
            }
        }


        public void ExecuteGiveMeFPS(bool HairSimulationState, float CurveDensityValue, float HairMultiplierValue, float HairWidthValue, float iterationsValue, bool BreastPhysicsState, bool LowerPhysicsState, bool TonguePhysicsState, string shaderTypeValue, bool disablePixelLightsState, string antiAliasingValue, string textureSizeValue )
        {
            //ToggleHairSimulationFPS(HairSimulationState);
            onToggleHairSimulationState.toggle.isOn = HairSimulationState;
            onToggleBreastPhysicsState.toggle.isOn = BreastPhysicsState;
            onToggleLowerPhysicsState.toggle.isOn = LowerPhysicsState;
            onToggleTonguePhysicsState.toggle.isOn = TonguePhysicsState;
            onToggledisablePixelLightState.toggle.isOn = disablePixelLightsState;
            iterationsSlider.slider.value = iterationsValue;
            HairMultiplierSlider.slider.value = HairMultiplierValue;
            CurveDensitySlider.slider.value = CurveDensityValue;
            HairWidthValueSlider.slider.value = HairWidthValue;
            shaderChooser.val = shaderTypeValue;
            reflectionTextureSizeChooser.val = textureSizeValue;

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
                            hairControl.SetFloatParamValue("width", HairWidthValue);
                            hairControl.SetFloatParamValue("iterations", iterationsValue);
                            hairControl.SetStringChooserParamValue("shaderType", shaderTypeValue);
                        }
                        //SuperController.LogMessage("test = " + hairControl);
                    }
                    atom.GetStorableByID("BreastPhysicsMesh").GetBoolJSONParam("on").val=BreastPhysicsState;
                    atom.GetStorableByID("LowerPhysicsMesh").GetBoolJSONParam("on").val=LowerPhysicsState;
                    atom.GetStorableByID("TongueControl").GetBoolJSONParam("tongueCollision").val=TonguePhysicsState;
                }

                if (atom.GetStorableByID("MirrorRender"))
                {
                    atom.GetStorableByID("MirrorRender").GetBoolJSONParam("disablePixelLights").val=disablePixelLightsState;
                    atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("antiAliasing").val = antiAliasingValue;
                    atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("textureSize").val = textureSizeValue;
                }
                
            }
        }

        public void doReflectionTextureChoice(string textureSizeValue)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.GetStorableByID("MirrorRender"))
                {
                    atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("textureSize").val = textureSizeValue;
                }
            }
        }

        public void doShaderChoice(string shaderTypeValue)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetStringChooserParamValue("shaderType", shaderTypeValue);
                        }
                    }
                }
            }
        }

        public void ToggleBreastPhysicsFPS(bool BreastPhysicsState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    atom.GetStorableByID("BreastPhysicsMesh").GetBoolJSONParam("on").val=BreastPhysicsState;

                }
            }
        }
        public void ToggleLowerPhysicsFPS(bool LowerPhysicsState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    atom.GetStorableByID("LowerPhysicsMesh").GetBoolJSONParam("on").val=LowerPhysicsState;

                }
            }
        }
        public void ToggleTonguePhysicsFPS(bool TonguePhysicsState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    atom.GetStorableByID("TongueControl").GetBoolJSONParam("tongueCollision").val=TonguePhysicsState;

                }
            }
        }

        
        public void onToggledisablePixelLightStateFPS(bool disablePixelLightsState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.GetStorableByID("MirrorRender"))
                {
                    atom.GetStorableByID("MirrorRender").GetBoolJSONParam("disablePixelLights").val=disablePixelLightsState;
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

        public void ToggleHairSimulationFPS(bool HairSimulationState)
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
                        }
                    }
                }
            }
        }

        protected void HairMultiplierCallback(JSONStorableFloat HairMultiplierValue)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetFloatParamValue("hairMultiplier", HairMultiplierValue.val);
                        }

                    }
                }
            }
        }

        protected void CurveDensityCallback(JSONStorableFloat CurveDensityValue)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetFloatParamValue("curveDensity", CurveDensityValue.val);
                        }

                    }
                }
            }
        }

        protected void iterationsCallback(JSONStorableFloat iterationsCallback)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetFloatParamValue("iterations", iterationsCallback.val);
                        }

                    }
                }
            }
        }

        protected void HairWidthValueCallback(JSONStorableFloat HairWidthValue)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetFloatParamValue("width", HairWidthValue.val);
                        }

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
