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

    enum AnimationState {
        PLAY = 0,
        PAUSE = 1,
        STOP = 2
    }


    AnimatorMode _mode = AnimatorMode.ANIMATORS;
    Animator[] _animators;
    AnimationClip[][] _animations;
    bool _loop = true;
    int _animatorSelected = -1;
    int _animationSelected = -1;
    float _timer = 0;
    float _ownDeltaTime = 0;
    float _LastUpdateTime = 0;
    float _animationsSpeed = 1;
    float _animationsOffset = 0;
    AnimationState _animationState = AnimationState.STOP;
    private GUIContent _content;
    private GUIContent _contentd;
#if UNITY_EDITOR
    void OnGUI() {
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
        if (GUILayout.Button(_animators[_animatorSelected].gameObject.name)) {
            _mode = (AnimatorMode)Mathf.Clamp((int)_mode - 1, 0, 2);
            StopAnimation();
            return;
        }
        GUILayout.EndHorizontal();
        if (_animators[_animatorSelected].runtimeAnimatorController == null || _animations[_animatorSelected].Length == 0) {
            GUILayout.Label("No animations", EditorStyles.boldLabel);
        } else {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            for (int j = 0; j < _animations[_animatorSelected].Length; j++) {
                if (GUILayout.Button(_animations[_animatorSelected][j].name)) {
                    _animationSelected = j;
                    _mode = (AnimatorMode)Mathf.Clamp((int)_mode + 1, 0, 2);
                    PauseAnimation();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            for (int j = 0; j < _animations[_animatorSelected].Length; j++) {
                if (_animationSelected == j) {
                    if (_animationState == AnimationState.PLAY) {
                        if (GUILayout.Button("Pause", GUILayout.Width(70))) {
                            PauseAnimation();
                        }
                    } else {
                        if (GUILayout.Button("Continue", GUILayout.Width(70))) {
                            ContinueAnimation();
                        }
                    }
                } else {
                    if (GUILayout.Button("Play", GUILayout.Width(70))) {
                        StartAnimation(j);
                    }
                }
            }
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Stop", GUILayout.Height(2 * (_animations[_animatorSelected].Length - 1) + 19 * _animations[_animatorSelected].Length))) {
                StopAnimation();
            }
            EditorGUILayout.EndHorizontal();
        }

    }

    void FocusAnimation() {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(_animations[_animatorSelected][_animationSelected].name)) {
            _mode = (AnimatorMode)Mathf.Clamp((int)_mode - 1, 0, 2);
            StopAnimation();
            return;
        }
        if (_animationState == AnimationState.PLAY) {
            if (GUILayout.Button("Pause", GUILayout.Width(50))) {
                PauseAnimation();
            }
        } else {
            if (GUILayout.Button("Play", GUILayout.Width(50))) {
                StartAnimation(_animationSelected);
            }
        }
        if (GUILayout.Button("Reset", GUILayout.Width(50))) {
            ResetAnimation();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Loop", EditorStyles.boldLabel);
        _loop = EditorGUILayout.Toggle(_loop);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sample", EditorStyles.boldLabel);
        _timer = EditorGUILayout.Slider(_timer, 0, _animations[_animatorSelected][_animationSelected].length);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
        _animationsSpeed = EditorGUILayout.Slider(_animationsSpeed, 0, 5);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Offset", EditorStyles.boldLabel);
        _animationsOffset = EditorGUILayout.Slider(_animationsOffset, 0, 10);
        EditorGUILayout.EndHorizontal();

    }

    private void Update() {
        OwnDeltaTime();
        if (_animationState == AnimationState.PLAY && _animatorSelected != -1 && null != _animations[_animatorSelected] && _animationSelected < _animations[_animatorSelected].Length) {
            _timer += _ownDeltaTime * _animationsSpeed;
            if (_timer > _animations[_animatorSelected][_animationSelected].length + _animationsOffset * _animationsSpeed) {
                _timer = 0;
                if (!_loop) {
                    PauseAnimation();
                }
            }
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode()) {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_animators[_animatorSelected].gameObject, _animations[_animatorSelected][_animationSelected], _timer);
                AnimationMode.EndSampling();
                SceneView.RepaintAll();
            }
        }
        if(_animationState == AnimationState.PAUSE) {
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode()) {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_animators[_animatorSelected].gameObject, _animations[_animatorSelected][_animationSelected], _timer);
                AnimationMode.EndSampling();
                SceneView.RepaintAll();
            }
        }
    }
    void StartAnimation(int animationSelected) {
        _animationSelected = animationSelected;
        AnimationMode.StartAnimationMode();
        _animationState = AnimationState.PLAY;
    }

    void PauseAnimation() {
        _animationState = AnimationState.PAUSE;
    }

    void ContinueAnimation() {
        _animationState = AnimationState.PLAY;
    }

    void ResetAnimation() {
        _animationState = AnimationState.PAUSE;
        _animationsSpeed = 1;
        _animationsOffset = 0;
        _timer = 0;
        _loop = true;
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(_animators[_animatorSelected].gameObject, _animations[_animatorSelected][_animationSelected], _timer);
        AnimationMode.EndSampling();
        AnimationMode.StopAnimationMode();
    }
    void StopAnimation() {
        _timer = 0;
        if (_animationSelected != -1) {
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(_animators[_animatorSelected].gameObject, _animations[_animatorSelected][_animationSelected], _timer);
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();
            _animationSelected = -1;
        }
        _animationState = AnimationState.STOP;
    }

    void OwnDeltaTime() {
        _ownDeltaTime = Time.realtimeSinceStartup - _LastUpdateTime;
        _LastUpdateTime = Time.realtimeSinceStartup;
    }

    void OnEnable() {
        // Debug.Log("OnEnable");
        _mode = AnimatorMode.ANIMATORS;
        _animators = FindObjectsOfType<Animator>();
        _animations = new AnimationClip[_animators.Length][];
        _animatorSelected = -1;
        _animationSelected = -1;
        StopAnimation();
    }
#endif
}
