using System.Collections.Generic;
using UnityEngine;

namespace MeetAndTalk
{
    public class DialogueGetData : MonoBehaviour
    {
        [HideInInspector] public DialogueContainerSO dialogueContainer;
        public Dictionary<string, (BaseNodeData, BaseNodeData)> NodeDataDictionary = new Dictionary<string, (BaseNodeData, BaseNodeData)>();

        protected void PopulateDictionary()
        {
            NodeDataDictionary.Clear();
            foreach (var node in dialogueContainer.AllNodes)
            {
                NodeDataDictionary[node.NodeGuid] = (node, null);
            }

            foreach (var link in dialogueContainer.NodeLinkDatas)
            {
                if (!NodeDataDictionary.ContainsKey(link.BaseNodeGuid))
                    continue;

                NodeDataDictionary[link.BaseNodeGuid] = (NodeDataDictionary[link.BaseNodeGuid].Item1, GetNodeByGuid(link.TargetNodeGuid));
            }
        }

        public BaseNodeData GetNodeByGuid(string targetNodeGUID)
        {
            return NodeDataDictionary.ContainsKey(targetNodeGUID) ? NodeDataDictionary[targetNodeGUID].Item1 : null;
        }

        public BaseNodeData GetNextNode(BaseNodeData nodeData)
        {
            return NodeDataDictionary.ContainsKey(nodeData.NodeGuid) ? NodeDataDictionary[nodeData.NodeGuid].Item2 : null;
        }
    }
}