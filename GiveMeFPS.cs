using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;

/// <summary>
/// Give Me FPS Version v3.2.1
/// By Redeyes
/// Session plugin to quickly set ALL person options to give more frames per second at diffently levels to user requirements
/// </summary>

namespace Redeyes{

    public class GiveMeFPS : MVRScript
    {
        private UIDynamicButton btn;

        private JSONStorableBool clothSimState;

        private JSONStorableBool HairSimulationState;
        private UIDynamicToggle onToggleHairSimulationState;

        private JSONStorableBool BreastPhysicsState;
        private UIDynamicToggle onToggleBreastPhysicsState;

        private JSONStorableBool LowerPhysicsState;
        private UIDynamicToggle onToggleLowerPhysicsState;

        private JSONStorableBool TonguePhysicsState;
        private UIDynamicToggle onToggleTonguePhysicsState;

        private JSONStorableBool disablePixelLightsState;
        private UIDynamicToggle onToggledisablePixelLightState;

        private JSONStorableFloat HairMultiplierValue;
        private UIDynamicSlider HairMultiplierSlider;
        
        private JSONStorableFloat CurveDensityValue;
        private UIDynamicSlider CurveDensitySlider;

        private JSONStorableFloat HairWidthValue;
        private UIDynamicSlider HairWidthValueSlider;

        private JSONStorableFloat iterationsValue;
        private UIDynamicSlider iterationsSlider;

        private JSONStorableStringChooser shaderChooser;
        private List<string> shaderChoices = new List<string>();

        private JSONStorableStringChooser reflectionTextureSizeChooser;
        private List<string> reflectionTextureChoices = new List<string>();

        private JSONStorableFloat pixelLightCountValue;
        private UIDynamicSlider pixelLightCountSlider;

        private JSONStorableStringChooser msaaLevelchooser;
        private List<string> msaaLevelChoices = new List<string>();

        private JSONStorableStringChooser ShaderLODChooser;
        private List<string> ShaderLODChoices = new List<string>();

        private JSONStorableFloat renderScaleValue;
        private UIDynamicSlider renderScaleSlider;

        private JSONStorableFloat smoothPassesValue;
        private UIDynamicSlider smoothPassesSlider;

        private JSONStorableStringChooser GlowEffectsLevelChooser;
        private List<string> GlowEffectsLevelChoices = new List<string>();

        private JSONStorableBool mirrorReflectionsState;
        private UIDynamicToggle onTogglemirrorReflectionsState;

        private JSONStorableBool softPhysicsState;
        private UIDynamicToggle onTogglesoftPhysicsState;

        private JSONStorableFloat physicsUpdateCapValue;
        private UIDynamicSlider physicsUpdateCapSlider;

        private JSONStorableStringChooser physicsRateChooser;
        private List<string> physicsRateChoices = new List<string>();

        private JSONStorableBool clothSimState_orginal;
        private JSONStorableBool softPhysicsState_orginal;
        private JSONStorableStringChooser msaaLevelchooser_orginal;
        private JSONStorableFloat pixelLightCountValue_orginal;
        private JSONStorableBool mirrorReflectionsState_orginal;
        private JSONStorableFloat smoothPassesValue_orginal;
        private JSONStorableStringChooser ShaderLODChooser_orginal;

        private JSONStorableBool overrideScenePreferencesState;

        private string msaa_popup;
        private float count=0;
        private bool first_time_bench = true;

        public override void Init()
        {
            try
            {
                SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;
                SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;
                
                if (containingAtom.type != "CoreControl" && containingAtom.type != "SessionPluginManager")
                {
                    SuperController.LogError($"Please load GiveMeFPS as a Session or Scene Plugin, not with a '{containingAtom.type}' atom.");
                    DestroyImmediate(this);
                    return;
                }
                //start if user fine tuning
                SetupInfoText(this, "<color=#606060><size=40><b>Quick Buttons</b></size></color>", 20.0f, false);
                
                //Quick buttons
                btn = CreateButton("Give me some FPS - Recommend");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 3, 0.00045000f, 1, true, false, false, "Quality", true, "1", "1024", false); });

                btn = CreateButton("Give me some FPS - Hair Only");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 3, 0.00045000f, 1, true, true, true, "Quality", false, "8", "1024", true); });

                btn = CreateButton("Give me ALL the FPS!");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(false, 10, 2, 0.00065000f, 1, false, false, false, "Fast", true, "1", "512", false); });

                btn = CreateButton("Atoms VAM Defaults");
                btn.button.onClick.AddListener(() => { ExecuteGiveMeFPS(true, 16, 16, 0.00010000f, 2, true, true, true, "Quality", false , "8", "1024", false); });

                //cloth sim toggle
                clothSimState = new JSONStorableBool("Disable All cloth Sim ", false);
                clothSimState_orginal = new JSONStorableBool("Disable All cloth Sim ", false);
                RegisterBool(clothSimState);
                UIDynamicToggle onToggleClothSimState = CreateToggle(clothSimState, false);
                onToggleClothSimState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleSimClothFPS(on);
                });

                //Override scene preferences
                SetupInfoText(this,
                    "<color=#606060><size=40><b>Override Atom Settings On Scene Load</b></size>\n" +
                    "Tick this to apply these settings when loading any scene. " +
                    "Make sure to save 'Session Plugin Presets -> Change User Defaults -> Set Current As User Defaults' to make this permanent. Does not include performance preferences </color>\n\n",
                    380.0f, false
                );
                overrideScenePreferencesState = new JSONStorableBool("Override On Scene Load ", false);
                RegisterBool(overrideScenePreferencesState);
                UIDynamicToggle onToggleoverrideScenePreferencesState = CreateToggle(overrideScenePreferencesState, false);
                onToggleoverrideScenePreferencesState.toggle.onValueChanged.AddListener((on) =>
                {
                    SuperController.LogMessage("Override Scene Preferences on load = " + on);
                });

                //Preferences - Performance
                SetupInfoText(this, "<color=#606060><size=40><b>Performance Preferences</b></size></color>", 20.0f, false);

                //renderscale
                renderScaleValue = new JSONStorableFloat("Render Scale", UserPreferences.singleton.renderScale, renderScaleCallback, 0.5f, 2.0f, true);
                RegisterFloat(renderScaleValue);
                renderScaleSlider = CreateSlider(renderScaleValue, false);
                renderScaleSlider.rangeAdjustEnabled = false;
                //renderScaleSlider.quickButtonsEnabled  = false;

                //mirrorReflections
                mirrorReflectionsState = new JSONStorableBool("Mirror Reflection", UserPreferences.singleton.mirrorReflections);
                mirrorReflectionsState_orginal = new JSONStorableBool("Mirror Reflection", UserPreferences.singleton.mirrorReflections);
                RegisterBool(mirrorReflectionsState);
                onTogglemirrorReflectionsState = CreateToggle(mirrorReflectionsState, false);
                onTogglemirrorReflectionsState.toggle.onValueChanged.AddListener((on) =>
                {
                    TogglemirrorReflectionsFPS(on);
                });

                //Soft physics
                softPhysicsState = new JSONStorableBool("Soft Body Physics", UserPreferences.singleton.softPhysics);
                softPhysicsState_orginal = new JSONStorableBool("Soft Body Physics", UserPreferences.singleton.softPhysics);
                RegisterBool(softPhysicsState);
                onTogglesoftPhysicsState = CreateToggle(softPhysicsState, false);
                onTogglesoftPhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    TogglesoftPhysicsFPS(on);
                });

                //SHADER
                ShaderLODChoices.Add("Low");
                ShaderLODChoices.Add("Medium");
                ShaderLODChoices.Add("High");
                ShaderLODChooser = new JSONStorableStringChooser("Shader Quality", ShaderLODChoices, UserPreferences.singleton.shaderLOD.ToString(), "Shader Quality", doShaderLODChoice);
                ShaderLODChooser_orginal = new JSONStorableStringChooser("Shader Quality", ShaderLODChoices, UserPreferences.singleton.shaderLOD.ToString(), "Shader Quality", doShaderLODChoice);
                RegisterStringChooser(ShaderLODChooser);
                CreatePopup(ShaderLODChooser, false);

                //msaa
                msaaLevelChoices.Add("Off");
                msaaLevelChoices.Add("2x");
                msaaLevelChoices.Add("4x");
                msaaLevelChoices.Add("8x");
                switch( UserPreferences.singleton.msaaLevel)
                {
                  case 0:
                    msaa_popup = "Off";
                    break;
                  case 2:
                    msaa_popup = "2x";
                    break;
                  case 4:
                    msaa_popup = "4x";
                    break;
                  case 8:
                    msaa_popup = "8x";
                    break;
                  default:
                    msaa_popup = "8x";
                    break;
                }
                msaaLevelchooser = new JSONStorableStringChooser("MSAA Level", msaaLevelChoices, msaa_popup, "MSAA Level", doMSAAlevelChoice);
                msaaLevelchooser_orginal = new JSONStorableStringChooser("MSAA Level", msaaLevelChoices, msaa_popup, "MSAA Level", doMSAAlevelChoice);
                RegisterStringChooser(msaaLevelchooser);
                CreatePopup(msaaLevelchooser, false);

                //pixelLightCount
                pixelLightCountValue = new JSONStorableFloat("Pixel Light Count", UserPreferences.singleton.pixelLightCount, pixelLightCountCallback, 0f, 6, true);
                pixelLightCountValue_orginal = new JSONStorableFloat("Pixel Light Count", UserPreferences.singleton.pixelLightCount, pixelLightCountCallback, 0f, 6, true);
                RegisterFloat(pixelLightCountValue);
                pixelLightCountSlider = CreateSlider(pixelLightCountValue, false);
                pixelLightCountSlider.slider.wholeNumbers = true;
                pixelLightCountSlider.rangeAdjustEnabled = false;
                pixelLightCountSlider.quickButtonsEnabled  = false;

                //smooth passes
                smoothPassesValue = new JSONStorableFloat("Smooth Passes", UserPreferences.singleton.smoothPasses, smoothPassesCallback, 0, 4, true);
                smoothPassesValue_orginal = new JSONStorableFloat("Smooth Passes", UserPreferences.singleton.smoothPasses, smoothPassesCallback, 0, 4, true);
                RegisterFloat(smoothPassesValue);
                smoothPassesSlider = CreateSlider(smoothPassesValue, false);
                smoothPassesSlider.slider.wholeNumbers = true;
                smoothPassesSlider.rangeAdjustEnabled = false;
                smoothPassesSlider.quickButtonsEnabled  = false;

                //Glow
                GlowEffectsLevelChoices.Add("Off");
                GlowEffectsLevelChoices.Add("Low");
                GlowEffectsLevelChoices.Add("High");
                GlowEffectsLevelChooser = new JSONStorableStringChooser("Glow Effects Level", GlowEffectsLevelChoices, UserPreferences.singleton.glowEffects.ToString(), "Glow Effects Level", doGlowEffectsLevelChoice);
                RegisterStringChooser(GlowEffectsLevelChooser);
                CreatePopup(GlowEffectsLevelChooser, false);

                //PhysicsRat
                physicsRateChoices.Add("Auto");
                physicsRateChoices.Add("45");
                physicsRateChoices.Add("60");
                physicsRateChoices.Add("72");
                physicsRateChoices.Add("80");
                physicsRateChoices.Add("90");
                physicsRateChoices.Add("120");
                physicsRateChoices.Add("144");
                physicsRateChoices.Add("240");
                physicsRateChoices.Add("288");
                physicsRateChooser = new JSONStorableStringChooser("Physics Rate", physicsRateChoices, UserPreferences.singleton.physicsRate.ToString(), "Physics Rate", dophysicsRateChoice);
                RegisterStringChooser(physicsRateChooser);
                CreatePopup(physicsRateChooser, false);

                //physicsUpdateCap
                physicsUpdateCapValue = new JSONStorableFloat("Physics Update Cap", UserPreferences.singleton.physicsUpdateCap, physicsUpdateCapCallback, 1, 3, true);
                RegisterFloat(physicsUpdateCapValue);
                physicsUpdateCapSlider = CreateSlider(physicsUpdateCapValue, false);
                physicsUpdateCapSlider.slider.wholeNumbers = true;
                physicsUpdateCapSlider.rangeAdjustEnabled = false;
                physicsUpdateCapSlider.quickButtonsEnabled  = false;

                SetupInfoText(this, 
                    "<color=#606060><size=40><b>Give Me FPS v3.2.1</b></size>\nA Session Plugin.\n" +
                    //"These will set softbody physics for Tongue, breast & glute on/off to gain fps\n\n" +
                    "4 Quick buttons and cloth sim - with user fine tuning of the options the 4 buttons use + performance preferences for easy access</color>\n\n" +
                    "<b>Give me some FPS - Recommend:</b> Turns off Tongue & Glute softbody physics, breasts on, Hair Curve Density 16 - Multiplier 3 - strand width 0.00045 - iterations 1, Quality hair shader, disable pixel lights reflections and anti aliasing 1\n\n" +
                    "<b>Give me some FPS - Hair Only:</b> Hair same as recommend but all other options VAM default\n\n" +
                    "<b>Give me ALL the FPS!:</b> In addition to recommended - Turns off breast softbody physics, Hair sim off, Hair Curve Density 10 - Multiplier 2, reflections texture size 512.\n\n" +
                    "<b>Default:</b> Switch all settings back to VAM defaults (this isn't the same as the scene loaded with)\n\n" +
                    "<b>Toggle all clothes Sim On/Off:</b> Toggles all clothing simulation Off / On \n\n",
                    1100.0f, false
                );

                //start if user fine tuning
                SetupInfoText(this, "<color=#606060><size=40><b>Atom Settings (Applies to all persons)</b></size></color>", 20.0f, true);

                //Hair sim
                HairSimulationState = new JSONStorableBool("Disable all Hair Sim", false);
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
                BreastPhysicsState = new JSONStorableBool("Disable breast softbody Sim", false);
                RegisterBool(BreastPhysicsState);
                onToggleBreastPhysicsState = CreateToggle(BreastPhysicsState, true);
                onToggleBreastPhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleBreastPhysicsFPS(on);
                });

                LowerPhysicsState = new JSONStorableBool("Disable Glute softbody Sim", false);
                RegisterBool(LowerPhysicsState);
                onToggleLowerPhysicsState = CreateToggle(LowerPhysicsState, true);
                onToggleLowerPhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleLowerPhysicsFPS(on);
                });

                TonguePhysicsState = new JSONStorableBool("Disable Tongue softbody Sim", false);
                RegisterBool(TonguePhysicsState);
                onToggleTonguePhysicsState = CreateToggle(TonguePhysicsState, true);
                onToggleTonguePhysicsState.toggle.onValueChanged.AddListener((on) =>
                {
                    ToggleTonguePhysicsFPS(on);
                });

                //reflection controls
                disablePixelLightsState = new JSONStorableBool("Disable Reflection pixel shader", false);
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
                

                SetupInfoText(this,
                    "<color=#606060><size=40><b>CPU Bench FPS</b></size>\n" +
                    "WARNING - These buttons will adjust performance parameters to reduce as much load on the GPU as possible - this should show how many FPS a good GPU should be able to give - suggest use 2 or more persons in a scene to test\n\n" +
                    "The real killer of CPU you'll find is the softbody physics - this is why I recommend switching of tongue and glute softbody physics as the best compromise</color>\n\n",
                    530.0f, true
                );

                btn = CreateButton("CPU Bench - Soft Body physics On", true);
                btn.button.onClick.AddListener(() => { CPUbench(); });

                btn = CreateButton("CPU Bench - Soft Body physics Off", true);
                btn.button.onClick.AddListener(() => { CPUbenchNoSoftPhysics(); });

                btn = CreateButton("Restore Performance Parameters", true);
                btn.button.onClick.AddListener(() => { restore_vam_performance_parameters(); });

                SetupInfoText(this,
                    "<color=#606060><size=40><b>Adjust Person On Load Look</b></size>\n" +
                    "The plugin will attempt to adjust a person on loading a look\n\n" +
                    "However it's only possible to do this if the name of the look changes (OnAtomUIDsChanged)\n\n" +
                    "If the name of the loaded look is the same as the current person, it won't apply the settings</color>\n\n",
                    480.0f, true
                );
            }
            catch (Exception e)
            {
                SuperController.LogError("Failed to initialize plugin: " + e);
            }
        }

        
        private void OnSceneLoaded()
        {
            try {
                if (overrideScenePreferencesState.val) {
                    SuperController.LogMessage("GiveMeFPS - updated scene with user preferences test");
                    ToggleHairSimulationFPS(HairSimulationState.val);
                    HairMultiplierCallback(HairMultiplierValue);
                    CurveDensityCallback(CurveDensityValue);
                    HairWidthValueCallback(HairWidthValue);
                    iterationsCallback(iterationsValue);
                    doShaderChoice(shaderChooser.val);
                    ToggleBreastPhysicsFPS(BreastPhysicsState.val);
                    ToggleLowerPhysicsFPS(LowerPhysicsState.val);
                    ToggleTonguePhysicsFPS(TonguePhysicsState.val);
                    doReflectionTextureChoice(reflectionTextureSizeChooser.val);
                    onToggledisablePixelLightStateFPS(disablePixelLightsState.val);
                }
            }
            catch (Exception e) {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void OnAtomUIDsChanged(List<string> test)
        {
            count++;
            try {
                if (overrideScenePreferencesState.val) {
                     foreach(string str in test)
                     {
                       SuperController.LogMessage("GiveMeFPS - updated atom when appearance changes user preferences" + count + "str" + str);
                     }
                    SuperController.LogMessage("GiveMeFPS - updated atom when appearance changes user preferences" + count);
                    ToggleHairSimulationFPS(HairSimulationState.val);
                    HairMultiplierCallback(HairMultiplierValue);
                    CurveDensityCallback(CurveDensityValue);
                    HairWidthValueCallback(HairWidthValue);
                    iterationsCallback(iterationsValue);
                    doShaderChoice(shaderChooser.val);
                    ToggleBreastPhysicsFPS(BreastPhysicsState.val);
                    ToggleLowerPhysicsFPS(LowerPhysicsState.val);
                    ToggleTonguePhysicsFPS(TonguePhysicsState.val);
                    doReflectionTextureChoice(reflectionTextureSizeChooser.val);
                    onToggledisablePixelLightStateFPS(disablePixelLightsState.val);
                }
            }
            catch (Exception e) {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        //CPU Bench
        public void CPUbench()
        {
            if (first_time_bench){
                store_vam_performance_parameters();
                first_time_bench = false;
            }
            clothSimState.val = true;
            softPhysicsState.val = true;
            msaaLevelchooser.val = "Off";
            pixelLightCountValue.val = 0;
            mirrorReflectionsState.val = false;
            smoothPassesValue.val = 0 ;
            ShaderLODChooser.val ="Low";
            ExecuteGiveMeFPS(false, 10, 2, 0.00065000f, 1, true, true, true, "Fast", true, "1", "512", false);
        }

        //CPU Bench
        public void CPUbenchNoSoftPhysics()
        {
            if (first_time_bench){
                store_vam_performance_parameters();
                first_time_bench = false;
            }
            clothSimState.val = true;
            softPhysicsState.val = false;
            msaaLevelchooser.val = "Off";
            pixelLightCountValue.val = 0;
            mirrorReflectionsState.val = false;
            smoothPassesValue.val = 0 ;
            ShaderLODChooser.val ="Low";
            ExecuteGiveMeFPS(false, 10, 2, 0.00065000f, 1, false, false, false, "Fast", true, "1", "512", false);
        }

        //CPU Bench
        public void store_vam_performance_parameters()
        {
            clothSimState_orginal.val = clothSimState.val;
            softPhysicsState_orginal.val = softPhysicsState.val;
            msaaLevelchooser_orginal.val = msaaLevelchooser.val;
            pixelLightCountValue_orginal.val = pixelLightCountValue.val;
            mirrorReflectionsState_orginal.val = mirrorReflectionsState.val;
            smoothPassesValue_orginal.val = smoothPassesValue.val;
            ShaderLODChooser_orginal.val = ShaderLODChooser.val;
        }

        //CPU Bench
        public void restore_vam_performance_parameters()
        {
            clothSimState.val = clothSimState_orginal.val;
            softPhysicsState.val = softPhysicsState_orginal.val;
            msaaLevelchooser.val = msaaLevelchooser_orginal.val;
            pixelLightCountValue.val = pixelLightCountValue_orginal.val;
            mirrorReflectionsState.val = mirrorReflectionsState_orginal.val;
            smoothPassesValue.val = smoothPassesValue_orginal.val;
            ShaderLODChooser.val = ShaderLODChooser_orginal.val;
            ExecuteGiveMeFPS(true, 16, 16, 0.00010000f, 2, true, true, true, "Quality", false , "8", "1024", false);
        }

        //Preferences
        public void TogglemirrorReflectionsFPS(bool mirrorReflectionsState)
        {
            UserPreferences.singleton.mirrorReflections = mirrorReflectionsState;
        }

        public void TogglesoftPhysicsFPS(bool softPhysicsState)
        {
            UserPreferences.singleton.softPhysics = softPhysicsState;
        }

        public void pixelLightCountCallback(JSONStorableFloat pixelLightCountValue)
        {
            if ( pixelLightCountValue.val >= 0f && pixelLightCountValue.val <= 6f)
            {
            UserPreferences.singleton.pixelLightCount = (int)pixelLightCountValue.val;
            }
        }

        public void physicsUpdateCapCallback(JSONStorableFloat physicsUpdateCapValue)
        {
            if ( physicsUpdateCapValue.val >= 1f && physicsUpdateCapValue.val <= 3f)
            {
            UserPreferences.singleton.physicsUpdateCap = (int)physicsUpdateCapValue.val;
            }
        }

        public void renderScaleCallback(JSONStorableFloat renderScaleValue)
        {
            if ( renderScaleValue.val >= 0.5f && renderScaleValue.val <= 2.0f)
            {
            UserPreferences.singleton.renderScale = renderScaleValue.val;
            }
        }

        public void smoothPassesCallback(JSONStorableFloat smoothPassesValue)
        {
            if ( smoothPassesValue.val >= 0f && smoothPassesValue.val <= 4f)
            {
            UserPreferences.singleton.smoothPasses = (int)smoothPassesValue.val;
            }
        }

        public void doShaderLODChoice(string ShaderChoice)
        {
            switch(ShaderChoice)
            {
              case "Low":
                UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.Low;
                break;
              case "Medium":
                UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.Medium;
                break;
              case "High":
                UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.High;
                break;
              default:
                UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.Medium;
                break;
            }
        }

        public void doGlowEffectsLevelChoice(string GlowEffectsLevel)
        {
            switch(GlowEffectsLevel)
            {
              case "Off":
                UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.Off;
                break;
              case "Low":
                UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.Low;
                break;
              case "High":
                UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.High;
                break;
              default:
                UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.High;
                break;
            }
        }

        public void dophysicsRateChoice(string physicsRate)
        {
            switch(physicsRate)
            {
              case "Auto":
                UserPreferences.singleton.SetPhysicsRateFromString("Auto");
                break;
              case "45":
                UserPreferences.singleton.SetPhysicsRateFromString("_45");
                break;
              case "60":
                UserPreferences.singleton.SetPhysicsRateFromString("_60");
                break;
              case "72":
                UserPreferences.singleton.SetPhysicsRateFromString("_72");
                break;
              case "80":
                UserPreferences.singleton.SetPhysicsRateFromString("_80");
                break;
              case "90":
                UserPreferences.singleton.SetPhysicsRateFromString("_90");
                break;
              case "120":
                UserPreferences.singleton.SetPhysicsRateFromString("_120");
                break;
              case "144":
                UserPreferences.singleton.SetPhysicsRateFromString("_144");
                break;
              case "240":
                UserPreferences.singleton.SetPhysicsRateFromString("_240");
                break;
              case "288":
                UserPreferences.singleton.SetPhysicsRateFromString("_288");
                break;
              default:
                UserPreferences.singleton.SetPhysicsRateFromString("Auto");
                break;
            }
        }

        public void doMSAAlevelChoice(string msaaChoice)
        {
            switch(msaaChoice)
            {
              case "Off":
                UserPreferences.singleton.msaaLevel = 0;
                break;
              case "2x":
                UserPreferences.singleton.msaaLevel = 2;
                break;
              case "4x":
                UserPreferences.singleton.msaaLevel = 4;
                break;
              case "8x":
                UserPreferences.singleton.msaaLevel = 8;
                break;
              default:
                UserPreferences.singleton.msaaLevel = 8;
                break;
            }
        }

        //Quick buttons
        public void ExecuteGiveMeFPS(bool HairSimulationState, float CurveDensityValue, float HairMultiplierValue, float HairWidthValue, float iterationsValue, bool BreastPhysicsState, bool LowerPhysicsState, bool TonguePhysicsState, string shaderTypeValue, bool disablePixelLightsState, string antiAliasingValue, string textureSizeValue, bool hairOnly )
        {
            onToggleHairSimulationState.toggle.isOn = !HairSimulationState;
            iterationsSlider.slider.value = iterationsValue;
            HairMultiplierSlider.slider.value = HairMultiplierValue;
            CurveDensitySlider.slider.value = CurveDensityValue;
            HairWidthValueSlider.slider.value = HairWidthValue;
            shaderChooser.val = shaderTypeValue;

            if (!hairOnly) {
                onToggleBreastPhysicsState.toggle.isOn = !BreastPhysicsState;
                onToggleLowerPhysicsState.toggle.isOn = !LowerPhysicsState;
                onToggleTonguePhysicsState.toggle.isOn = !TonguePhysicsState;
                onToggledisablePixelLightState.toggle.isOn = disablePixelLightsState;
                reflectionTextureSizeChooser.val = textureSizeValue;
            }

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
                    if (!hairOnly) {
                        atom.GetStorableByID("BreastPhysicsMesh").GetBoolJSONParam("on").val=BreastPhysicsState;
                        atom.GetStorableByID("LowerPhysicsMesh").GetBoolJSONParam("on").val=LowerPhysicsState;
                        atom.GetStorableByID("TongueControl").GetBoolJSONParam("tongueCollision").val=TonguePhysicsState;
                    }
                }

                if (!hairOnly) {
                    if (atom.GetStorableByID("MirrorRender"))
                    {
                        atom.GetStorableByID("MirrorRender").GetBoolJSONParam("disablePixelLights").val=disablePixelLightsState;
                        atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("antiAliasing").val = antiAliasingValue;
                        atom.GetStorableByID("MirrorRender").GetStringChooserJSONParam("textureSize").val = textureSizeValue;
                    }
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
                    atom.GetStorableByID("BreastPhysicsMesh").GetBoolJSONParam("on").val=!BreastPhysicsState;

                }
            }
        }
        public void ToggleLowerPhysicsFPS(bool LowerPhysicsState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    atom.GetStorableByID("LowerPhysicsMesh").GetBoolJSONParam("on").val=!LowerPhysicsState;

                }
            }
        }
        public void ToggleTonguePhysicsFPS(bool TonguePhysicsState)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    atom.GetStorableByID("TongueControl").GetBoolJSONParam("tongueCollision").val=!TonguePhysicsState;

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
                        clothControl.SetBoolParamValue("simEnabled", !ClothSimState);
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
                            hairControl.SetBoolParamValue("simulationEnabled", !HairSimulationState);
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

        protected void iterationsCallback(JSONStorableFloat iterationsValue)
        {
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom.type == "Person")
                {
                    foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
                    {
                        HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();
                        if (hairControl!=null) {
                            hairControl.SetFloatParamValue("iterations", iterationsValue.val);
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
