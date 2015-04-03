using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Goon.AI
{
    #if UNITY_EDITOR
    public class StateMachineEditor : EditorWindow
    {
        public static StateMachineEditor curWindow;
        public AIStateMachine stateMachine;
        public GameObject gameObject;

        private List<AIState> states;
        private List<AITransition> transitions;

        private AIState selectedState = null;
        private AITransition selectedTransition = null;

        private AIState stateWantsConnection = null;
        private AITransition transitionWantsConnection = null;

        private static readonly Rect stateRect = new Rect(0f, 0f, 300f, 115f);
        private static readonly Rect transRect = new Rect(0f, 0f, 300f, 95f);

        private Vector2 mousePos = Vector2.zero;
        private Vector2 gridOffset = Vector2.zero;

        private GUISkin skin;

        public static void LaunchEditor(AIStateMachine machine)
        {
            curWindow = EditorWindow.GetWindow<StateMachineEditor>();
            curWindow.stateMachine = machine;
            curWindow.gameObject = machine.gameObject;
            curWindow.title = "AI Editor";
            curWindow.skin = Resources.Load<GUISkin>("AINodeSkin");
        }

        private void OnGUI()
        {
            states = new List<AIState>(gameObject.GetComponentsInChildren<AIState>());
            transitions = new List<AITransition>(gameObject.GetComponentsInChildren<AITransition>());

            //Draw grid
            AINodeUtils.DrawGrid(position, 60f, gridOffset, 0.1f, Color.white);
            AINodeUtils.DrawGrid(position, 300f, gridOffset, 0.15f, Color.white);

            //Draw transitions from states
            foreach (AIState state in states)
            {
                if (state.transitions != null)
                {
                    foreach (AITransition transition in state.transitions)
                    {
                        if (transition != null)
                        {
                            AINodeUtils.DrawBezier(state.nodePos + stateRect.center, transition.nodePos + transRect.center, Color.blue);
                        }
                    }
                }
            }

            //Draw transitions to states
            foreach (AITransition transition in transitions)
            {
                if (transition.targetState != null)
                {
                    AINodeUtils.DrawBezier(transition.nodePos + transRect.center, transition.targetState.nodePos + stateRect.center, Color.yellow);
                }
            }

            //Draw transitions
            foreach (AITransition transition in transitions)
            {
                Rect transBox = transRect.Translated(transition.nodePos);
                if (selectedTransition == transition)
                {
                    GUI.Box(transBox, transition.transitionName, skin.customStyles[0]);
                }
                else
                {
                    GUI.Box(transBox, transition.transitionName, skin.box);
                }

                GUILayout.BeginArea(transBox);
                GUILayout.Space(25f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(10f);
                GUILayout.BeginVertical();

                transition.transitionName = EditorGUILayout.TextField("Name", transition.transitionName);
                if (transition.transitionName != "Transition" && transition.gameObject != gameObject)
                {
                    transition.gameObject.name = "Transition: " + transition.transitionName;
                }

                GUILayout.Space(10f);

                transition.condition = (AICondition)EditorGUILayout.ObjectField("Condition", transition.condition, typeof(AICondition), true);
                transition.action = (AIAction)EditorGUILayout.ObjectField("Action", transition.action, typeof(AIAction), true);

                GUILayout.EndVertical();
                GUILayout.Space(10f);
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }

            //Draw states
            foreach (AIState state in states)
            {
                Rect stateBox = stateRect.Translated(state.nodePos);
                if (selectedState == state)
                {
                    GUI.Box(stateBox, state.stateName, skin.customStyles[0]);
                }
                else
                {
                    GUI.Box(stateBox, state.stateName, skin.box);
                }

                GUILayout.BeginArea(stateBox);
                GUILayout.Space(25f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(10f);
                GUILayout.BeginVertical();

                state.stateName = EditorGUILayout.TextField("Name", state.stateName);

                if (state.stateName != "State" && state.gameObject != gameObject)
                {
                    state.gameObject.name = "State: " + state.stateName;
                }

                GUILayout.Space(10f);

                state.action = (AIAction)EditorGUILayout.ObjectField("Update Action", state.action, typeof(AIAction), true);
                state.entryAction = (AIAction)EditorGUILayout.ObjectField("Entry Action", state.entryAction, typeof(AIAction), true);
                state.exitAction = (AIAction)EditorGUILayout.ObjectField("Exit Action", state.exitAction, typeof(AIAction), true);

                GUILayout.EndVertical();
                GUILayout.Space(10f);
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }

            if (stateWantsConnection != null)
            {
                AINodeUtils.DrawBezier(stateWantsConnection.nodePos + stateRect.center, mousePos, Color.white);
            }
            else if (transitionWantsConnection != null)
            {
                AINodeUtils.DrawBezier(transitionWantsConnection.nodePos + transRect.center, mousePos, Color.white);
            }

            Repaint();

            Event e = Event.current;
            mousePos = e.mousePosition;

            if (e.isMouse)
            {
                AIState overState = GetMouseOverState(states, e.mousePosition);
                AITransition overTrans = null;
                if (overState == null)
                {
                    overTrans = GetMouseOverTransition(transitions, e.mousePosition);
                }

                if (stateWantsConnection != null || transitionWantsConnection != null)
                {
                    if (e.button == 0 && e.type == EventType.MouseDown)
                    {
                        HandleWantsConnection(overState, overTrans, e);
                    }
                }
                else if (e.button == 0)
                {
                    switch (e.type)
                    {
                        case EventType.MouseDown:
                            HandleLeftClick(overState, overTrans, e);
                            break;
                        case EventType.MouseDrag:
                            HandleLeftDrag(e);
                            break;
                    }
                }
                else if (e.button == 1 && e.type == EventType.MouseDown)
                {
                    HandleRightClick(overState, overTrans, e);
                }
                else if (e.button == 2 && e.type == EventType.MouseDrag)
                {
                    HandleMiddleDrag(e);
                }
            }

        }

        #region handler functions
        private void HandleWantsConnection(AIState state, AITransition transition, Event e)
        {
            if (transition != null && stateWantsConnection != null)
            {
                if (stateWantsConnection.transitions == null)
                {
                    stateWantsConnection.transitions = new List<AITransition>();
                }

                stateWantsConnection.transitions.Add(transition);
            }
            else if (state != null && transitionWantsConnection != null)
            {
                transitionWantsConnection.targetState = state;
            }

            stateWantsConnection = null;
            transitionWantsConnection = null;
        }

        private void HandleLeftClick(AIState state, AITransition transition, Event e)
        {
            if (state != null)
            {
                selectedState = state;
                selectedTransition = null;
                Selection.activeGameObject = selectedState.gameObject;
            }
            else if (transition != null)
            {
                selectedState = null;
                selectedTransition = transition;
                Selection.activeGameObject = selectedTransition.gameObject;
            }
            else
            {
                selectedState = null;
                selectedTransition = null;
            }
        }

        private void HandleLeftDrag(Event e)
        {
            if (selectedState != null)
            {
                selectedState.nodePos += e.delta;
            }
            else if (selectedTransition != null)
            {
                selectedTransition.nodePos += e.delta;
            }
        }

        private void HandleRightClick(AIState state, AITransition transition, Event e)
        {
            HandleLeftClick(state, transition, e);

            GenericMenu dropDown = new GenericMenu();
            if (state != null)
            {
                dropDown.AddItem(new GUIContent("Connect to Transition"), false, ConnectToTransition);
                dropDown.AddItem(new GUIContent("Set as Initial State"), false, SetAsInitState);
                dropDown.AddSeparator("");
                dropDown.AddItem(new GUIContent("Delete State"), false, RemoveState);
            }
            else if (transition != null)
            {
                dropDown.AddItem(new GUIContent("Connect to State"), false, ConnectToState);
                dropDown.AddItem(new GUIContent("Remove Connections"), false, RemoveTransitionConnections);
                dropDown.AddSeparator("");
                dropDown.AddItem(new GUIContent("Delete Transition"), false, RemoveTransition);
            }
            else
            {
                dropDown.AddItem(new GUIContent("Create State"), false, CreateState);
                dropDown.AddItem(new GUIContent("Create Transition"), false, CreateTransition);
            }
            dropDown.ShowAsContext();

            e.Use();
        }

        private void HandleMiddleDrag(Event e)
        {
            foreach (AIState state in states)
            {
                state.nodePos += e.delta;
            }

            foreach (AITransition transition in transitions)
            {
                transition.nodePos += e.delta;
            }

            gridOffset += e.delta;
        }
        #endregion

        #region context menu functions
        private void SetAsInitState()
        {
            if (selectedState != null)
            {
                stateMachine.initialState = selectedState;
            }
        }

        private void ConnectToTransition()
        {
            stateWantsConnection = selectedState;
        }

        private void ConnectToState()
        {
            transitionWantsConnection = selectedTransition;
        }

        private void RemoveState()
        {
            if (selectedState != null)
            {
                foreach (AITransition transition in transitions)
                {
                    if (transition.targetState == selectedState)
                    {
                        transition.targetState = null;
                    }
                }

                stateMachine.states.Remove(selectedState);
                if (stateMachine.initialState == selectedState)
                {
                    if (stateMachine.states.Count > 0)
                    {
                        stateMachine.initialState = stateMachine.states[0];
                    }
                    else
                    {
                        stateMachine.initialState = null;
                    }
                }

                DestroyImmediate(selectedState);
                selectedState = null;
            }
        }

        private void RemoveTransition()
        {
            if (selectedTransition != null)
            {
                RemoveTransitionConnections();
                DestroyImmediate(selectedTransition);
                selectedTransition = null;
            }
        }

        private void RemoveTransitionConnections()
        {
            if (selectedTransition != null)
            {
                foreach (AIState state in states)
                {
                    if (state.transitions != null)
                    {
                        for (int i = 0; i < state.transitions.Count; ++i)
                        {
                            if (state.transitions[i] == selectedTransition)
                            {
                                state.transitions.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                selectedTransition.targetState = null;
            }
        }

        private void CreateState()
        {
            GameObject go = new GameObject("State");
            go.transform.parent = gameObject.transform;
            go.transform.localPosition = Vector3.zero;

            AIState state = go.AddComponent<AIState>();
            state.nodePos = mousePos;
            stateMachine.states.Add(state);

            if (stateMachine.states.Count == 1)
            {
                stateMachine.initialState = state;
            }

            Selection.activeGameObject = go;
        }

        private void CreateTransition()
        {
            GameObject go = new GameObject("Transition");
            go.transform.parent = gameObject.transform;
            go.transform.localPosition = Vector3.zero;

            AITransition transition = go.AddComponent<AITransition>();
            transition.nodePos = mousePos;

            Selection.activeGameObject = go;
        }
        #endregion

        private AIState GetMouseOverState(List<AIState> states, Vector2 mousePos)
        {
            AIState found = null;

            foreach (AIState state in states)
            {
                if (stateRect.Translated(state.nodePos).Contains(mousePos))
                {
                    found = state;
                    break;
                }
            }

            return found;
        }

        private AITransition GetMouseOverTransition(List<AITransition> transitions, Vector2 mousePos)
        {
            AITransition found = null;

            foreach (AITransition transition in transitions)
            {
                if (transRect.Translated(transition.nodePos).Contains(mousePos))
                {
                    found = transition;
                    break;
                }
            }

            return found;
        }
    }

    public static class AINodeUtils
    {
        public static Rect Translated(this Rect rect, Vector2 translation)
        {
            Rect result = new Rect(rect);
            result.x += translation.x;
            result.y += translation.y;
            return result;
        }

        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.x, rect.y);
        }

        public static void DrawBezier(Vector2 start, Vector2 end, Color color)
        {
            Handles.BeginGUI();

            Vector2 startTan = start;
            Vector2 endTan = end;
            Vector2 difference = end - start;

            float tanScalar = .9f;
            if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
            {
                startTan.x += difference.x * tanScalar;
                endTan.x -= difference.x * tanScalar;
            }
            else
            {
                startTan.y += difference.y * tanScalar;
                endTan.y -= difference.y * tanScalar;
            }
            
            Handles.DrawBezier(start, end,
                               startTan, endTan,
                               color, null, 2f);

            Handles.EndGUI();
        }

        public static void DrawGrid(Rect viewRect, float gridSpacing, Vector2 offset, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(viewRect.width / gridSpacing) + 1;
            int heightDivs = Mathf.CeilToInt(viewRect.height / gridSpacing) + 1;
            offset.x = offset.x % gridSpacing;
            offset.y = offset.y % gridSpacing;

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int x = 0; x < widthDivs; ++x)
            {
                Handles.DrawLine(new Vector3(gridSpacing * x + offset.x, 0f, 0f), new Vector3(gridSpacing * x + offset.x, viewRect.height, 0f));
            }

            for (int y = 0; y < heightDivs; ++y)
            {
                Handles.DrawLine(new Vector3(0f, gridSpacing * y + offset.y, 0f), new Vector3(viewRect.width, gridSpacing * y + offset.y, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }

    #endif
}

