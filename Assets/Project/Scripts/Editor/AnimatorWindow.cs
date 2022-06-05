using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimatorWindow : EditorWindow {
    [MenuItem("Sloot/My Window")]
    public static void ShowWindow() {
        GetWindow(typeof(AnimatorWindow));
    }

    enum AnimatorMode {
        ANIMATORS = 0,
        ANIMATIONS = 1,
        ANIMATIONFOCUS = 2
    }

    AnimatorMode _mode = AnimatorMode.ANIMATORS;
    Animator[] _animators;
    AnimationClip[][] _animations;
    int _animatorSelected = -1;
    int _animationSelected = -1;
    float timeHeure = 0;
    private GUIContent _content;
    private GUIContent _contentd;
    void OnGUI() {
        //EditorGUILayout.BeginHorizontal();
        //GUILayout.Label(_content);
        //if (GUILayout.Button("<")) {
        //    _mode = (AnimatorMode)Mathf.Clamp((int)_mode - 1, 0, 2);
        //}
        //GUILayout.FlexibleSpace();
        //if (GUILayout.Button(">")) {
        //    _mode = (AnimatorMode)Mathf.Clamp((int)_mode + 1, 0, 2);
        //}
        //EditorGUILayout.EndHorizontal();
        //if (null == _content)
        //    _content = new GUIContent("", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Project/Sprites/leftA.png", typeof(Sprite)));
        //if (null == _contentd)
        //    _contentd = new GUIContent("", (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Project/Sprites/rightA.png", typeof(Sprite)));

        switch (_mode) {
            default:
            case AnimatorMode.ANIMATORS:
                ShowAnimators();
                break;
            case AnimatorMode.ANIMATIONS:
                ShowAnimations();
                break;
            case AnimatorMode.ANIMATIONFOCUS:
                FocusAnimation();
                break;
        }
    }

    void ShowAnimators() {
        if (_animators != null) {
            GUILayout.Label("Animators :", EditorStyles.boldLabel);
            for (int i = 0; i < _animators.Length; i++) {
                if (GUILayout.Button(_animators[i].name)) {
                    Selection.activeGameObject = _animators[i].transform.parent.gameObject;
                    SceneView.FrameLastActiveSceneView();
                    if (_animators[i].runtimeAnimatorController) {
                        _animations[i] = _animators[i].runtimeAnimatorController.animationClips;
                    }
                    _animatorSelected = i;
                    _mode = AnimatorMode.ANIMATIONS;
                }
            }
        }
    }

    void ShowAnimations() {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<")) {
            _mode = (AnimatorMode)Mathf.Clamp((int)_mode - 1, 0, 2);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label("Animations :", EditorStyles.boldLabel);
        if (_animators[_animatorSelected].runtimeAnimatorController == null || _animations[_animatorSelected].Length == 0) {
            GUILayout.Label("No animations", EditorStyles.boldLabel);
        } else {
            EditorGUILayout.BeginVertical();
            for (int j = 0; j < _animations[_animatorSelected].Length; j++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(_animations[_animatorSelected][j].name, EditorStyles.boldLabel);
                if (GUILayout.Button("Play", GUILayout.Width(50))) {
                    AnimationMode.StartAnimationMode();
                    _animationSelected = j;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        if (GUILayout.Button("Stop", GUILayout.Width(50))) {
            AnimationMode.StopAnimationMode();
            _animationSelected = -1;
        }

    }

    void FocusAnimation() {

    }

    private void Update() {
        timeHeure += Time.deltaTime;
        if (_animationSelected != -1 && _animatorSelected != -1 && null != _animations[_animatorSelected] && _animationSelected < _animations[_animatorSelected].Length) {
            if (timeHeure > _animations[_animatorSelected][_animationSelected].length) {
                timeHeure = 0;
            }
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode() ) {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_animators[_animatorSelected].gameObject, _animations[_animatorSelected][_animationSelected], timeHeure);
                AnimationMode.EndSampling();

                SceneView.RepaintAll();
            }
        }
        Debug.Log(_animatorSelected);
    }

    void OnEnable() {
        Debug.Log("OnEnable");
        _mode = AnimatorMode.ANIMATORS;
        _animators = FindObjectsOfType<Animator>();
        _animations = new AnimationClip[_animators.Length][];
        _animatorSelected = -1;
    }
}
