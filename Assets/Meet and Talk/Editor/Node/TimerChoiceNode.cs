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
    public class TimerChoiceNode : DialogueChoiceNode
    {
        private float time = 10;
        public float ChoiceTime { get => time; set => time = value; }
        protected FloatField _choiceTimerField;

        public TimerChoiceNode() : base()
        {

        }

        public TimerChoiceNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView) :
            base(_position, _editorWindow, _graphView)
        {
        }

        protected override void PopulateContainer()
        {
            base.PopulateContainer();
            mainContainer.Add(_choiceTimerField);
        }
        protected override void CreateFields()
        {
            base.CreateFields();

            _choiceTimerField = new FloatField("Duration To Show");
            _choiceTimerField.RegisterValueChangedCallback(value =>
            {
                time = value.newValue;
            });
            _choiceTimerField.SetValueWithoutNotify(time);
            _choiceTimerField.AddToClassList("TextTime");
        }

        public override void LoadValueInToField()
        {
            base.LoadValueInToField();
            _choiceTimerField.SetValueWithoutNotify(time);
        }

        public override void SetValidation()
        {
            base.SetValidation();

            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            if (ChoiceTime < 3)
                warning.Add("Short time for Make Decision");

            ErrorList.AddRange(error);
            WarningList.AddRange(warning);
        }
    }
}
