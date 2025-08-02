using System;
using System.Linq;
using MeetAndTalk.Localization;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace MeetAndTalk.Nodes
{
    public static class DialogueNodeHelper
    {
        public static Port GetPort(this BaseNode node)
        {
            Port port = node.GetPortInstance(Direction.Output);
            port.contentContainer.style.flexGrow = 1;
            port.contentContainer.style.height = 50;

            return port;
        }
        public static DialogueNodePort GeneratePort(this BaseNode node, BaseNode baseNode, DialogueNodePort inDialogueNodePort = null)
        {
            string outputPortName = "";
            int outputPortCount = baseNode.outputContainer.Query("connector").ToList().Count();
            if (outputPortCount < 9) { outputPortName = $"Choice 0{outputPortCount + 1}"; }
            else { outputPortName = $"Choice {outputPortCount + 1}"; }

            DialogueNodePort dialogueNodePort = new();
            dialogueNodePort.PortGuid = Guid.NewGuid().ToString(); //NOWE

            foreach (LocalizationEnum language in (LocalizationEnum[])Enum.GetValues(typeof(LocalizationEnum)))
            {
                dialogueNodePort.TextLanguage.Add(new LanguageGeneric<string>()
                {
                    languageEnum = language,
                    LanguageGenericType = outputPortName
                });

                dialogueNodePort.HintLanguage.Add(new LanguageGeneric<string>()
                {
                    languageEnum = language,
                    LanguageGenericType = ""
                });
            }

            if (inDialogueNodePort != null)
            {
                dialogueNodePort.InputGuid = inDialogueNodePort.InputGuid;
                dialogueNodePort.OutputGuid = inDialogueNodePort.OutputGuid;

                if (inDialogueNodePort.PortGuid == "") { inDialogueNodePort.PortGuid = Guid.NewGuid().ToString(); } //NOWE
                dialogueNodePort.PortGuid = inDialogueNodePort.PortGuid; //NOWE

                foreach (LanguageGeneric<string> languageGeneric in inDialogueNodePort.TextLanguage)
                {
                    dialogueNodePort.TextLanguage.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
                }

                foreach (LanguageGeneric<string> languageGeneric in inDialogueNodePort.HintLanguage)
                {
                    dialogueNodePort.HintLanguage.Find(language => language.languageEnum == languageGeneric.languageEnum).LanguageGenericType = languageGeneric.LanguageGenericType;
                }
            }

            return dialogueNodePort;
        }

        public static VisualElement GeneratePortVisualElement(this BaseNode node, Action deleteAction, out TextField textField, out TextField hintField)
        {
            var mainContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };
            Button deleteButton = new(deleteAction)
            {
                text = "X"
            };

            mainContainer.Add(deleteButton);

            var newContainer = new VisualElement()
            {
                style = {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1
                }
            };

            textField = new TextField()
            {
                style = {
                    flexGrow = 0,
                }
            };

            var hintContainer = new VisualElement()
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    alignContent = Align.Stretch,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.FlexEnd
                }
            };

            hintContainer.Add(new Label("Hint")
            {
                style = {
                    alignSelf = Align.Center
                }
            });

            hintField = new TextField()
            {
                style = {
                    flexGrow = 1
                }
            };

            hintContainer.Add(hintField);
            newContainer.Add(textField);
            newContainer.Add(hintContainer);

            mainContainer.Add(newContainer);

            return mainContainer;
        }
    }
}