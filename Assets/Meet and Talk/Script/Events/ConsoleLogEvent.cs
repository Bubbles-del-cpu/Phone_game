using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeetAndTalk.Event
{
    [CreateAssetMenu(menuName = "Dialogue/Event/Console Log")]
    public class ConsoleLogEvent : DialogueEventSO
    {
        #region Variables
        public LogType logType;
        public string Content;
        #endregion

        /// <summary>.
        /// The RunEvent function is called by the Event Node
        /// It can also be called manually
        /// </summary>.
        public override void RunEvent()
        {
            // Przyk��d Wywo�ania Funkcji Kt�ra znajduje si� w DialogueEventManager
            DialogueEventManager.Instance.ConsoleLogEvent(Content, logType);
        }
        public override void RollbackEvent()
        {
            //Blank
        }
    }

    public enum LogType
    {
        Info = 0, Warning = 1, Error = 2
    }
}
