using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using MeetAndTalk.Editor;
using MeetAndTalk.Localization;
using MeetAndTalk.Event;

namespace MeetAndTalk.Nodes
{

    public class RandomNote : BaseNode
    {
        public List<DialogueNodePort> dialogueNodePorts = new List<DialogueNodePort>();

        public RandomNote()
        {

        }

        public RandomNote(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Random";
            SetPosition(new Rect(_position, defualtNodeSize));
            nodeGuid = Guid.NewGuid().ToString();

            AddInputPort("Input ", Port.Capacity.Multi);
            AddValidationContainer();

            Button button = new Button()
            {
                text = "+ Add Option"
            };
            button.clicked += () =>
            {
                AddChoicePort(this);
            };

            titleButtonContainer.Add(button);
        }

        public void ReloadLanguage()
        {
            foreach (DialogueNodePort nodePort in dialogueNodePorts)
            {
                TextFieldRegisterLangCallback(nodePort.TextField, nodePort.TextLanguage);
                TextFieldRegisterLangCallback(nodePort.HintField, nodePort.HintLanguage);
            }
        }

        public override void LoadValueInToField()
        {

        }

        public Port AddChoicePort(BaseNode basenote, DialogueNodePort inDialogueNodePort = null)
        {
            Port port = this.GetPort();
            DialogueNodePort dialogueNodePort = this.GeneratePort(basenote, inDialogueNodePort);

            var mainContainer = this.GeneratePortVisualElement(() => DeleteButton(basenote, port), out dialogueNodePort.TextField, out dialogueNodePort.HintField);
            port.contentContainer.Add(mainContainer);

            TextFieldRegisterLangCallback(dialogueNodePort.TextField, dialogueNodePort.TextLanguage);
            TextFieldRegisterLangCallback(dialogueNodePort.HintField, dialogueNodePort.HintLanguage);

#if UNITY_EDITOR
            dialogueNodePort.MyPort = port;
#endif
            port.portName = "";

            dialogueNodePorts.Add(dialogueNodePort);

            basenote.outputContainer.Add(port);

            basenote.RefreshPorts();
            basenote.RefreshExpandedState();

            return port;
        }

        private void DeleteButton(BaseNode _node, Port _port)
        {
#if UNITY_EDITOR
            DialogueNodePort tmp = dialogueNodePorts.Find(port => port.MyPort == _port);
            dialogueNodePorts.Remove(tmp);
#endif

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == _port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            _node.outputContainer.Remove(_port);

            _node.RefreshPorts();
            _node.RefreshExpandedState();
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");

            Port output = outputContainer.Query<Port>().First();
            if (!output.connected) error.Add("Output does not lead to any node");

            if (dialogueNodePorts.Count < 1) error.Add("You need to add more Output");
            else
            {
                for (int i = 0; i < dialogueNodePorts.Count; i++)
                {
                    if (!dialogueNodePorts[i].MyPort.connected) error.Add($"Output ID:{i} does not lead to any node");
                }
            }

            ErrorList = error;
            WarningList = warning;
        }
    }
}
