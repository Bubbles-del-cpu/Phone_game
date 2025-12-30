using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SocialMediaPostSO;

public class SocialMediaPostComment : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _comment;
    [SerializeField] private Color _color1;
    [SerializeField] private Color _color2;
    [SerializeField] private SocialMediaComment _socialMediaComment;

    /// <summary>
    /// Populates the social media comment with content
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="index"></param>
    public void SetComment(SocialMediaComment comment, int index)
    {
        _socialMediaComment = comment;
        if (_socialMediaComment != null)
        {
            _name.text = _socialMediaComment.Name;
            _comment.text = _socialMediaComment.Content;


            //Alternate the background colour so everything is easier to read
            _background.color = index % 2 == 0 ? _color1 : _color2;
        }
    }
}
